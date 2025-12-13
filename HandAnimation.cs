// HandAnimator.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimator : MonoBehaviour
{
    [Header("Animator and parameters")]
    public Animator handAnimator;              // Animator on the hand mesh GameObject
    public string gripParam = "Grip";          // float 0..1
    public string triggerParam = "Trigger";    // float 0..1

    [Header("XR input actions")]
    public InputActionProperty gripAction;     // bind to left/right grip
    public InputActionProperty triggerAction;  // bind to left/right trigger

    void OnEnable()
    {
        gripAction.action?.Enable();
        triggerAction.action?.Enable();
    }

    void OnDisable()
    {
        gripAction.action?.Disable();
        triggerAction.action?.Disable();
    }

    void Update()
    {
        if (!handAnimator) return;

        float grip = gripAction.action != null ? gripAction.action.ReadValue<float>() : 0f;
        float trigger = triggerAction.action != null ? triggerAction.action.ReadValue<float>() : 0f;

        handAnimator.SetFloat(gripParam, grip);
        handAnimator.SetFloat(triggerParam, trigger);
    }
}
