using LeaveItThere.Helpers;
using UnityEngine;
using UnityEngine.UI;
using static LeaveItThere.CustomUI.MoveModeUI;

namespace LeaveItThere.CustomUI
{
    internal static class UIColorHelper
    {
        public static void RefreshColors()
        {
            MoveModeUI ui = Instance;
            Color buttonColor = GetButtonColor(ui.SelectedTab);

            foreach (var tab in ui.AllMenuTabs)
            {
                bool isSelected = tab == ui.SelectedTab;
                SetSelectableColor(tab.TabButton, GetTabButtonColor(isSelected, tab), ERecolorTarget.Base);
                SetSelectableColor(tab.TabButton, Settings.HighlightColor.Value, ERecolorTarget.Highlight);
                SetSelectableColor(tab.TabButton, Settings.ClickColor.Value, ERecolorTarget.Pressed);
            }

            SetSelectableColor(ui.SaveButton, buttonColor, ERecolorTarget.Base);
            SetSelectableColor(ui.SaveButton, Settings.HighlightColor.Value, ERecolorTarget.Highlight);
            SetSelectableColor(ui.SaveButton, Settings.ClickColor.Value, ERecolorTarget.Pressed);

            SetSelectableColor(ui.CancelButton, buttonColor, ERecolorTarget.Base);
            SetSelectableColor(ui.CancelButton, Settings.HighlightColor.Value, ERecolorTarget.Highlight);
            SetSelectableColor(ui.CancelButton, Settings.ClickColor.Value, ERecolorTarget.Pressed);

            SetSelectableColor(ui.DragWindowButton, buttonColor, ERecolorTarget.Base);
            SetSelectableColor(ui.DragWindowButton, Settings.HighlightColor.Value, ERecolorTarget.Highlight);
            SetSelectableColor(ui.DragWindowButton, Settings.ClickColor.Value, ERecolorTarget.Pressed);

            SetTabColor(ui.PosTab, Settings.PositionTabColor.Value, ERecolorTarget.Base);
            SetTabColor(ui.RotTab, Settings.RotationTabColor.Value, ERecolorTarget.Base);
            SetTabColor(ui.PhysTab, Settings.PhysicsTabColor.Value, ERecolorTarget.Base);

            SetTabColor(ui.PosTab, Settings.HighlightColor.Value, ERecolorTarget.Highlight);
            SetTabColor(ui.RotTab, Settings.HighlightColor.Value, ERecolorTarget.Highlight);
            SetTabColor(ui.PhysTab, Settings.HighlightColor.Value, ERecolorTarget.Highlight);

            SetTabColor(ui.PosTab, Settings.ClickColor.Value, ERecolorTarget.Pressed);
            SetTabColor(ui.RotTab, Settings.ClickColor.Value, ERecolorTarget.Pressed);
            SetTabColor(ui.PhysTab, Settings.ClickColor.Value, ERecolorTarget.Pressed);

            ui.Background.color = Settings.BackgroundColor.Value;
        }

        public static Color GetButtonColor(MenuTab selectedTab)
        {
            if (selectedTab is PositionTab) return Settings.PositionTabColor.Value;
            if (selectedTab is RotationTab) return Settings.RotationTabColor.Value;
            if (selectedTab is PhysicsTab) return Settings.PhysicsTabColor.Value;
            return Color.gray;
        }

        public static Color GetTabButtonColor(bool isSelected, MenuTab tab)
        {
            if (!isSelected) return Color.gray;
            if (tab is PositionTab) return Settings.PositionTabColor.Value;
            if (tab is RotationTab) return Settings.RotationTabColor.Value;
            if (tab is PhysicsTab) return Settings.PhysicsTabColor.Value;
            return Color.gray;
        }

        public static void SetSelectableColor(Selectable selectable, Color color, ERecolorTarget target)
        {
            ColorBlock colorBlock = selectable.colors;

            if (target == ERecolorTarget.Base)
            {
                colorBlock.normalColor = color;
                colorBlock.selectedColor = color;
            }

            if (target == ERecolorTarget.Highlight)
            {
                colorBlock.highlightedColor = color;
            }

            if (target == ERecolorTarget.Pressed)
            {
                colorBlock.pressedColor = color;
            }

            selectable.colors = colorBlock;
        }

        public static void SetTabColor(MenuTab tab, Color color, ERecolorTarget target)
        {
            tab.SelectablesOnTab.ExecuteForEach(sel =>
            {
                SetSelectableColor(sel, color, target);
            });
        }
    }
}
