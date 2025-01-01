using HarmonyLib;
using LeaveItThere.Components;
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

        public static void RefreshPrompt(bool force = false)
        {
            var session = ModSession.Instance;
            if (force)
            {
                session.GamePlayerOwner.AvailableInteractionState.Value = null;
            }
            else
            {
                session.GamePlayerOwner.ClearInteractionState();
            }
        }

        public static void NotificationLong(string message)
        {
            NotificationManagerClass.DisplayMessageNotification(message, EFT.Communications.ENotificationDurationType.Long);
        }
        public static void NotificationLongWarning(string message)
        {
            NotificationManagerClass.DisplayWarningNotification(message, EFT.Communications.ENotificationDurationType.Long);
        }

        public static void SetCameraRotationLocked(bool enabled)
        {
            var player = ModSession.Instance.Player;

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
    }
}
