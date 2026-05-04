using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
public class AimAndShoot : MonoBehaviour
{
    [SerializeField] private float aimSpeed,rotationSpeed;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform playerTransform,shootPosition;
    [SerializeField] private AudioClip launchArrowClip;
    [SerializeField] private PlayerInput playerInput;


    private Camera cam;
    private Vector2 mouseWorldPosition, direction;
    private bool canShoot = false;

    void Start()
    {
        cam = Camera.main;
        // Delay shooting for 1 seconds to prevent shooting immediately after respawn
        StartCoroutine(EnableShootingAfterDelay(1f));
    }

    IEnumerator EnableShootingAfterDelay(float delay)
    {
        canShoot = false;
        yield return new WaitForSeconds(delay);
        canShoot = true;
    }

    void Update()
    {
        if (GameManager.Instance.IsPlayerDead)
        {
            return;
        }
        transform.position = playerTransform.position;

        if (playerInput.currentControlScheme == "Gamepad")
        {
            Rotate();
        }
        else
        {
            Aim();
        }
    }

    void Rotate()
    {
        float rotateInput = playerInput.actions["Aim"].ReadValue<Vector2>().x;
        transform.Rotate(new Vector3(0f, 0f, -rotateInput * rotationSpeed * Time.deltaTime));
        direction = transform.right;
    }


    private void Aim()
    {
        Vector2 mousePosition = playerInput.actions["Aim"].ReadValue<Vector2>();
        mouseWorldPosition = cam.ScreenToWorldPoint(mousePosition);
        direction = (mouseWorldPosition - (Vector2)transform.position).normalized;
        transform.right = Vector2.MoveTowards(transform.right, direction, aimSpeed * Time.deltaTime);
    }

    public void Shoot(InputAction.CallbackContext callbackContext)
    {
        if (GameManager.Instance.IsPlayerDead)
        {
            return;
        }
        if (!canShoot)
        {
            return;
        }
        if (callbackContext.performed)
        {
            GameObject arrow = Instantiate(arrowPrefab, shootPosition.position, transform.rotation);
            arrow.GetComponent<Arrow>().Launch(direction);
            AudioManager.Instance.PlaySoundEffect(launchArrowClip, 0.5f);
        }
    }

}
