using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private float speed;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 input;
    private bool isFacingRight = true;
    private PlayerInput playerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();  
    }

    void Update()
    {
        if (GameManager.Instance.IsPlayerDead)
        {
            input = Vector2.zero;
            return;
        }
        ProcessInputs();
        Flip();
        animator.SetFloat("Speed", input.magnitude);
    }
    private void ProcessInputs()
    {
        input = playerInput.actions["Move"].ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }

    private void Flip()
    {
        if ((isFacingRight && input.x < 0f) || (!isFacingRight && input.x > 0f))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
            isFacingRight = !isFacingRight;
        }
    }
}
