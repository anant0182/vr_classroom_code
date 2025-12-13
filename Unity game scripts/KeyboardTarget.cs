using UnityEngine;
using TMPro;

public class KeyboardTarget : MonoBehaviour
{
    public TMP_InputField nextField; // What receives focus on Enter

    public void SetTargetField(TMP_InputField field)
    {
        Keyboard.SetTarget(field);
    }

    // Called when Enter key is pressed
    public void OnSubmitFlow(string text)
    {
        if (nextField != null)
        {
            // Move to next input field
            Keyboard.SetTarget(nextField);
            nextField.ActivateInputField();
        }
        else
        {
            // Last field → submit login
            var login = FindAnyObjectByType<LoginManager>();
            login?.OnLogin();
        }
    }
}