using UnityEngine;
using UnityEngine.InputSystem;

public class ToolHandler : MonoBehaviour
{
    public CoreTool curTool;

    public void OnUseL(InputAction.CallbackContext context)
    {
        curTool.TryUse(context);
    }
}
