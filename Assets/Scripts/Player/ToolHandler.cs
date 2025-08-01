using UnityEngine;
using UnityEngine.InputSystem;

public class ToolHandler : MonoBehaviour
{
    public CoreTool curTool;

    public void OnUseL(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() == 1)
            curTool.TryUse(context);
    }
}
