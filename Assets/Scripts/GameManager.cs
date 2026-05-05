using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text timePlayedText;
    [SerializeField] private int lives;

    private int enemiesLeft;
    private int currentScene = 0;

    private bool allWavesSpawned;
    public int fromWave = 0; 

    public float timePlayed = 0f;
    private bool counting = false;
    private int lastSecond = -1;
    private PlayerInput playerInput;
    private string filePath;

    private bool helpPanelShown = false;
    private GameObject helpPanel;

    public int bestScore;
    const bool putBestScoreToMax = false; // Used for testing purposes, to reset the score to the max value (9999) every time the game starts. Set it to false for normal behavior.

    private string key; // Used to protect the score file

    public bool IsPlayerDead { get; private set; }

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Here you can update UI, search objets and init things
        int index = scene.buildIndex;
        int lastIndex = SceneManager.sceneCountInBuildSettings - 1;
        UpdateLevelText();
        if (index == 0 || index == lastIndex)
        {
            counting = false;
            SetTextsActive(false);
        }
        else
        {
            counting = true;
            SetTextsActive(true);
        }

        //Debug.Log($"Scene loaded: {scene.name} with index: {index} , lastIndex: {lastIndex}, counting: {counting}");
    }

    private void Start()
    {
        SetTextsActive(false);
        helpPanel = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "HelpPanel");
        UpdateLivesText();
        playerInput = GetComponent<PlayerInput>();
        key = GetKey();
        // Debug.Log("GetKey(): " + key);
        filePath = Path.Combine(Application.persistentDataPath, "score.dat");
        // Debug.Log("File Path: " + filePath);
        LoadOrCreateScore();
        // GenerateKey(); // Used only to generate the key to paste in GetKey()
    }


    private void UpdateLivesText()
    {
        livesText.text = "Lives: " + lives.ToString();
    }

    void Update()
    {
        ProcessInputs();
        if (!counting) return;
        timePlayed += Time.deltaTime;
        int actualSecond = Mathf.FloorToInt(timePlayed);
        if (actualSecond != lastSecond)
        {
            lastSecond = actualSecond;
            // This will execute ONLY once per second
            UpdateTimePlayedText(actualSecond);
        }
    }

    private void ProcessInputs()
    {
        bool exit = playerInput.actions["Exit"].WasPressedThisFrame();
        bool changeFullscreen = playerInput.actions["ChangeFullscreen"].WasPressedThisFrame();
        if (exit && helpPanelShown)
        {
            helpPanelShown = false;
            helpPanel.SetActive(false);
        }
        if (changeFullscreen)
        {
            SetFullscreen(!Screen.fullScreen);
        }
        if (exit && counting)
        {
            ResetGame();
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
    }

    public void StopTimer()
    {
        counting = false;
    }

    private void UpdateLevelText()
    {
        string level = SceneManager.GetActiveScene().name.Replace("Level", "").Trim();
        levelText.text = "Level: " + level;
    }

    public void UpdateWavesText(int waveNumber)
    {
        waveText.text = "Wave: " + waveNumber.ToString();
    }

    public void UpdateTimePlayedText(int timePlayed)
    {
        timePlayedText.text = "Time: " + timePlayed.ToString();
    }

    public void IncreaseEnemiesLeft()
    {
        enemiesLeft++;
    }

    public void DecreaseEnemiesLeft()
    {
        enemiesLeft--;

        if (enemiesLeft == 0 && allWavesSpawned)
        {
            LoadNextScene();
        }
    }

    public void SetAllWavesSpawned()
    {
        allWavesSpawned = true;
        fromWave = 0;
    }

    public void LoadNextScene()
    {
        currentScene++;
        LoadScene(currentScene);
    }

    private void LoadScene(int nScene)
    {
        Reset();
        SceneManager.LoadScene(nScene);        
    }

    private void SetTextsActive(bool value)
    {
        livesText.gameObject.SetActive(value);
        levelText.gameObject.SetActive(value);
        waveText.gameObject.SetActive(value);
        timePlayedText.gameObject.SetActive(value);
    }

    public void Die()
    {
        IsPlayerDead = true;
        lives--;
        UpdateLivesText();
        StopEnemies();
        StopEnemiesSpawn();
        StartCoroutine(WaitAndRestart(0.5f));
    }

    private void Reset()
    {
        enemiesLeft = 0;
        allWavesSpawned = false;
        IsPlayerDead = false;
    }

    private void StopEnemiesSpawn()
    {
        Spawner spawner = FindFirstObjectByType<Spawner>();
        spawner.StopAllCoroutines();
    }

    private void StopEnemies()
    {
        EnemyMovement[] enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
        foreach (EnemyMovement enemy in enemies)
        {
            enemy.StopMovement();
        }
    }

    private IEnumerator WaitAndRestart(float restartTime)
    {
        yield return new WaitForSeconds(restartTime);
        Reset();
        if (lives > 0)
        { 
            LoadScene(currentScene);
            UpdateLevelText();
        }
        else
        {
            ResetGame();
        }
    }

    public void ResetGame()
    {
        LoadScene(0);
        Destroy(gameObject);
        Destroy(AudioManager.Instance.gameObject);
    }

    string GetKey()
    {
        byte[] data = {
        32, 36, 18, 46, 33, 44, 59, 40,
        18, 62, 40, 46, 63, 40, 57, 44, 
        18, 124, 127, 126
        };

        byte k = 77;
        for (int i = 0; i < data.Length; i++)
            data[i] ^= k;
        return Encoding.UTF8.GetString(data);
    }

    // Generates the key in byte[] data format to put in GetKey() function
    void GenerateKey()
    {
        // key = "my_secret_key_123"
        string key = "";
        byte k = 77;
        byte[] bytes = Encoding.UTF8.GetBytes(key);
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] ^= k;
        Debug.Log(string.Join(", ", bytes));
    }

    void LoadOrCreateScore()
    {
        if (File.Exists(filePath))
        {
            string encrypted = File.ReadAllText(filePath);
            string decrypted = Decrypt(encrypted);

            if (!int.TryParse(decrypted, out bestScore))
            {
                bestScore = 9999;
                SaveEncrypted(bestScore);
            }
        }
        else
        {
            bestScore = 9999;
            SaveEncrypted(bestScore);
        }
        // This is used to reset the score for testing purposes
        #pragma warning disable CS0162
        if (putBestScoreToMax)
        {
                bestScore = 9999;
        }
        #pragma warning restore CS0162
    }

    public void SetFullscreen(bool fullscreen)
    {
        if (fullscreen)
        {
            Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        }
    }

    public void HelpWindow()
    {
        GameManager.Instance.helpPanel.SetActive(true);
        GameManager.Instance.helpPanelShown = true;
        }

    // Checks if there are multiple instances of GameManager in the scene, which should never happen
    private void checkDifferentInstances() {
        int inst1 = GetInstanceID();
        int inst2 = GameManager.Instance.GetInstanceID();
        Debug.Log($"Instance ID: {inst1} , Instance ID from other site: {inst2}");
        if (inst1 != inst2)
        {
            Debug.Log("There are multiple instances of GameManager!");
        } 
    }

    public void SaveScore(int newScore)
    {
        if (newScore < bestScore)
        {
            bestScore = newScore;
            SaveEncrypted(bestScore);
        }
    }

    void SaveEncrypted(int value)
    {
        string encrypted = Encrypt(value.ToString());
        File.WriteAllText(filePath, encrypted);
    }

    string Encrypt(string plainText)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32));
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = new byte[16];

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            return System.Convert.ToBase64String(encryptedBytes);
        }
    }

    string Decrypt(string cipherText)
    {
        try
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32));
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = new byte[16];

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                byte[] cipherBytes = System.Convert.FromBase64String(cipherText);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
        catch
        {
            return ""; // if it fails, it will force TryParse to reset
        }
    }

}