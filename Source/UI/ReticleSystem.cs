using UnityEngine;
using UnityEngine.UIElements;

/* 
UI CLASS REF GUIDE:

.reticle
   .reticle__center-dot
   .reticle__line-top
   .reticle__line-right
   .reticle__line-bottom
   .reticle__line-left

RETICLE:
used for colors and border
do not change sizes/radius here

CENTER DOT:
used for sizes and radius
always make size width and height the same
always make sure radius is half of width and height
do not change colors here

TOP LINE:
"bottom:" used to move the top line away from the center dot
"height:" represents the "length" of the lines
"width:" represents the "thickness" of the lines

RIGHT LINE:
"left:" used to move the right line away from the center dot
"width:" represents the "length" of the lines
"height:" represents the "thickness" of the lines

BOTTOM LINE:
"top:" used to move the bottom line away from the center dot
"height:" represents the "length" of the lines
"width:" represents the "thickness" of the lines

LEFT LINE:
"right:" used to move the left line away from the center dot
"width:" represents the "length" of the lines
"height:" represents the "thickness" of the lines

 */
namespace BinaryBetrayal.UI
{
    #region Enums
    public enum ReticleState
    {
        Disabled,
        Weapon,
        Interact,
        Reload,
        Stealth
    }
    #endregion

    #region Configs
    [System.Serializable]
    public class ReticleStateConfig
    {
        public Color reticleColor = Color.white;
        public float centerDotSize = 4f;
        public float reticleSpacing = 10f;
        public float reticleLineThickness = 2f;
        public float reticleLineLength = 13f;
        public float reticleLineBorderWidth = 0f;
        public Color reticleLineColor = Color.white;
    }
    #endregion

    #region Reticle System
    public class ReticleSystem : MonoBehaviour
    {
        private static ReticleSystem _instance;
        public static ReticleSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogWarning($"Attempting to access ReticleSystem before initialization.");
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [SerializeField] private UIDocument reticleUIDocument;
        private VisualElement root => reticleUIDocument.rootVisualElement;

        [SerializeField][Range(20f, 120f)] private float reticleMaxSpread = 60f;
        [SerializeField] private AnimationCurve impulseCurve;

        [SerializeField] private ReticleStateConfig weaponConfig;
        [SerializeField] private ReticleStateConfig interactConfig;
        [SerializeField] private ReticleStateConfig reloadConfig;
        [SerializeField] private ReticleStateConfig stealthConfig;

        private ReticleStateConfig currentConfig;
        private ReticleState currentState;
        private const ReticleState DEFAULT_STATE = ReticleState.Weapon;

        private const string NAME_CENTER = "Center";
        private const string NAME_TOP = "Top";
        private const string NAME_RIGHT = "Right";
        private const string NAME_BOTTOM = "Bottom";
        private const string NAME_LEFT = "Left";
        private const string NAME_HOVER_TEXT = "HoverText";

        public Label HoverInfoText => _hoverInfoText;
        private Label _hoverInfoText;

        // Cached UI elements
        private VisualElement _centerDot;
        private VisualElement _lineTop;
        private VisualElement _lineRight;
        private VisualElement _lineBottom;
        private VisualElement _lineLeft;

        private bool _initialized = false;

        void Awake()
        {
            if (this.InitializeSingleton(ref _instance, true) != this) return;

            // reticleUIDocument = GetComponent<UIDocument>();
            if (reticleUIDocument == null)
            {
                SBGDebug.LogError("UIDocument component not found on ReticleSystem", "ReticleSystem | Awake");
                enabled = false;
                return;
            }
            else
            {
                impulseCurve.preWrapMode = WrapMode.Clamp;
                impulseCurve.postWrapMode = WrapMode.Clamp;
                SBGDebug.LogInfo("Using assigned impulse curve", "ReticleSystem | Awake");
            }



            InitializeUIElements();
        }

        private void InitializeUIElements()
        {
            var root = reticleUIDocument.rootVisualElement;

            _centerDot = root.Q<VisualElement>(NAME_CENTER);
            _lineTop = root.Q<VisualElement>(NAME_TOP);
            _lineRight = root.Q<VisualElement>(NAME_RIGHT);
            _lineBottom = root.Q<VisualElement>(NAME_BOTTOM);
            _lineLeft = root.Q<VisualElement>(NAME_LEFT);
            _hoverInfoText = root.Q<Label>(NAME_HOVER_TEXT);

            _initialized = true;

            SetState(DEFAULT_STATE);
        }

        private void Update()
        {
            if (!_isImpulseActive || !_initialized) return;

            float progress = Mathf.Clamp01((Time.time - _impulseStartTime) / _impulseDuration);

            if (progress >= 1f)
            {
                _isImpulseActive = false;
            }

            UpdateReticlePositions();
        }

        #region Public API
        private float _impulseStartTime;
        private float _impulseDuration;
        private float _currentIntensity;
        private bool _isImpulseActive;

        public void Impulse(float duration, float intensity)
        {
            _impulseStartTime = Time.time;
            _impulseDuration = duration;
            _currentIntensity = intensity;
            _isImpulseActive = true;

            SBGDebug.LogInfo($"Starting impulse - Duration: {duration}, Intensity: {intensity}, MaxSpread: {reticleMaxSpread}", "ReticleSystem | Impulse");
        }

        public ReticleStateConfig GetCurrentConfig()
        {
            return currentConfig;
        }

        public void SetState(ReticleState state)
        {
            SetReticleState(state);
        }

        public void SetReticleColor(Color color)
        {
            if (currentConfig == null) return;
            currentConfig.reticleColor = color;
            UpdateReticleUI();
        }

        public void SetBorderWidth(float width)
        {
            if (currentConfig == null) return;
            currentConfig.reticleLineBorderWidth = Mathf.Max(0f, width);
            UpdateReticleUI();
        }

        public void SetBorderColor(Color color)
        {
            if (currentConfig == null) return;
            currentConfig.reticleLineColor = color;
            UpdateReticleUI();
        }
        #endregion

        #region Private API
        private void SetReticleState(ReticleState state)
        {
            if (currentState == state) return;

            currentState = state;
            SBGDebug.LogInfo($"Reticle State Changed to: {state}", "ReticleSystem | SetReticleState");

            if (state == ReticleState.Disabled)
            {
                root.style.display = DisplayStyle.None;
                currentConfig = null;
            }
            else
            {
                root.style.display = DisplayStyle.Flex;
                currentConfig = GetConfigForState(state);
            }

            if (currentConfig != null)
            {
                UpdateReticleUI();
                UpdateReticlePositions();
            }
        }

        private ReticleStateConfig GetConfigForState(ReticleState state)
        {
            return state switch
            {
                ReticleState.Weapon => weaponConfig,
                ReticleState.Interact => interactConfig,
                ReticleState.Reload => reloadConfig,
                ReticleState.Stealth => stealthConfig,
                _ => null
            };
        }

        private void UpdateCenterDot()
        {
            float halfSize = currentConfig.centerDotSize / 2;
            _centerDot.style.width = currentConfig.centerDotSize;
            _centerDot.style.height = currentConfig.centerDotSize;
            _centerDot.style.borderTopLeftRadius = halfSize;
            _centerDot.style.borderTopRightRadius = halfSize;
            _centerDot.style.borderBottomLeftRadius = halfSize;
            _centerDot.style.borderBottomRightRadius = halfSize;
            _centerDot.style.backgroundColor = currentConfig.reticleColor;
        }

        private void UpdateReticleLine(VisualElement line, bool isVertical)
        {
            if (isVertical)
            {
                line.style.width = currentConfig.reticleLineThickness;
                line.style.height = currentConfig.reticleLineLength;
            }
            else
            {
                line.style.width = currentConfig.reticleLineLength;
                line.style.height = currentConfig.reticleLineThickness;
            }

            line.style.backgroundColor = currentConfig.reticleColor;

            if (currentConfig.reticleLineBorderWidth > 0)
            {
                line.style.borderTopWidth = currentConfig.reticleLineBorderWidth;
                line.style.borderRightWidth = currentConfig.reticleLineBorderWidth;
                line.style.borderBottomWidth = currentConfig.reticleLineBorderWidth;
                line.style.borderLeftWidth = currentConfig.reticleLineBorderWidth;

                line.style.borderTopColor = currentConfig.reticleLineColor;
                line.style.borderRightColor = currentConfig.reticleLineColor;
                line.style.borderBottomColor = currentConfig.reticleLineColor;
                line.style.borderLeftColor = currentConfig.reticleLineColor;
            }
        }

        private void UpdateReticleUI()
        {
            if (currentConfig == null) return;

            // Update center dot
            UpdateCenterDot();

            // Update lines
            UpdateReticleLine(_lineTop, true);
            UpdateReticleLine(_lineRight, false);
            UpdateReticleLine(_lineBottom, true);
            UpdateReticleLine(_lineLeft, false);
        }

        private void UpdateReticlePositions()
        {
            if (currentConfig == null) return;

            float spread = GetCurrentSpread();
            float baseSpacing = 10f; // From USS file
            float totalSpacing = baseSpacing + spread;

            SBGDebug.LogInfo($"Updating reticle positions - Base: {baseSpacing}, Spread: {spread}, Total: {totalSpacing}", "ReticleSystem | UpdateReticlePositions");

            // Update positioning to spread lines from center
            _lineTop.style.bottom = new Length(totalSpacing, LengthUnit.Pixel);
            _lineRight.style.left = new Length(totalSpacing, LengthUnit.Pixel);
            _lineBottom.style.top = new Length(totalSpacing, LengthUnit.Pixel);
            _lineLeft.style.right = new Length(totalSpacing, LengthUnit.Pixel);
        }

        private float GetCurrentSpread()
        {
            if (!_isImpulseActive) return 0f;

            float progress = Mathf.Clamp01((Time.time - _impulseStartTime) / _impulseDuration);
            float curveValue = impulseCurve.Evaluate(progress);
            return reticleMaxSpread * _currentIntensity * curveValue;
        }

        #endregion
        private void OnDestroy()
        {
            _instance = null;
        }
    }
    #endregion
}