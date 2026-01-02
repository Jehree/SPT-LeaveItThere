using Comfort.Common;
using EFT;
using LeaveItThere.Components;
using Newtonsoft.Json;
using SPT.Common.Http;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LeaveItThere.Helpers;

public class LITUtils
{
    public static string AssemblyPath { get; private set; } = Assembly.GetExecutingAssembly().Location;
    public static string AssemblyFolderPath { get; private set; } = Path.GetDirectoryName(AssemblyPath);

    public static T ServerRoute<T>(string url, T data = default)
    {
        string json = JsonConvert.SerializeObject(data);
        Plugin.LogSource.LogError(json);
        string req = RequestHandler.PostJson(url, json);
        return JsonConvert.DeserializeObject<T>(req);
    }

    public static Vector3 PlayerFront
    {
        get
        {
            Player player = RaidSession.Instance.Player;
            return player.Transform.Original.position + player.Transform.Original.forward + (player.Transform.Original.up / 2);
        }
    }

    public static Vector3 CameraForward
    {
        get
        {
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0;
            return forward;
        }
    }

    public static List<GameObject> GetAllDescendants(GameObject parent)
    {
        List<GameObject> descendants = [];

        foreach (Transform child in parent.transform)
        {
            descendants.Add(child.gameObject);
            descendants.AddRange(GetAllDescendants(child.gameObject));
        }

        return descendants;
    }

    public static string GetCardinalDirection(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        direction.y = 0;
        direction.Normalize();
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        
        string locId = RaidSession.Instance.GameWorld.LocationId;
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
}
