using Comfort.Common;
using EFT;
using EFT.InputSystem;
using EFT.UI;
using HarmonyLib;
using LeaveItThere.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LeaveItThere.Helpers
{
    public static class InteractionHelper
    {
        public static MethodInfo GetInteractiveActionsMethodInfo<TIneractive>()
        {

            return AccessTools.FirstMethod(
                typeof(GetActionsClass),
                method =>
                method.GetParameters()[0].Name == "owner" &&
                method.GetParameters()[1].ParameterType == typeof(TIneractive)
            );
        }

        public static void RefreshPrompt()
        {
            LITSession session = LITSession.Instance;
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

        public static void ErrorPlayerFeedback(string message)
        {
            NotificationLongWarning(message);
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
        }

        public static void SetCameraRotationLocked(bool enabled)
        {
            Player player = LITSession.Instance.Player;

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
            except ??= [];

            if (ignored)
            {
                GamePlayerOwner.AddIgnoreInputCommands(_allCommands.Except(except));
            }
            else
            {
                GamePlayerOwner.RemoveIgnoreInputCommands(_allCommands.Except(except));
            }
        }
    }
}
