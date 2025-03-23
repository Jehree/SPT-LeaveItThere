using EFT.InputSystem;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Helpers.CursorHelper;

public static class CursorHelper
{
    public static bool CursorForceUnlocked { get; private set; } = false;
    private static bool _blockAllInput = false;

    private static readonly Type _cursorType;
    private static readonly MethodInfo _setCursorMethod;

    static CursorHelper()
    {
        _cursorType = PatchConstants.EftTypes.Single(x => x.GetMethod("SetCursor") != null);
        _setCursorMethod = _cursorType.GetMethod("SetCursor");
    }

    public static void SetCursor(ECursorType type)
    {
        _setCursorMethod.Invoke(null, [type]);
    }

    public static void ToggleCursorForceUnlocked(bool blockAllInput = false)
    {
        SetCursorForceUnlocked(!CursorForceUnlocked, blockAllInput);
    }

    public static void SetCursorForceUnlocked(bool unlocked, bool blockAllInput = false)
    {
        if (unlocked)
        {
            ForceUnlockCursor(blockAllInput);
        }
        else
        {
            ReturnCursorControlToEFT();
        }
    }

    public static void ForceUnlockCursor(bool blockAllInput = false)
    {
        _blockAllInput = blockAllInput;
        CursorForceUnlocked = true;
        SetCursor(ECursorType.Idle);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public static void ReturnCursorControlToEFT()
    {
        _blockAllInput = false;
        CursorForceUnlocked = false;
        SetCursor(ECursorType.Invisible);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public class CursorPatch : ModulePatch
    {
        private static FieldInfo _cursorResultField;

        protected override MethodBase GetTargetMethod()
        {
            _cursorResultField = typeof(InputManager).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(f => f.FieldType == typeof(ECursorResult));
            return AccessTools.Method(typeof(InputManager), nameof(InputManager.Update));
        }

        [PatchPrefix]
        static bool PatchPrefix(InputManager __instance)
        {
            if (_blockAllInput) return false;

            if (CursorForceUnlocked)
            {
                _cursorResultField.SetValue(__instance, ECursorResult.ShowCursor);
            }

            return true;
        }
    }
}