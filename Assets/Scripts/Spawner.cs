using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int enemiesPerWave, waves;
    [SerializeField] private float timeBetweenSpawns, timeBetweenWaves;

    void Start()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        for (int i = 0; i < waves; i++)
        {
            GameManager.Instance.UpdateWavesText(i + 1);
            for (int j = 0; j < enemiesPerWave; j++)
            {
                yield return new WaitForSeconds(timeBetweenSpawns);
                int randomIndex = Random.Range(0, spawnPoints.Length);
                Instantiate(enemyPrefab, spawnPoints[randomIndex].position, Quaternion.identity);
                GameManager.Instance.IncreaseEnemiesLeft();
            }
            if (i < waves - 1)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }
        GameManager.Instance.SetAllWavesSpawned();
    }
}
