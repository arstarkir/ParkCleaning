using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] GameObject mCam;
    Rigidbody rb;
    NetworkAnimator toolAniamtor;

    [SerializeField] float speed = 1.5f;
    [SerializeField] float sprintSpeed = 2f;
    bool isSprinting = false;
    Vector2 _moveInput;

    [SerializeField] FoliageRemoverTool foliageRemover;

    public NetworkVariable<bool> IsWalking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    void Start()
    {
        if (!IsOwner)
        {
            GetComponent<PlayerInput>().DeactivateInput();
            this.enabled = false;
            return;
        }

        var playerInput = GetComponent<PlayerInput>();

        playerInput.SwitchCurrentControlScheme(Keyboard.current,Mouse.current);

        playerInput.ActivateInput();

        rb = GetComponent<Rigidbody>();
        toolAniamtor = GetComponent<ToolHandler>().curTool.gameObject.GetComponent<NetworkAnimator>();
    }

    void Update()
    {
        toolAniamtor.Animator.SetBool("isWalking", IsWalking.Value);

        PlayerMove();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnUseR(InputAction.CallbackContext context)
    {
        foliageRemover.TryUse(context);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValue<float>() == 1 ? true : false;

        if (IsOwner)
            IsWalking.Value = isSprinting;
    }

    void PlayerMove()
    {
        Vector3 camForward = mCam.transform.forward;
        Vector3 camRight = mCam.transform.right;
        camForward.y = camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camForward * _moveInput.y + camRight * _moveInput.x;
        Vector3 newPos = rb.position + move * (isSprinting ? sprintSpeed : speed) * Time.deltaTime;
        rb.MovePosition(newPos);
    }
}
