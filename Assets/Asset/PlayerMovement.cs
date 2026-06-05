using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody rb;
    public float jumpForce = 5f;
    public float checkDistance = 0.6f;

    void Start() { rb = GetComponent<Rigidbody>(); }

    void Update()
    {
        Vector2 inputVector = Vector2.zero;

        void Start() { rb = GetComponent<Rigidbody>(); }

        if (Keyboard.current is not null)
        {
            float h = 0;
            float v = 0;

            if (Keyboard.current.aKey.isPressed) h = -1;
            if (Keyboard.current.dKey.isPressed) h = 1;
            if (Keyboard.current.wKey.isPressed) v = 1;
            if (Keyboard.current.sKey.isPressed) v = -1;

            inputVector = new Vector2(h, v);
        }

        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y).normalized;

        if (moveDir.magnitude > 0)
        {
            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
        }

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, checkDistance);

        if (isGrounded && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // Vector3.up은 월드 기준 위 방향인 (0, 1, 0) 벡터입니다.
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}