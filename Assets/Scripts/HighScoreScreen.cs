using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HighScoreScreen : MonoBehaviour
{
    public TMP_Text bestScoreText;
    public TMP_Text lastScoreText;
    public Button backButton;

    void Start()
    {
        float best = PlayerPrefs.GetFloat("HighScoreAccuracy", 0f);
        float last = PlayerPrefs.GetFloat("LastAccuracy", 0f);

        if (bestScoreText != null)
            bestScoreText.text = $"Best Accuracy: {best:F1}%";

        if (lastScoreText != null)
            lastScoreText.text = $"Last Session: {last:F1}%";

        if (backButton != null)
            backButton.onClick.AddListener(BackToMainMenu);
    }

    void BackToMainMenu()
    {
        // TODO: change "MainScene" to your real main game scene name
        SceneManager.LoadScene("MainGameScene");
    }
}
