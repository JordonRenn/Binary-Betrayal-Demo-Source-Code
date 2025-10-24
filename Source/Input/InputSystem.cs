using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using BinaryBetrayal.DataManagement;

/* 
Set to replace the old InputHandler with the new InputSystem.cs.
Static class instead of MonoBehaviour singleton.
- Added global Map for inputs that should always be active (e.g., pause menu, dev options).
    - No longer supports open menu (Player, Pause, etc..) actions in other maps (e.g., FirstPerson).
- Uses native C# events (Action) instead of UnityEvents
- Loads InputActionAsset via Resources.Load

To Do:
- Implement rebinding and saving/loading bindings
 */
namespace BinaryBetrayal.InputManagement
{
    public enum InputState
    {
        FirstPerson,
        Focus,
        Cutscene,
        UI
    }
    
    public static class InputSystem
    {
        private static InputState defaultState = InputState.FirstPerson;
        public static InputState currentState { get; private set; } = defaultState;

        // settings
        private static float horizontalSensitivityMultiplier = 1.0f;
        private static float verticalSensitivityMultiplier = 1.0f;
        private static bool invertYAxis = false;

        // Input Action Asset and Maps
        private static InputActionAsset inputActions;
        private const string NAME_INPUT_ACTION_ASSET = "InputActions";
        private const string PATH_INPUT_ACTION_ASSET = "Input"; // in "Resources" folder
        private static InputActionMap map_FirstPerson;
        private const string NAME_MAP_FIRST_PERSON = "FirstPerson";
        private static InputActionMap map_Focus;
        private const string NAME_MAP_FOCUS = "Focus";
        private static InputActionMap map_CutScene;
        private const string NAME_MAP_CUTSCENE = "CutScene";
        private static InputActionMap map_UI;
        private const string NAME_MAP_UI = "UI";
        private static InputActionMap map_Global;
        private const string NAME_MAP_GLOBAL = "Global";

        private static List<InputActionMap> allMaps = new List<InputActionMap>();

        #region First Person Actions
        // First Person Action Events and States
        private const string NAME_ACTION_FP_MOVE = "Move";
        public static Action<Vector2> OnMove_fp;
        public static Vector2 MoveInput { get; private set; }

        private const string NAME_ACTION_FP_LOOK = "Look";
        public static Action<Vector2> OnLook_fp;
        public static Vector2 LookInput { get; private set; }

        private const string NAME_ACTION_FP_WALK_TOGGLE = "WalkToggle";
        public static Action OnWalkToggleDown_fp;
        public static Action OnWalkToggleUp_fp;
        public static bool WalkToggleInput { get; private set; }

        private const string NAME_ACTION_FP_SLOW_WALK = "Slow Walk";
        public static Action OnSlowWalkDown_fp;
        public static Action OnSlowWalkUp_fp;
        public static bool SlowWalkInput { get; private set; }

        private const string NAME_ACTION_FP_CROUCH = "Crouch";
        public static Action OnCrouchDown_fp;
        public static Action OnCrouchUp_fp;
        public static bool CrouchInput { get; private set; }

        private const string NAME_ACTION_FP_JUMP = "Jump";
        public static Action OnJumpDown_fp;
        public static Action OnJumpUp_fp;
        public static bool JumpInput { get; private set; }

        private const string NAME_ACTION_FP_INTERACT = "Interact";
        public static Action OnInteractDown_fp;
        public static Action OnInteractUp_fp;
        public static bool InteractInput { get; private set; }

        private const string NAME_ACTION_FP_EQUIP_WEAPON_PRIMARY = "Equip Weapon Slot 1";
        public static Action OnEquipWeaponPrimaryDown_fp;
        public static Action OnEquipWeaponPrimaryUp_fp;
        public static bool EquipWeaponPrimaryInput { get; private set; }

        private const string NAME_ACTION_FP_EQUIP_WEAPON_SECONDARY = "Equip Weapon Slot 2";
        public static Action OnEquipWeaponSecondaryDown_fp;
        public static Action OnEquipWeaponSecondaryUp_fp;
        public static bool EquipWeaponSecondaryInput { get; private set; }

        private const string NAME_ACTION_FP_EQUIP_MELEE = "Equip Melee";
        public static Action OnEquipMeleeDown_fp;
        public static Action OnEquipMeleeUp_fp;
        public static bool EquipMeleeInput { get; private set; }

        private const string NAME_ACTION_FP_EQUIP_UTILITY = "Equip Utility";
        public static Action OnEquipUtilityDown_fp;
        public static Action OnEquipUtilityUp_fp;
        public static bool EquipUtilityInput { get; private set; }

        private const string NAME_ACTION_FP_EQUIP_UNARMED = "Equip Unarmed";
        public static Action OnEquipUnarmedDown_fp;
        public static Action OnEquipUnarmedUp_fp;
        public static bool EquipUnarmedInput { get; private set; }

        private const string NAME_ACTION_FP_SWAP_EQUIPED_WEAPON = "Weapon Swap";
        public static Action OnSwapEquipedWeaponDown_fp;
        public static Action OnSwapEquipedWeaponUp_fp;
        public static bool SwapEquipedWeaponInput { get; private set; }

        private const string NAME_ACTION_FP_FIRE = "Fire";
        public static Action OnFireDown_fp;
        public static Action OnFireUp_fp;
        public static bool FireInput { get; private set; }

        private const string NAME_ACTION_FP_AIM = "Aim";
        public static Action OnAimDown_fp;
        public static Action OnAimUp_fp;
        public static bool AimInput { get; private set; }

        private const string NAME_ACTION_FP_RELOAD = "Reload";
        public static Action OnReloadDown_fp;
        public static Action OnReloadUp_fp;
        public static bool ReloadInput { get; private set; }
        #endregion

        #region Focus Actions
        private const string NAME_ACTION_FOCUS_POINT = "Point";
        public static Action<Vector2> OnPoint_focus;
        public static Vector2 FocusPointInput { get; private set; }

        private const string NAME_ACTION_FOCUS_CLICK = "Click";
        public static Action OnClickDown_focus;
        public static Action OnClickUp_focus;
        public static bool FocusClickInput { get; private set; }

        private const string NAME_ACTION_FOCUS_INTERACT = "Interact";
        public static Action OnInteractDown_focus;
        public static Action OnInteractUp_focus;
        public static bool FocusInteractInput { get; private set; }

        private const string NAME_ACTION_FOCUS_CANCEL = "Cancel";
        public static Action OnCancelDown_focus;
        public static Action OnCancelUp_focus;
        public static bool FocusCancelInput { get; private set; }

        private const string NAME_ACTION_FOCUS_NUM1 = "Num_1";
        public static Action OnNum1Down_focus;
        public static Action OnNum1Up_focus;
        public static bool FocusNum1Input { get; private set; }

        private const string NAME_ACTION_FOCUS_NUM2 = "Num_2";
        public static Action OnNum2Down_focus;
        public static Action OnNum2Up_focus;
        public static bool FocusNum2Input { get; private set; }

        private const string NAME_ACTION_FOCUS_NUM3 = "Num_3";
        public static Action OnNum3Down_focus;
        public static Action OnNum3Up_focus;
        public static bool FocusNum3Input { get; private set; }

        private const string NAME_ACTION_FOCUS_NUM4 = "Num_4";
        public static Action OnNum4Down_focus;
        public static Action OnNum4Up_focus;
        public static bool FocusNum4Input { get; private set; }

        private const string NAME_ACTION_FOCUS_NUM5 = "Num_5";
        public static Action OnNum5Down_focus;
        public static Action OnNum5Up_focus;
        public static bool FocusNum5Input { get; private set; }

        private const string NAME_ACTION_FOCUS_NUM6 = "Num_6";
        public static Action OnNum6Down_focus;
        public static Action OnNum6Up_focus;
        public static bool FocusNum6Input { get; private set; }

        private const string NAME_ACTION_FOCUS_NUM7 = "Num_7";
        public static Action OnNum7Down_focus;
        public static Action OnNum7Up_focus;
        public static bool FocusNum7Input { get; private set; }

        private const string NAME_ACTION_FOCUS_NUM8 = "Num_8";
        public static Action OnNum8Down_focus;
        public static Action OnNum8Up_focus;
        public static bool FocusNum8Input { get; private set; }

        private const string NAME_ACTION_FOCUS_NUM9 = "Num_9";
        public static Action OnNum9Down_focus;
        public static Action OnNum9Up_focus;
        public static bool FocusNum9Input { get; private set; }

        private const string NAME_ACTION_FOCUS_NUM0 = "Num_0";
        public static Action OnNum0Down_focus;
        public static Action OnNum0Up_focus;
        public static bool FocusNum0Input { get; private set; }

        #endregion

        #region CutScene Actions
        // CutScene Action Events and States
        private const string NAME_ACTION_CUTSCENE_SKIP = "Skip";
        public static Action OnSkipDown_cutscene;
        public static Action OnSkipUp_cutscene;
        public static bool SkipInput { get; private set; }

        private const string NAME_ACTION_CUTSCENE_NEXT = "Next";
        public static Action OnNextDown_cutscene;
        public static Action OnNextUp_cutscene;
        public static bool NextInput { get; private set; }
        #endregion

        #region UI Actions
        // UI Action Events and States
        private const string NAME_ACTION_UI_NAVIGATE = "Navigate";
        public static Action<Vector2> OnNavigate_ui;
        public static Vector2 NavigateInput { get; private set; }

        private const string NAME_ACTION_UI_SUBMIT = "Submit";
        public static Action OnSubmitDown_ui;
        public static Action OnSubmitUp_ui;
        public static bool SubmitInput { get; private set; }

        private const string NAME_ACTION_UI_CANCEL = "Cancel";
        public static Action OnCancelDown_ui;
        public static Action OnCancelUp_ui;
        public static bool CancelInput { get; private set; }

        private const string NAME_ACTION_UI_POINT = "Point";
        public static Action<Vector2> OnPoint_ui;
        public static Vector2 PointInput { get; private set; }

        private const string NAME_ACTION_UI_CLICK = "Click";
        public static Action OnClickDown_ui;
        public static Action OnClickUp_ui;
        public static bool ClickInput { get; private set; }

        private const string NAME_ACTION_UI_RIGHT_CLICK = "RightClick";
        public static Action OnRightClickDown_ui;
        public static Action OnRightClickUp_ui;
        public static bool RightClickInput { get; private set; }

        private const string NAME_ACTION_UI_MIDDLE_CLICK = "MiddleClick";
        public static Action OnMiddleClickDown_ui;
        public static Action OnMiddleClickUp_ui;
        public static bool MiddleClickInput { get; private set; }

        private const string NAME_ACTION_UI_SCROLL = "ScrollWheel";
        public static Action<Vector2> OnScroll_ui;
        public static Vector2 ScrollInput { get; private set; }

        private const string NAME_ACTION_UI_INTERACT = "Interact";
        public static Action OnInteractDown_ui;
        public static Action OnInteractUp_ui;
        public static bool InteractInput_ui { get; private set; }
        #endregion

        #region Global Actions
        private const string NAME_ACTION_GLOBAL_CONSOLE = "Console";
        public static Action OnConsoleDown_global;
        public static Action OnConsoleUp_global;
        public static bool ConsoleInput { get; private set; }

        private const string NAME_ACTION_GLOBAL_F1 = "F1";
        public static Action OnF1Down_global;
        public static Action OnF1Up_global;
        public static bool F1Input { get; private set; }

        private const string NAME_ACTION_GLOBAL_F2 = "F2";
        public static Action OnF2Down_global;
        public static Action OnF2Up_global;
        public static bool F2Input { get; private set; }

        private const string NAME_ACTION_GLOBAL_F3 = "F3";
        public static Action OnF3Down_global;
        public static Action OnF3Up_global;
        public static bool F3Input { get; private set; }

        private const string NAME_ACTION_GLOBAL_F4 = "F4";
        public static Action OnF4Down_global;
        public static Action OnF4Up_global;
        public static bool F4Input { get; private set; }

        private const string NAME_ACTION_GLOBAL_QUICKSAVE = "QuickSave";
        public static Action OnQuickSaveDown_global;
        public static Action OnQuickSaveUp_global;
        public static bool QuickSaveInput { get; private set; }

        private const string NAME_ACTION_GLOBAL_PAUSE_MENU = "Pause Menu";
        public static Action OnPauseMenuDown_global;
        public static Action OnPauseMenuUp_global;
        public static bool PauseMenuInput { get; private set; }

        private const string NAME_ACTION_GLOBAL_PLAYER_MENU = "Player Menu";
        public static Action OnPlayerMenuDown_global;
        public static Action OnPlayerMenuUp_global;
        public static bool PlayerMenuInput { get; private set; }

        private const string NAME_ACTION_GLOBAL_INVENTORY_MENU = "Inventory Menu";
        public static Action OnInventoryDown_global;
        public static Action OnInventoryUp_global;
        public static bool InventoryInput { get; private set; }

        private const string NAME_ACTION_GLOBAL_MAP_MENU = "Map Menu";
        public static Action OnMapDown_global;
        public static Action OnMapUp_global;
        public static bool MapInput { get; private set; }

        private const string NAME_ACTION_GLOBAL_JOURNAL_MENU = "Journal Menu";
        public static Action OnJournalDown_global;
        public static Action OnJournalUp_global;
        public static bool JournalInput { get; private set; }
        #endregion

        static InputSystem()
        {
            LoadInputAsset();
        }

        private static void LoadInputAsset()
        {
            try
            {
                // Load the InputActionAsset from the Resources folder
                string resourcePath = $"{PATH_INPUT_ACTION_ASSET}/{NAME_INPUT_ACTION_ASSET}";
                inputActions = Resources.Load<InputActionAsset>(resourcePath);
                
                if (inputActions == null)
                {
                    Debug.LogError($"Failed to load InputActionAsset from Resources path: {resourcePath}");
                    return;
                }
                
                Debug.Log($"Loaded InputActionAsset from Resources: {resourcePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load InputActionAsset: {e.Message}");
                return;
            }

            map_FirstPerson = inputActions.FindActionMap(NAME_MAP_FIRST_PERSON);
            allMaps.Add(map_FirstPerson);

            map_Focus = inputActions.FindActionMap(NAME_MAP_FOCUS);
            allMaps.Add(map_Focus);

            map_CutScene = inputActions.FindActionMap(NAME_MAP_CUTSCENE);
            allMaps.Add(map_CutScene);

            map_UI = inputActions.FindActionMap(NAME_MAP_UI);
            allMaps.Add(map_UI);

            map_Global = inputActions.FindActionMap(NAME_MAP_GLOBAL);
            map_Global.Enable();

            if (map_FirstPerson == null || map_Focus == null || map_CutScene == null || map_UI == null || map_Global == null)
            {
                Debug.LogError("One or more InputActionMaps not found in the InputActionAsset.");
                return;
            }

            SubscribeToInputEvents();
            Debug.Log("Input system initialized successfully!");
        }

        #region Event Subscription
        private static void SubscribeToInputEvents()
        {
            SubscribeToFirstPersonEvents();
            SubscribeToFocusEvents();
            SubscribeToCutSceneEvents();
            SubscribeToUIEvents();
            SubscribeToGlobalEvents();

            SetInputState(defaultState);
            UpdateInputSettings();
        }

        private static void SubscribeToFirstPersonEvents()
        {
            map_FirstPerson[NAME_ACTION_FP_MOVE].performed += ctx =>
            {
                MoveInput = ctx.ReadValue<Vector2>();
                OnMove_fp?.Invoke(MoveInput);
            };
            map_FirstPerson[NAME_ACTION_FP_MOVE].canceled += ctx =>
            {
                MoveInput = Vector2.zero;
                OnMove_fp?.Invoke(MoveInput);
            };

            map_FirstPerson[NAME_ACTION_FP_LOOK].performed += ctx =>
            {
                LookInput = ctx.ReadValue<Vector2>();
                OnLook_fp?.Invoke(LookInput);
            };
            map_FirstPerson[NAME_ACTION_FP_LOOK].canceled += ctx =>
            {
                LookInput = Vector2.zero;
                OnLook_fp?.Invoke(LookInput);
            };

            map_FirstPerson[NAME_ACTION_FP_WALK_TOGGLE].started += ctx =>
            {
                WalkToggleInput = true;
                OnWalkToggleDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_WALK_TOGGLE].canceled += ctx =>
            {
                WalkToggleInput = false;
                OnWalkToggleUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_SLOW_WALK].started += ctx =>
            {
                SlowWalkInput = true;
                OnSlowWalkDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_SLOW_WALK].canceled += ctx =>
            {
                SlowWalkInput = false;
                OnSlowWalkUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_CROUCH].started += ctx =>
            {
                CrouchInput = true;
                OnCrouchDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_CROUCH].canceled += ctx =>
            {
                CrouchInput = false;
                OnCrouchUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_JUMP].started += ctx =>
            {
                JumpInput = true;
                OnJumpDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_JUMP].canceled += ctx =>
            {
                JumpInput = false;
                OnJumpUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_INTERACT].started += ctx =>
            {
                InteractInput = true;
                OnInteractDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_INTERACT].canceled += ctx =>
            {
                InteractInput = false;
                OnInteractUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_EQUIP_WEAPON_PRIMARY].started += ctx =>
            {
                EquipWeaponPrimaryInput = true;
                OnEquipWeaponPrimaryDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_EQUIP_WEAPON_PRIMARY].canceled += ctx =>
            {
                EquipWeaponPrimaryInput = false;
                OnEquipWeaponPrimaryUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_EQUIP_WEAPON_SECONDARY].started += ctx =>
            {
                EquipWeaponSecondaryInput = true;
                OnEquipWeaponSecondaryDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_EQUIP_WEAPON_SECONDARY].canceled += ctx =>
            {
                EquipWeaponSecondaryInput = false;
                OnEquipWeaponSecondaryUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_EQUIP_MELEE].started += ctx =>
            {
                EquipMeleeInput = true;
                OnEquipMeleeDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_EQUIP_MELEE].canceled += ctx =>
            {
                EquipMeleeInput = false;
                OnEquipMeleeUp_fp?.Invoke();
            };
            map_FirstPerson[NAME_ACTION_FP_EQUIP_UTILITY].started += ctx =>
            {
                EquipUtilityInput = true;
                OnEquipUtilityDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_EQUIP_UTILITY].canceled += ctx =>
            {
                EquipUtilityInput = false;
                OnEquipUtilityUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_EQUIP_UNARMED].started += ctx =>
            {
                EquipUnarmedInput = true;
                OnEquipUnarmedDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_EQUIP_UNARMED].canceled += ctx =>
            {
                EquipUnarmedInput = false;
                OnEquipUnarmedUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_SWAP_EQUIPED_WEAPON].started += ctx =>
            {
                SwapEquipedWeaponInput = true;
                OnSwapEquipedWeaponDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_SWAP_EQUIPED_WEAPON].canceled += ctx =>
            {
                SwapEquipedWeaponInput = false;
                OnSwapEquipedWeaponUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_FIRE].started += ctx =>
            {
                FireInput = true;
                OnFireDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_FIRE].canceled += ctx =>
            {
                FireInput = false;
                OnFireUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_AIM].started += ctx =>
            {
                AimInput = true;
                OnAimDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_AIM].canceled += ctx =>
            {
                AimInput = false;
                OnAimUp_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_RELOAD].started += ctx =>
            {
                ReloadInput = true;
                OnReloadDown_fp?.Invoke();
            };

            map_FirstPerson[NAME_ACTION_FP_RELOAD].canceled += ctx =>
            {
                ReloadInput = false;
                OnReloadUp_fp?.Invoke();
            };
        }

        private static void SubscribeToFocusEvents()
        {
            map_Focus[NAME_ACTION_FOCUS_POINT].performed += ctx =>
            {
                FocusPointInput = ctx.ReadValue<Vector2>();
                OnPoint_focus?.Invoke(FocusPointInput);
            };
            map_Focus[NAME_ACTION_FOCUS_POINT].canceled += ctx =>
            {
                FocusPointInput = Vector2.zero;
                OnPoint_focus?.Invoke(FocusPointInput);
            };

            map_Focus[NAME_ACTION_FOCUS_CLICK].started += ctx =>
            {
                FocusClickInput = true;
                OnClickDown_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_CLICK].canceled += ctx =>
            {
                FocusClickInput = false;
                OnClickUp_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_INTERACT].started += ctx =>
            {
                FocusInteractInput = true;
                OnInteractDown_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_INTERACT].canceled += ctx =>
            {
                FocusInteractInput = false;
                OnInteractUp_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_CANCEL].started += ctx =>
            {
                FocusCancelInput = true;
                OnCancelDown_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_CANCEL].canceled += ctx =>
            {
                FocusCancelInput = false;
                OnCancelUp_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM1].started += ctx =>
            {
                FocusNum1Input = true;
                OnNum1Down_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM1].canceled += ctx =>
            {
                FocusNum1Input = false;
                OnNum1Up_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM2].started += ctx =>
            {
                FocusNum2Input = true;
                OnNum2Down_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM2].canceled += ctx =>
            {
                FocusNum2Input = false;
                OnNum2Up_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM3].started += ctx =>
            {
                FocusNum3Input = true;
                OnNum3Down_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM3].canceled += ctx =>
            {
                FocusNum3Input = false;
                OnNum3Up_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM4].started += ctx =>
            {
                FocusNum4Input = true;
                OnNum4Down_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM4].canceled += ctx =>
            {
                FocusNum4Input = false;
                OnNum4Up_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM5].started += ctx =>
            {
                FocusNum5Input = true;
                OnNum5Down_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM5].canceled += ctx =>
            {
                FocusNum5Input = false;
                OnNum5Up_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM6].started += ctx =>
            {
                FocusNum6Input = true;
                OnNum6Down_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM6].canceled += ctx =>
            {
                FocusNum6Input = false;
                OnNum6Up_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM7].started += ctx =>
            {
                FocusNum7Input = true;
                OnNum7Down_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM7].canceled += ctx =>
            {
                FocusNum7Input = false;
                OnNum7Up_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM8].started += ctx =>
            {
                FocusNum8Input = true;
                OnNum8Down_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM8].canceled += ctx =>
            {
                FocusNum8Input = false;
                OnNum8Up_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM9].started += ctx =>
            {
                FocusNum9Input = true;
                OnNum9Down_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM9].canceled += ctx =>
            {
                FocusNum9Input = false;
                OnNum9Up_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM0].started += ctx =>
            {
                FocusNum0Input = true;
                OnNum0Down_focus?.Invoke();
            };

            map_Focus[NAME_ACTION_FOCUS_NUM0].canceled += ctx =>
            {
                FocusNum0Input = false;
                OnNum0Up_focus?.Invoke();
            };
        }

        private static void SubscribeToCutSceneEvents()
        {
            map_CutScene[NAME_ACTION_CUTSCENE_SKIP].started += ctx =>
            {
                SkipInput = true;
                OnSkipDown_cutscene?.Invoke();
            };

            map_CutScene[NAME_ACTION_CUTSCENE_SKIP].canceled += ctx =>
            {
                SkipInput = false;
                OnSkipUp_cutscene?.Invoke();
            };

            map_CutScene[NAME_ACTION_CUTSCENE_NEXT].started += ctx =>
            {
                NextInput = true;
                OnNextDown_cutscene?.Invoke();
            };

            map_CutScene[NAME_ACTION_CUTSCENE_NEXT].canceled += ctx =>
            {
                NextInput = false;
                OnNextUp_cutscene?.Invoke();
            };
        }

        private static void SubscribeToUIEvents()
        {
            map_UI[NAME_ACTION_UI_NAVIGATE].performed += ctx =>
            {
                NavigateInput = ctx.ReadValue<Vector2>();
                OnNavigate_ui?.Invoke(NavigateInput);
            };
            map_UI[NAME_ACTION_UI_NAVIGATE].canceled += ctx =>
            {
                NavigateInput = Vector2.zero;
                OnNavigate_ui?.Invoke(NavigateInput);
            };

            map_UI[NAME_ACTION_UI_SUBMIT].started += ctx =>
            {
                SubmitInput = true;
                OnSubmitDown_ui?.Invoke();
            };
            map_UI[NAME_ACTION_UI_SUBMIT].canceled += ctx =>
            {
                SubmitInput = false;
                OnSubmitUp_ui?.Invoke();
            };

            map_UI[NAME_ACTION_UI_CANCEL].started += ctx =>
            {
                CancelInput = true;
                OnCancelDown_ui?.Invoke();
            };
            map_UI[NAME_ACTION_UI_CANCEL].canceled += ctx =>
            {
                CancelInput = false;
                OnCancelUp_ui?.Invoke();
            };

            map_UI[NAME_ACTION_UI_POINT].performed += ctx =>
            {
                PointInput = ctx.ReadValue<Vector2>();
                OnPoint_ui?.Invoke(PointInput);
            };
            map_UI[NAME_ACTION_UI_POINT].canceled += ctx =>
            {
                PointInput = Vector2.zero;
                OnPoint_ui?.Invoke(PointInput);
            };

            map_UI[NAME_ACTION_UI_CLICK].started += ctx =>
            {
                ClickInput = true;
                OnClickDown_ui?.Invoke();
            };
            map_UI[NAME_ACTION_UI_CLICK].canceled += ctx =>
            {
                ClickInput = false;
                OnClickUp_ui?.Invoke();
            };

            map_UI[NAME_ACTION_UI_RIGHT_CLICK].started += ctx =>
            {
                RightClickInput = true;
                OnRightClickDown_ui?.Invoke();
            };
            map_UI[NAME_ACTION_UI_RIGHT_CLICK].canceled += ctx =>
            {
                RightClickInput = false;
                OnRightClickUp_ui?.Invoke();
            };

            map_UI[NAME_ACTION_UI_MIDDLE_CLICK].started += ctx =>
            {
                MiddleClickInput = true;
                OnMiddleClickDown_ui?.Invoke();
            };
            map_UI[NAME_ACTION_UI_MIDDLE_CLICK].canceled += ctx =>
            {
                MiddleClickInput = false;
                OnMiddleClickUp_ui?.Invoke();
            };

            map_UI[NAME_ACTION_UI_SCROLL].performed += ctx =>
            {
                ScrollInput = ctx.ReadValue<Vector2>();
                OnScroll_ui?.Invoke(ScrollInput);
            };
            map_UI[NAME_ACTION_UI_SCROLL].canceled += ctx =>
            {
                ScrollInput = Vector2.zero;
                OnScroll_ui?.Invoke(ScrollInput);
            };

            map_UI[NAME_ACTION_UI_INTERACT].started += ctx =>
            {
                InteractInput_ui = true;
                OnInteractDown_ui?.Invoke();
            };
            map_UI[NAME_ACTION_UI_INTERACT].canceled += ctx =>
            {
                InteractInput_ui = false;
                OnInteractUp_ui?.Invoke();
            };
        }

        private static void SubscribeToGlobalEvents()
        {
            map_Global[NAME_ACTION_GLOBAL_CONSOLE].started += ctx =>
            {
                ConsoleInput = true;
                OnConsoleDown_global?.Invoke();
            };
            map_Global[NAME_ACTION_GLOBAL_CONSOLE].canceled += ctx =>
            {
                ConsoleInput = false;
                OnConsoleUp_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_F1].started += ctx =>
            {
                F1Input = true;
                OnF1Down_global?.Invoke();
            };
            map_Global[NAME_ACTION_GLOBAL_F1].canceled += ctx =>
            {
                F1Input = false;
                OnF1Up_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_F2].started += ctx =>
            {
                F2Input = true;
                OnF2Down_global?.Invoke();
            };
            map_Global[NAME_ACTION_GLOBAL_F2].canceled += ctx =>
            {
                F2Input = false;
                OnF2Up_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_F3].started += ctx =>
            {
                F3Input = true;
                OnF3Down_global?.Invoke();
            };
            map_Global[NAME_ACTION_GLOBAL_F3].canceled += ctx =>
            {
                F3Input = false;
                OnF3Up_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_F4].started += ctx =>
            {
                F4Input = true;
                OnF4Down_global?.Invoke();
            };
            map_Global[NAME_ACTION_GLOBAL_F4].canceled += ctx =>
            {
                F4Input = false;
                OnF4Up_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_QUICKSAVE].started += ctx =>
            {
                QuickSaveInput = true;
                OnQuickSaveDown_global?.Invoke();
            };
            map_Global[NAME_ACTION_GLOBAL_QUICKSAVE].canceled += ctx =>
            {
                QuickSaveInput = false;
                OnQuickSaveUp_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_PAUSE_MENU].started += ctx =>
            {
                PauseMenuInput = true;
                OnPauseMenuDown_global?.Invoke();
            };
            map_Global[NAME_ACTION_GLOBAL_PAUSE_MENU].canceled += ctx =>
            {
                PauseMenuInput = false;
                OnPauseMenuUp_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_PLAYER_MENU].started += ctx =>
            {
                PlayerMenuInput = true;
                OnPlayerMenuDown_global?.Invoke();
            };
            map_Global[NAME_ACTION_GLOBAL_PLAYER_MENU].canceled += ctx =>
            {
                PlayerMenuInput = false;
                OnPlayerMenuUp_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_INVENTORY_MENU].started += ctx =>
            {
                InventoryInput = true;
                OnInventoryDown_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_INVENTORY_MENU].canceled += ctx =>
            {
                InventoryInput = false;
                OnInventoryUp_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_MAP_MENU].started += ctx =>
            {
                MapInput = true;
                OnMapDown_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_MAP_MENU].canceled += ctx =>
            {
                MapInput = false;
                OnMapUp_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_JOURNAL_MENU].started += ctx =>
            {
                JournalInput = true;
                OnJournalDown_global?.Invoke();
            };

            map_Global[NAME_ACTION_GLOBAL_JOURNAL_MENU].canceled += ctx =>
            {
                JournalInput = false;
                OnJournalUp_global?.Invoke();
            };
        }
        #endregion

        #region Input State
        /// <summary>
        /// Public method to set the InputState "currentState" and disable/enable the appropriate input actions.
        /// </summary>
        public static void SetInputState(InputState state)
        {
            switch (state)
            {
                case InputState.FirstPerson:
                    CursorLock(true);
                    currentState = InputState.FirstPerson;
                    allMaps.ForEach(map => map.Disable());
                    map_FirstPerson.Enable();
                    map_Global.Enable(); // Global map always active
                    break;

                case InputState.Focus:
                    CursorLock(false);
                    currentState = InputState.Focus;
                    allMaps.ForEach(map => map.Disable());
                    map_Focus.Enable();
                    map_Global.Enable(); // Global map always active
                    break;

                case InputState.Cutscene:
                    CursorLock(false);
                    currentState = InputState.Cutscene;
                    allMaps.ForEach(map => map.Disable());
                    map_CutScene.Enable();
                    map_Global.Enable(); // Global map always active
                    break;

                case InputState.UI:
                    CursorLock(false);
                    currentState = InputState.UI;
                    allMaps.ForEach(map => map.Disable());
                    map_UI.Enable(); // UI Toolkit may need UI map for proper input handling
                    // Global map disabled in UI state like InputHandler did
                    break;
                default:
                    Debug.LogError($"Invalid InputState: {state}");
                    return;
            }
        }
        #endregion

        #region Cursor Lock
        public static void CursorLock(bool state)
        {
            if (state)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        #endregion

        #region Settings
        public static void UpdateInputSettings()
        {
            PlayerSettings settings = GameSettings.GetSettings();
            horizontalSensitivityMultiplier = settings.GetHorizontalSensitivityMultiplier();
            verticalSensitivityMultiplier = settings.GetVerticalSensitivityMultiplier();
            invertYAxis = settings.invertYAxis;
        }

        public static void UpdateBindings()
        {
            // To be implemented: Load saved bindings and apply to inputActions
        }
        #endregion
    }
}