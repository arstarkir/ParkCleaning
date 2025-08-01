using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] GameObject mCam;
    Rigidbody rb;

    [SerializeField] float speed = 1.5f;
    [SerializeField] float sprintSpeed = 2f;
    bool isSprinting = false;
    Vector2 _moveInput;

    void Start()
    {
        if (!IsLocalPlayer)
            this.enabled = false;

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        PlayerMove();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {

        isSprinting = context.ReadValue<float>() == 1 ? true : false;
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
