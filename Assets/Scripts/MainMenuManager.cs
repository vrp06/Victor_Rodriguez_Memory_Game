using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI - Botons")]
    public Button startButton;       // Botó Start
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    [Header("UI - Best Score")]
    public TMP_Text bestScoreText;

    // Dificultat actual (per defecte Medium)
    private int selectedRows = 4;
    private int selectedCols = 4;

    void Start()
    {
        // Assignar listeners
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (easyButton != null) easyButton.onClick.AddListener(SetEasy);
        if (mediumButton != null) mediumButton.onClick.AddListener(SetMedium);
        if (hardButton != null) hardButton.onClick.AddListener(SetHard);

        // Mostrar el best score inicial (Medium 4x4)
        UpdateBestScore(selectedRows, selectedCols);
    }

    void SetEasy()
    {
        selectedRows = 3;
        selectedCols = 2;
        UpdateBestScore(selectedRows, selectedCols);
    }

    void SetMedium()
    {
        selectedRows = 4;
        selectedCols = 4;
        UpdateBestScore(selectedRows, selectedCols);
    }

    void SetHard()
    {
        selectedRows = 6;
        selectedCols = 4;
        UpdateBestScore(selectedRows, selectedCols);
    }

    public void StartGame()
    {
        // Guardar la dificultat escollida a PlayerPrefs
        PlayerPrefs.SetInt("Rows", selectedRows);
        PlayerPrefs.SetInt("Cols", selectedCols);

        // Canviar d'escena al joc
        SceneManager.LoadScene("GameScene");
    }

    void UpdateBestScore(int rows, int cols)
    {
        string key = $"BestTime_{rows}x{cols}";
        float best = PlayerPrefs.GetFloat(key, 0f);
        if (best > 0f && best < Mathf.Infinity && bestScoreText != null)
        {
            bestScoreText.text = $"Best: {Mathf.Round(best)}s";
        }
        else if (bestScoreText != null)
        {
            bestScoreText.text = "";
        }
    }
}
