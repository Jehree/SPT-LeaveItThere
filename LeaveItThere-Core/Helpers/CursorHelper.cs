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

    public static void SetCursor(ECursorType type)
    {
        GClass3746.SetCursor(type);
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

    public class InputManagerUpdatePatch : ModulePatch
    {
        private static FieldInfo _cursorResultField;

        protected override MethodBase GetTargetMethod()
        {
            // name is ecursorResult_0 in the InputManager class
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