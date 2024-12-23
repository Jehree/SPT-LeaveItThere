using Comfort.Common;
using EFT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LeaveItThere.Helpers
{
    internal class Utils
    {
        public static string GetCardinalDirection(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            direction.y = 0;
            direction.Normalize();
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;

            string locId = Singleton<GameWorld>.Instance.LocationId;
            if (locId == "factory4_day" || locId == "factory4_night")
            {
                if (angle >= 337.5 || angle < 22.5) return "South";
                if (angle >= 22.5 && angle < 67.5) return "South East";
                if (angle >= 67.5 && angle < 112.5) return "East";
                if (angle >= 112.5 && angle < 157.5) return "North East";
                if (angle >= 157.5 && angle < 202.5) return "North";
                if (angle >= 202.5 && angle < 247.5) return "North West";
                if (angle >= 247.5 && angle < 292.5) return "West";
                if (angle >= 292.5 && angle < 337.5) return "South West";
            }
            else
            {
                if (angle >= 337.5 || angle < 22.5) return "East";
                if (angle >= 22.5 && angle < 67.5) return "North East";
                if (angle >= 67.5 && angle < 112.5) return "North";
                if (angle >= 112.5 && angle < 157.5) return "North West";
                if (angle >= 157.5 && angle < 202.5) return "West";
                if (angle >= 202.5 && angle < 247.5) return "South West";
                if (angle >= 247.5 && angle < 292.5) return "South";
                if (angle >= 292.5 && angle < 337.5) return "South East";
            }


            return "this shouldn't ever be reached";
        }

        public static void ExecuteAfterSeconds(int seconds, Action callback)
        {
            StaticManager.BeginCoroutine(ExecuteAfterSecondsRoutine(seconds, callback));
        }

        public static IEnumerator ExecuteAfterSecondsRoutine(int seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback();
        }

        public static void ExecuteNextFrame(Action callback)
        {
            StaticManager.BeginCoroutine(ExecuteNextFrameRoutine(callback));
        }

        public static IEnumerator ExecuteNextFrameRoutine(Action callback)
        {
            yield return null;
            callback();
        }
    }
}
