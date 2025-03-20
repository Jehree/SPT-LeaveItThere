using EFT.UI;
using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeaveItThere.CustomUI;

internal class MoveModeUI : MonoBehaviour
{
    private static MoveModeUI _instance = null;
    public static MoveModeUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Instantiate(BundleThings.MoveModeUIPrefab).AddComponent<MoveModeUI>();
            }
            return _instance;
        }
    }

    private MoveModeUI()
    {
        PosTab = new(this);
        RotTab = new(this);
        PhysTab = new(this);
        AllMenuTabs = [PosTab, RotTab, PhysTab];

        DragWindowButton = gameObject.transform.Find("TabGroup/StaticButtons/Drag").gameObject.GetComponent<Button>();
        MenuRect = gameObject.transform.Find("TabGroup").gameObject.RectTransform();
        DragWindowButton.gameObject.AddComponent<ButtonDrag>().Init(MenuRect);

        Canvas = gameObject.GetComponent<Canvas>();
        SaveButton = gameObject.transform.Find("TabGroup/ExitButtons/SaveButton").gameObject.GetComponent<Button>();
        CancelButton = gameObject.transform.Find("TabGroup/ExitButtons/CancelButton").gameObject.GetComponent<Button>();

        gameObject.SetActive(false);
    }

    public enum ERecolorTarget
    {
        Base,
        Highlight,
        Pressed,
    }

    public Canvas Canvas;

    public PositionTab PosTab;
    public RotationTab RotTab;
    public PhysicsTab PhysTab;
    public MenuTab SelectedTab;
    public List<MenuTab> AllMenuTabs;

    public RectTransform MenuRect;
    public Button DragWindowButton;
    public Button SaveButton;
    public Button CancelButton;

    public event MenuTab.TabButtonClickedHandler TabSwitched;

    public bool IsActive { get => gameObject.activeSelf; }

    private void Awake()
    {
        PosTab.Activate();
        UIColorHelper.RefreshColors();

        PosTab.TabButtonClicked += OnTabButtonClicked;
        RotTab.TabButtonClicked += OnTabButtonClicked;
        PhysTab.TabButtonClicked += OnTabButtonClicked;
    }

    private void OnEnable()
    {
        gameObject.transform.SetAsLastSibling();
        Canvas.sortingOrder = 10;
    }

    public void OnTabButtonClicked(MenuTab tab)
    {
        ChangeTabs(tab.GetType());
    }

    public void ChangeTabs<TNewTab>() where TNewTab : MenuTab
    {
        ChangeTabs(typeof(TNewTab));
    }

    public void ChangeTabs(Type tabType)
    {
        if (SelectedTab.GetType() == tabType) return;
        SelectedTab.Deactivate();

        MenuTab tab = AllMenuTabs.First(t => t.GetType() == tabType);
        tab.Activate();
        TabSwitched?.Invoke(tab);

        UIColorHelper.RefreshColors();
    }

    public void ToggleActive()
    {
        gameObject.SetActive(!IsActive);
    }

    public class PhysicsTab : MenuTab
    {
        public Toggle ItemFloats;

        public PhysicsTab(MoveModeUI ui)
        {
            TabButton = ui.gameObject.transform.Find("TabGroup/TabButtons/PhysicsTab").gameObject.GetComponent<Button>();
            Content = ui.gameObject.transform.Find("TabGroup/PhysicsContent").gameObject;
            GameObject vContainer = Content.transform.Find("VContainer").gameObject;

            ItemFloats = vContainer.transform.Find("ItemFloats").gameObject.GetComponent<Toggle>();

            SelectablesOnTab = [ItemFloats];
            Init();
        }
    }

    public class RotationTab : MenuTab
    {
        public TMP_Dropdown RotateRelativeTo;
        public Toggle LockX;
        public Toggle LockY;
        public Toggle LockZ;
        public Button ResetRotationButton;
        public Button UndoRotationButton;

        public RotationTab(MoveModeUI ui)
        {
            TabButton = ui.gameObject.transform.Find("TabGroup/TabButtons/RotationTab").gameObject.GetComponent<Button>();
            Content = ui.gameObject.transform.Find("TabGroup/RotationContent").gameObject;
            GameObject vContainer = Content.transform.Find("VContainer").gameObject;

            RotateRelativeTo = vContainer.transform.Find("RotateRelativeTo").gameObject.GetComponent<TMP_Dropdown>();
            LockX = vContainer.transform.Find("LockX").gameObject.GetComponent<Toggle>();
            LockY = vContainer.transform.Find("LockY").gameObject.GetComponent<Toggle>();
            LockZ = vContainer.transform.Find("LockZ").gameObject.GetComponent<Toggle>();
            ResetRotationButton = vContainer.transform.Find("ResetRotationButton").gameObject.GetComponent<Button>();
            UndoRotationButton = vContainer.transform.Find("UndoRotationButton").gameObject.GetComponent<Button>();

            SelectablesOnTab = [RotateRelativeTo, LockX, LockY, LockZ, ResetRotationButton, UndoRotationButton];
            Init();
        }
    }

    public class PositionTab : MenuTab 
    {
        public TMP_Dropdown MoveRelativeTo;
        public Toggle LockX;
        public Toggle LockY;
        public Toggle LockZ;
        public Button MoveToPlayerButton;
        public Button UndoMoveButton;

        public PositionTab(MoveModeUI ui)
        {
            TabButton = ui.gameObject.transform.Find("TabGroup/TabButtons/PositionTab").gameObject.GetComponent<Button>();
            Content = ui.gameObject.transform.Find("TabGroup/PositionContent").gameObject;
            GameObject vContainer = Content.transform.Find("VContainer").gameObject;

            MoveRelativeTo = vContainer.transform.Find("MoveRelativeTo").gameObject.GetComponent<TMP_Dropdown>();
            LockX = vContainer.transform.Find("LockX").gameObject.GetComponent<Toggle>();
            LockY = vContainer.transform.Find("LockY").gameObject.GetComponent<Toggle>();
            LockZ = vContainer.transform.Find("LockZ").gameObject.GetComponent<Toggle>();
            MoveToPlayerButton = vContainer.transform.Find("MoveToPlayerButton").gameObject.GetComponent<Button>();
            UndoMoveButton = vContainer.transform.Find("UndoMoveButton").gameObject.GetComponent<Button>();

            SelectablesOnTab = [MoveRelativeTo, LockX, LockY, LockZ, MoveToPlayerButton, UndoMoveButton];
            Init();
        }
    }


    public abstract class MenuTab
    {
        public GameObject Content;
        public Button TabButton; // has color set separately
        public List<Selectable> SelectablesOnTab = [];

        public delegate void TabButtonClickedHandler(MenuTab tab);
        public event TabButtonClickedHandler TabButtonClicked;

        public void Init()
        {
            TabButton.onClick.AddListener(ButtonClicked);
        }

        private void ButtonClicked()
        {
            TabButtonClicked?.Invoke(this);
        }

        public void Activate()
        {
            Content.SetActive(true);
            Instance.SelectedTab = this;
        }

        public void Deactivate()
        {
            Content.SetActive(false);
        }
    }
}
