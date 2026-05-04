using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private Healthbar healthbar;
    [SerializeField] private int maxHealth;
    [SerializeField] private AudioClip hitClip, dieClip;
    [SerializeField] private Transform enemySpriteTransform;

    private SpriteRenderer spriteRenderer;
    private int health;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = maxHealth;
    }

    private void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
            AudioManager.Instance.PlaySoundEffect(dieClip, 1f);
            GameManager.Instance.DecreaseEnemiesLeft();
        }
        else
        {
            healthbar.UpdateHealthbar(maxHealth, health);
            AudioManager.Instance.PlaySoundEffect(hitClip, 0.5f);
            StartCoroutine(Blink(0.1f)); 
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Arrow arrow = collision.gameObject.GetComponent<Arrow>();
        if (arrow != null)
        {
            TakeDamage(1);
        }
    }

    private IEnumerator Blink(float blinkTime)
    {
        enemySpriteTransform.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(blinkTime);
        enemySpriteTransform.GetComponent<SpriteRenderer>().color = Color.white;
    }

}
