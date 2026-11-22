using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LaunchpadRevamped.Modifiers.Game.Universal;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace LaunchpadRevamped.Networking;

public static class GuesserRpc
{
    // The 0.4.3-gamemodes interop no longer has PlayerVoteArea.TargetPlayerId, so the id member is
    // looked up once by candidate name. Once you know the real name, put it first (or replace TargetId
    // with a plain property access).
    private static readonly string[] IdMemberNames = { "TargetPlayerId", "TargetPlayer", "PlayerId", "TargetId" };
    private static MemberInfo? _idMember;
    private static bool _idResolved;

    /// <summary>
    /// True once the meeting has reached the results / proceeding phase.
    /// Compared by name because MeetingHud.VoteStates is not available in this interop.
    /// </summary>
    public static bool IsOver(this MeetingHud meeting)
    {
        return meeting.state.ToString() is "Results" or "Proceeding";
    }

    /// <summary>
    /// The player id a vote area belongs to, or 255 if the member could not be found.
    /// </summary>
    public static byte TargetId(this PlayerVoteArea area)
    {
        if (!_idResolved)
        {
            _idResolved = true;

            foreach (var name in IdMemberNames)
            {
                _idMember = (MemberInfo?)AccessTools.Property(typeof(PlayerVoteArea), name)
                            ?? AccessTools.Field(typeof(PlayerVoteArea), name);
                if (_idMember != null)
                {
                    break;
                }
            }

            if (_idMember == null)
            {
                var candidates = typeof(PlayerVoteArea)
                    .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => (m is PropertyInfo p && (p.PropertyType == typeof(byte) || p.PropertyType == typeof(sbyte)))
                                || (m is FieldInfo f && (f.FieldType == typeof(byte) || f.FieldType == typeof(sbyte))))
                    .Select(m => m.Name);

                UnityEngine.Debug.LogError("[Guesser] Could not find the player id member on PlayerVoteArea. byte/sbyte members: "
                               + string.Join(", ", candidates));
            }
        }

        object? value = _idMember switch
        {
            PropertyInfo property => property.GetValue(area),
            FieldInfo field => field.GetValue(area),
            _ => null,
        };

        return value == null ? byte.MaxValue : ToByte(value);
    }

    /// <summary>
    /// Turns a player id value into a byte. In this interop the id is an InnerNet.PlayerId wrapper
    /// rather than a plain number, so look for a conversion operator or a numeric member on it.
    /// </summary>
    private static byte ToByte(object value)
    {
        if (value is IConvertible)
        {
            return unchecked((byte)Convert.ToInt32(value));
        }

        var type = value.GetType();
        const BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        static bool IsNumber(Type t) => t == typeof(byte) || t == typeof(sbyte) || t == typeof(int) || t == typeof(uint) || t == typeof(short) || t == typeof(ushort);

        // 1) explicit / implicit conversion operator to a number
        var op = type.GetMethods(all).FirstOrDefault(m =>
            m.IsStatic && (m.Name == "op_Explicit" || m.Name == "op_Implicit")
            && IsNumber(m.ReturnType)
            && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == type);
        if (op != null)
        {
            return unchecked((byte)Convert.ToInt32(op.Invoke(null, new[] { value })));
        }

        // 2) a numeric instance field or property (e.g. "value")
        foreach (var member in type.GetMembers(all))
        {
            switch (member)
            {
                case FieldInfo { IsStatic: false } field when IsNumber(field.FieldType):
                    return unchecked((byte)Convert.ToInt32(field.GetValue(value)));
                case PropertyInfo { CanRead: true } property when IsNumber(property.PropertyType)
                                                                 && property.GetIndexParameters().Length == 0
                                                                 && !(property.GetMethod?.IsStatic ?? false):
                    return unchecked((byte)Convert.ToInt32(property.GetValue(value)));
            }
        }

        UnityEngine.Debug.LogError("[Guesser] Don't know how to read " + type.FullName + ". Members: "
                                   + string.Join(", ", type.GetMembers(all).Select(m => m.MemberType + " " + m.Name)));
        return byte.MaxValue;
    }

    /// <summary>
    /// Runs on every client. A right guess kills the target, a wrong guess kills the guesser.
    /// </summary>
    [MethodRpc((uint)LaunchpadRpc.Guess)]
    public static void RpcGuess(this PlayerControl guesser, byte targetId, ushort roleId)
    {
        var meeting = MeetingHud.Instance;
        if (!meeting || meeting.IsOver())
        {
            return;
        }

        var modifier = guesser.GetModifier<GuesserModifier>();
        if (modifier == null)
        {
            guesser.KickForCheating();
            return;
        }

        var target = GameData.Instance.GetPlayerById(targetId)?.Object;
        if (!target || target == guesser || target.Data.IsDead || !modifier.CanGuessNow)
        {
            return;
        }

        modifier.GuessesUsed++;
        modifier.GuessesThisMeeting++;

        var correct = (ushort)target.Data.Role.Role == roleId;
        KillInMeeting(meeting, correct ? target : guesser);
    }

    /// <summary>
    /// Calls PlayerVoteArea.SetDead whatever its signature is in this interop: every bool parameter
    /// is false except the one named like "dead" (but not "guardian"), which is true.
    /// </summary>
    private static void MarkDead(PlayerVoteArea area)
    {
        var method = typeof(PlayerVoteArea)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.Name == "SetDead" && m.GetParameters().All(p => p.ParameterType == typeof(bool)))
            .OrderByDescending(m => m.GetParameters().Length)
            .FirstOrDefault();

        if (method == null)
        {
            UnityEngine.Debug.LogError("[Guesser] No usable PlayerVoteArea.SetDead overload found. Overloads: "
                                       + string.Join(" | ", typeof(PlayerVoteArea).GetMethods()
                                           .Where(m => m.Name == "SetDead")
                                           .Select(m => string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)))));
            return;
        }

        var args = method.GetParameters()
            .Select(p => (object)(p.Name != null
                                  && p.Name.Contains("dead", StringComparison.OrdinalIgnoreCase)
                                  && !p.Name.Contains("guardian", StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        method.Invoke(area, args);
    }

    private static void KillInMeeting(MeetingHud meeting, PlayerControl victim)
    {
        victim.Die(DeathReason.Kill, false);

        var area = meeting.playerStates.FirstOrDefault(state => state.TargetId() == victim.PlayerId);
        if (area)
        {
            MarkDead(area);
        }

        if (victim.AmOwner)
        {
            meeting.SetForegroundForDead();
        }

        if (AmongUsClient.Instance.AmHost)
        {
            meeting.CheckForEndVoting();
        }
    }
}
