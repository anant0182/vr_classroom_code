using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Keyboard
{
    public static TMP_InputField CurrentTarget { get; private set; }

    public static void SetTarget(TMP_InputField input)
    {
        if (CurrentTarget != null && CurrentTarget != input)
        {
            CurrentTarget.DeactivateInputField();
        }

        CurrentTarget = input;

        if (CurrentTarget != null)
        {
            CurrentTarget.ActivateInputField();
            // Ensure caret is moved to the end without creating a selection
            CurrentTarget.MoveTextEnd(false);
            // Collapse selection explicitly to caret to avoid unexpected selection ranges
            int caret = CurrentTarget.caretPosition;
            CurrentTarget.selectionAnchorPosition = caret;
            CurrentTarget.selectionFocusPosition = caret;
            EventSystem.current?.SetSelectedGameObject(CurrentTarget.gameObject);
        }
    }

    public static void InsertText(string text)
    {
        if (CurrentTarget == null || string.IsNullOrEmpty(text)) return;

        CurrentTarget.ActivateInputField();

        // Handle selection replacement: if there is a selection, replace it
        int anchor = CurrentTarget.selectionAnchorPosition;
        int focus = CurrentTarget.selectionFocusPosition;
        int start = Mathf.Min(anchor, focus);
        int end = Mathf.Max(anchor, focus);

        string current = CurrentTarget.text ?? string.Empty;

        if (start != end)
        {
            // Replace selected range
            string before = current.Substring(0, start);
            string after = current.Substring(end);
            CurrentTarget.text = before + text + after;
            int newCaret = start + text.Length;
            CurrentTarget.caretPosition = newCaret;
            CurrentTarget.selectionAnchorPosition = newCaret;
            CurrentTarget.selectionFocusPosition = newCaret;
        }
        else
        {
            int pos = CurrentTarget.caretPosition;
            if (pos < 0) pos = 0;
            if (pos > current.Length) pos = current.Length;
            string before = current.Substring(0, pos);
            string after = current.Substring(pos);
            CurrentTarget.text = before + text + after;
            int newCaret = pos + text.Length;
            CurrentTarget.caretPosition = newCaret;
            CurrentTarget.selectionAnchorPosition = newCaret;
            CurrentTarget.selectionFocusPosition = newCaret;
        }

        CurrentTarget.ForceLabelUpdate();
    }

    public static void Backspace()
{
    if (CurrentTarget == null) return;

    // Always ensure the field is active BEFORE modifying text
    CurrentTarget.ActivateInputField();
    CurrentTarget.ForceLabelUpdate();

    // If there's a selection, delete the selected range
    int anchor = CurrentTarget.selectionAnchorPosition;
    int focus = CurrentTarget.selectionFocusPosition;
    int start = Mathf.Min(anchor, focus);
    int end = Mathf.Max(anchor, focus);

    string t = CurrentTarget.text ?? string.Empty;

    if (start != end)
    {
        string before = t.Substring(0, start);
        string after = t.Substring(end);
        CurrentTarget.text = before + after;
        CurrentTarget.caretPosition = start;
        CurrentTarget.selectionAnchorPosition = start;
        CurrentTarget.selectionFocusPosition = start;
    }
    else
    {
        int pos = CurrentTarget.caretPosition;
        if (pos <= 0) return;
        string before = t.Substring(0, pos - 1);
        string after = t.Substring(pos);
        CurrentTarget.text = before + after;
        int newPos = pos - 1;
        CurrentTarget.caretPosition = newPos;
        CurrentTarget.selectionAnchorPosition = newPos;
        CurrentTarget.selectionFocusPosition = newPos;
    }

    CurrentTarget.ForceLabelUpdate();
}

    public static void Submit()
    {
        if (CurrentTarget == null) return;

        CurrentTarget.onEndEdit?.Invoke(CurrentTarget.text);
    }
}