using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveScore : MonoBehaviour
{
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private Image thumbsUpImage;

    void Start()
    {
        bestScoreText.text = "Minimun Time: " + GameManager.Instance.bestScore;
        playerScoreText.text = "Your Time: " + (int)GameManager.Instance.timePlayed;
        if ((int)GameManager.Instance.timePlayed < GameManager.Instance.bestScore)
            thumbsUpImage.gameObject.SetActive(true);
        GameManager.Instance.SaveScore((int)GameManager.Instance.timePlayed);
    }   
}
