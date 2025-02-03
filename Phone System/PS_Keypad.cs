using UnityEngine;

public class PS_Keypad : MonoBehaviour
{
    [SerializeField] private PS_Main c_PhoneMain;
    [SerializeField] private Collider phoneHook;
    [SerializeField] private Collider NUM0;
    [SerializeField] private Collider NUM1;
    [SerializeField] private Collider NUM2;
    [SerializeField] private Collider NUM3;
    [SerializeField] private Collider NUM4;
    [SerializeField] private Collider NUM5;
    [SerializeField] private Collider NUM6;
    [SerializeField] private Collider NUM7;
    [SerializeField] private Collider NUM8;
    [SerializeField] private Collider NUM9;

    private string phoneNumber = "";

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == phoneHook)
                {
                    ResetInput();
                }
                else if (hit.collider == NUM0) AddDigit("0");
                else if (hit.collider == NUM1) AddDigit("1");
                else if (hit.collider == NUM2) AddDigit("2");
                else if (hit.collider == NUM3) AddDigit("3");
                else if (hit.collider == NUM4) AddDigit("4");
                else if (hit.collider == NUM5) AddDigit("5");
                else if (hit.collider == NUM6) AddDigit("6");
                else if (hit.collider == NUM7) AddDigit("7");
                else if (hit.collider == NUM8) AddDigit("8");
                else if (hit.collider == NUM9) AddDigit("9");
            }
        }
    }

    private void AddDigit(string digit)
    {
        if (phoneNumber.Length < 7)
        {
            phoneNumber += digit;
            if (phoneNumber.Length == 7)
            {
                c_PhoneMain.DialNumber(phoneNumber);
            }
        }
    }

    private void ResetInput()
    {
        phoneNumber = "";
    }
}
