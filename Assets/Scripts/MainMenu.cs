using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button startButton;
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public TMP_Text bestScoreText;

    // Dificultat actual (rows x cols)
    private int rows = 4;
    private int cols = 4;

    void Awake()
    {
        // Assignar listeners als botons
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (easyButton != null) easyButton.onClick.AddListener(() => SetDifficulty(3, 2));
        if (mediumButton != null) mediumButton.onClick.AddListener(() => SetDifficulty(4, 4));
        if (hardButton != null) hardButton.onClick.AddListener(() => SetDifficulty(6, 4));

        // Mostrar best score inicial
        UpdateBestScoreText();
    }

    void SetDifficulty(int r, int c)
    {
        rows = r;
        cols = c;
        UpdateBestScoreText();
    }

    void UpdateBestScoreText()
    {
        string key = $"BestTime_{rows}x{cols}";
        float best = PlayerPrefs.GetFloat(key, 0f);

        if (best > 0f && best < Mathf.Infinity)
            bestScoreText.text = $"Best: {Mathf.Round(best)}s";
        else
            bestScoreText.text = "";
    }

    void StartGame()
    {
        // Guardar dificultat seleccionada
        PlayerPrefs.SetInt("Rows", rows);
        PlayerPrefs.SetInt("Cols", cols);

        // Carregar escena de joc
        SceneManager.LoadScene("GameScene"); // Assegura't que la teva escena de joc es diu "GameScene"
    }
}
