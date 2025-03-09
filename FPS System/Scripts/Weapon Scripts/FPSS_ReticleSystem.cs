using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FPSS_ReticleSystem : MonoBehaviour
{
    public static FPSS_ReticleSystem Instance { get; private set; }

    private CharacterMovement characterMovement = null;

    [SerializeField][Range(10,100)] float size;
    float calculatedSize;
    [SerializeField] float adsRetTargetSizeMultiplier;
    float calculatedMultiplier;
    float gunFireMultiplier = 1f; //keep at 1f, dummy
    [SerializeField] float adsRetSpeed;
    [SerializeField] AnimationCurve gunFireCurve;
    [SerializeField] private float gunFireCurveMultiplier;
    Coroutine gunFireCoroutine;
    bool ads;

    static RectTransform reticlePanel;
    GameObject[] reticleElements;
    Image[] reticleimages;
    
    //Rigidbody playerRb;
    CharacterController playerController;
    GameObject player;
    private bool hidden = false;

    [Header("Dev  Options")]
    [Space(10)]

    [SerializeField] private float initTimeout = 5f;
    private bool initizalized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        reticlePanel = GetComponent<RectTransform>();
        FetchReticleElements();

        if (reticlePanel == null)
        {
            Debug.LogError("Reticle panel is not assigned.");
        }

        StartCoroutine(Init());
        GameMaster.Instance.gm_ReticleSystemSpawned.Invoke();
    }

    IEnumerator Init()
        {
            float initTime = Time.time;

            while (player == null && Time.time - initTime < initTimeout)
            {
                player = GameObject.FindWithTag("Player");
                yield return null;
            }

            if (player == null)
            {
                Debug.LogError($"RETICLE SYSTEM | Player object not found. Initialization failed.");
                yield break;
            }

            while (characterMovement == null && Time.time - initTime < initTimeout)
            {
                characterMovement = player.GetComponent<CharacterMovement>();
                yield return null;
            }

            if (characterMovement == null)
            {
                Debug.LogError("$RETICLE SYSTEM | CharacterMovement component not found. Initialization failed.");
                yield break;
            }

            playerController = player.GetComponent<CharacterController>();

            initizalized = true;
        }
    
    void Update()
    {
        if (!initizalized) {return;}

        if (reticlePanel != null)
        {
            float velocityMultiplier = CalculateVelocityMultiplier();
            float adsMultiplier = CalculateADSMultiplier(ads);
            float gunFireMultiplier = CalculatGunFireMultipler();

            calculatedMultiplier = ((velocityMultiplier * adsMultiplier) + gunFireMultiplier) / 2f;
            calculatedSize = size * calculatedMultiplier;

            if (!float.IsNaN(calculatedSize) && !float.IsInfinity(calculatedSize))
            {
                reticlePanel.sizeDelta = new Vector2(calculatedSize, calculatedSize);
            }
            else
            {
                Debug.LogError("Calculated size is invalid.");
            }
        }
        else
        {
            Debug.LogError("Reticle panel is not assigned.");
        }
    }

    void FetchReticleElements()
    {
        reticleElements = new GameObject[4];
        reticleimages = new Image[4];

        for (int i = 0; i < 4; i++)
        {
            reticleElements[i] = this.transform.GetChild(i).gameObject;
            
            if (reticleElements[i] == null)
            {
                Debug.LogError($"Reticle element {i} is not assigned.");
            }
            else
            {
                RectTransform rectTransform = reticleElements[i].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    if (float.IsNaN(rectTransform.anchoredPosition.x) || float.IsNaN(rectTransform.anchoredPosition.y))
                    {
                        Debug.LogError($"Reticle element {i} has invalid RectTransform position.");
                    }
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            reticleimages[i] = reticleElements[i].GetComponent<Image>();

            if (reticleimages[i] == null)
            {
                Debug.LogError($"Reticle image {i} is not assigned.");
            }
        }
    }

    void SetColor(Vector4 rgbaColor)
    {
        Color color = rgbaColor;
        ApplyColor(color);
    }

    void ApplyColor(Color color)
    {
        for (int i = 0; i < 4; i++)
        {
            reticleimages[i].color = color;
        }
    }

    float CalculateVelocityMultiplier()
    {
        float rbv = playerController.velocity.magnitude;  //possible source of error
        float rbms = characterMovement.moveSpeed; //possible source of error

        if (rbms > 0.0f)
        {
            if (rbv > 0.25)
            {
                return 1f + (rbv / rbms);
            }
            else
            {
                return 1f;
            }
        }
        else
        {
            Debug.LogError("Max speed (rbms) is zero or negative.");
            return 1f;
        }
    }

    float CalculateADSMultiplier(bool isAds)
    {
        if (!isAds)
        {
            return 1f;
        }
        else
        {
            return Mathf.Lerp(calculatedMultiplier, adsRetTargetSizeMultiplier, adsRetSpeed);
        }
    }

    float CalculatGunFireMultipler()
    {
        return gunFireMultiplier;
    }

    public void GunFire(float falloffTime)
    {
        gunFireMultiplier = 1f + gunFireCurve.Evaluate(0); // Instantly enlarge the reticle
        if (gunFireCoroutine != null)
        {
            StopCoroutine(gunFireCoroutine);
        }
        gunFireCoroutine = StartCoroutine(GunFireFalloff(falloffTime));
    }

    IEnumerator GunFireFalloff(float falloffTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < falloffTime)
        {
            gunFireMultiplier = 1f + (gunFireCurve.Evaluate(elapsedTime / falloffTime) * gunFireCurveMultiplier);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        gunFireMultiplier = 1f;
        gunFireCoroutine = null;
        
    }

    void ResetGFM()
    {
        gunFireMultiplier = 1f;
    }

    public void Hide(bool hide)
    {
        if (hide && !hidden)
        {
            for (int i = 0; i < 4; i++)
            {
                reticleimages[i].enabled = false;
            }

            hidden = true;
        }
        else if (!hide && hidden)
        {
            for (int i = 0; i < 4; i++)
            {
                reticleimages[i].enabled = true;
            }

            hidden = false;
        }
    }
}
