using UnityEngine;

public class KeyInput : MonoBehaviour
{
    public string character;

    public void InsertCharacter()
    {
        if (string.IsNullOrEmpty(character))
        {
            Debug.LogWarning("Key has no character assigned.", this);
            return;
        }

        Keyboard.InsertText(character);
    }

    public void Backspace()
    {
        Keyboard.Backspace();
    }

    public void Submit()
    {
        Keyboard.Submit();
    }
}