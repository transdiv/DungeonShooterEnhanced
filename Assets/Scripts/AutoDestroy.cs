using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    void Start()
    {
        float destroyTime = 1f;
        Destroy(gameObject, destroyTime);
    }
}
