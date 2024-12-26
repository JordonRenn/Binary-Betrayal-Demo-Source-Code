using System.Collections;
using UnityEngine;

public class FPSS_PlayerCamPositioning : MonoBehaviour
{
    [SerializeField] Transform targetPosition;

    [Header("DEV OPTIONS")]
    [Space(10)]
    
    [SerializeField] private bool debugMode;            //Enable/Disable debug mode
    [SerializeField] private float initDelay = 0.2f;    //used to pause execution between steps of initialization when needed
    [SerializeField] private float initTimeout = 10f;   //initialization timeout
    private bool initialized = false;                   //flag used to stop Update() from running before initialization is complete

    void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        yield return new WaitForSeconds(initDelay);

        GameObject tarPosObj = GameObject.FindWithTag("cam_Target");
        targetPosition = tarPosObj.transform;

        yield return new WaitForSeconds(initDelay);

        initialized = true;
    }

    void Update()
    {
        if (!initialized) {return;}

        UpdateCameraPosition();
    }

    public void UpdateCameraPosition()
    {
        transform.position = targetPosition.position;
    }
}
