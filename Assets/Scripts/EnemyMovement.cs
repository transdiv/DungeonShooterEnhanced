using UnityEngine;
using UnityEngine.UIElements;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private GameObject bar;  
    [SerializeField] private GameObject sprite;

    private Transform playerTransform;
    private bool isFacingRight = true;
    private Rigidbody2D rb;

    void Start()
    {
        playerTransform = FindFirstObjectByType<PlayerMovement>().transform;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Follow();
        Flip();
    }

    private void Follow()
    {
        Vector2 playerDirection = (playerTransform.position - transform.position).normalized;
        rb.MovePosition(rb.position + playerDirection * speed * Time.fixedDeltaTime);
    }

    private void Flip()
    {
        bool isPlayerRight = playerTransform.position.x > transform.position.x;

        if ((isFacingRight && !isPlayerRight) || (!isFacingRight && isPlayerRight))
        {
            Vector3 scale = sprite.transform.localScale;
            scale.x *= -1f;
            sprite.transform.localScale = scale;
            isFacingRight = !isFacingRight;
        }
    }

    public void StopMovement()
    {
        speed = 0f;
    }
}
