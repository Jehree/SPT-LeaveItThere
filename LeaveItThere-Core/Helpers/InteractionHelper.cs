using Comfort.Common;
using EFT;
using EFT.InputSystem;
using EFT.UI;
using LeaveItThere.Components;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LeaveItThere.Helpers;

public static class InteractionHelper
{
    public static Action RefreshPromptAction = new(RefreshPrompt);
    public static void RefreshPrompt()
    {
        RaidSession session = RaidSession.Instance;
        session.GamePlayerOwner.ClearInteractionState();

        try
        {
            session.GamePlayerOwner.InteractionsChangedHandler();
        }
        catch (System.Exception) { } // sometimes this causes errors, don't really care in those cases so just avoid the exception
    }

    public static void NotificationLong(string message)
    {
        NotificationManagerClass.DisplayMessageNotification(message, EFT.Communications.ENotificationDurationType.Long);
    }

    public static void NotificationLongWarning(string message)
    {
        NotificationManagerClass.DisplayWarningNotification(message, EFT.Communications.ENotificationDurationType.Long);
    }

    public static void NotificationError(string message)
    {
        NotificationLongWarning(message);
        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
    }

    public static void SetCameraRotationLocked(bool enabled)
    {
        Player player = RaidSession.Instance.Player;

        Vector2 fullYawRange = new Vector2(-360f, 360f);
        Vector2 standingPitchRange = new Vector2(-90f, 90f);
        Vector2 pronePitchRange = new Vector2(-16f, 25f);

        if (enabled)
        {
            Vector2 yawLimit = new Vector2(player.MovementContext.Rotation.x, player.MovementContext.Rotation.x);
            Vector2 pitchLimit = new Vector2(player.MovementContext.Rotation.y, player.MovementContext.Rotation.y);
            player.MovementContext.SetRotationLimit(yawLimit, pitchLimit);
        }
        else
        {
            Vector2 pitchLimit;

            if (player.MovementContext.IsInPronePose)
            {
                pitchLimit = pronePitchRange;
            }
            else
            {
                pitchLimit = standingPitchRange;
            }

            player.MovementContext.SetRotationLimit(fullYawRange, pitchLimit);
        }
    }

    private static IEnumerable<ECommand> _allCommands = Enum.GetValues(typeof(ECommand)).Cast<ECommand>();
    public static void SetMostInputsIgnored(bool ignored, IEnumerable<ECommand> except = null)
    {
        if (except == null)
        {
            except = [];
        }

        if (ignored)
        {
            GamePlayerOwner.AddIgnoreInputCommands(_allCommands.Except(except));
        }
        else
        {
            GamePlayerOwner.RemoveIgnoreInputCommands(_allCommands.Except(except));
        }
    }

    internal class InteractionsChangedHandlerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GamePlayerOwner).GetMethod(nameof(GamePlayerOwner.InteractionsChangedHandler));
        }

        [PatchPrefix]
        static bool PatchPrefix()
        {
            return RaidSession.Instance.InteractionsAllowed;
        }
    }
}