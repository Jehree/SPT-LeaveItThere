using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeaveItThere.UI;

internal class MoveModeUI : MonoBehaviour
{
    private static MoveModeUI _instance = null;
    public static MoveModeUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Instantiate(Prefab).AddComponent<MoveModeUI>();
            }
            return _instance;
        }
    }

    private static GameObject _prefab = null;
    public static GameObject Prefab
    {
        get
        {
            if (_prefab == null)
            {
                _prefab = BundleLoader.LoadAsset<GameObject>(AssetBundle, "EditPlacedItemMenu");
            }
            
            return _prefab;
        }
    }

    private static AssetBundle _assetBundle = null;
    public static AssetBundle AssetBundle
    {
        get
        {
            if (_assetBundle == null)
            {
                _assetBundle = BundleLoader.LoadBundle("editplaceditemmenu.menu");
            }
            
            return _assetBundle;
        }
    }

    public enum ERecolorTarget
    {
        Base,
        Highlight,
        Pressed,
    }

    public enum ETabType
    {
        None = -1,
        Position,
        Rotation,
        Physics
    }

    public enum ESpaceReference
    {
        Player,
        Item,
        World
    }

    public ESpaceReference RepositionReference
    {
        get
        {
            if (PosTab.MoveRelativeTo.value == 0) return ESpaceReference.Player;
            if (PosTab.MoveRelativeTo.value == 1) return ESpaceReference.Item;
            return ESpaceReference.World;
        }
    }

    public ESpaceReference RotationReference
    {
        get
        {
            if (RotTab.RotateRelativeTo.value == 0) return ESpaceReference.Player;
            if (RotTab.RotateRelativeTo.value == 1) return ESpaceReference.Item;
            return ESpaceReference.World;
        }
    }

    public MenuTab SelectedTab;
    public ETabType SelectedTabType
    {
        get
        {
            if (SelectedTab is PositionTab) return ETabType.Position;
            else if (SelectedTab is RotationTab) return ETabType.Rotation;
            else if (SelectedTab is PhysicsTab) return ETabType.Physics;
            return ETabType.None;
        }
    }

    public Canvas Canvas;
    public Image Background;

    public PositionTab PosTab;
    public RotationTab RotTab;
    public PhysicsTab PhysTab;
    public List<MenuTab> AllMenuTabs;

    public RectTransform MenuRect;
    public Button DragWindowButton;
    public Button SaveButton;
    public Button CancelButton;

    public delegate void TabSwitchedHandler(ETabType tabType);
    public event TabSwitchedHandler TabSwitched;

    public bool IsActive { get => gameObject.activeSelf; }

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
        Background = gameObject.transform.Find("TabGroup/bg").gameObject.GetComponent<Image>();
        SaveButton = gameObject.transform.Find("TabGroup/ExitButtons/SaveButton").gameObject.GetComponent<Button>();
        CancelButton = gameObject.transform.Find("TabGroup/ExitButtons/CancelButton").gameObject.GetComponent<Button>();

        gameObject.SetActive(false);
    }

    internal void Awake()
    {
        PosTab.Activate();

        PosTab.TabButtonClicked += OnTabButtonClicked;
        RotTab.TabButtonClicked += OnTabButtonClicked;
        PhysTab.TabButtonClicked += OnTabButtonClicked;
    }

    internal void OnEnable()
    {
        UIColorHelper.RefreshColors();
    }

    public void OnTabButtonClicked(MenuTab tab)
    {
        ChangeTabs(tab.GetType());
    }

    public void ChangeTabs(Type tabType)
    {
        if (SelectedTab.GetType() == tabType) return;
        SelectedTab.Deactivate();

        MenuTab tab = AllMenuTabs.First(t => t.GetType() == tabType);
        tab.Activate();
        TabSwitched?.Invoke(SelectedTabType);

        UIColorHelper.RefreshColors();
    }

    public void ChangeTabs<T>()
    {
        ChangeTabs(typeof(T));
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