using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private AudioClip dieClip;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            animator.SetTrigger("Die");
            AudioManager.Instance.PlaySoundEffect(dieClip, 1f);
            GameManager.Instance.Die();
        }
    }
}
