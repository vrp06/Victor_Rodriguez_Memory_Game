using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs i Recursos")]
    public GameObject prefabToken;             // Prefab amb Token.cs
    public Material[] uniqueMaterials;         // Materials únics

    [Header("Tauler")]
    public int rows = 4;
    public int cols = 4;
    public float spacing = 2f;

    [Header("UI - TextMeshPro")]
    public TMP_Text attemptsText;      // Comptador intents
    public TMP_Text timeText;          // Cronòmetre
    public TMP_Text bestScoreText;     // Best score
    public GameObject endPanel;        // Panell final
    public TMP_Text endTimeText;       // Temps final
    public TMP_Text endAttemptsText;   // Intents finals

    [Header("UI - Botons TMP")]
    public Button revealButton;    // Botó revelar fitxes 1s
    public Button returnMenuButton; // Botó tornar al menú

    [Header("So")]
    public AudioSource audioSource;
    public AudioClip clickClip;
    public AudioClip matchCorrectClip;
    public AudioClip matchWrongClip;
    public AudioClip newBestClip;

    private GameObject[,] tokens;
    private bool gameActive = false;
    private float timer = 0f;
    private int attempts = 0;
    private int openedCount = 0;
    private string firstName;
    private string secondName;
    private bool locked = false;
    private bool revealUsed = false;

    void Awake()
    {
        // Llegim la dificultat guardada al menú
        rows = PlayerPrefs.GetInt("Rows", 4);
        cols = PlayerPrefs.GetInt("Cols", 4);

        // Assignar listeners als botons
        if (revealButton != null) revealButton.onClick.AddListener(HandleReveal);
        if (returnMenuButton != null) returnMenuButton.onClick.AddListener(ReturnToMenu);

        // Inicialitzar UI
        if (attemptsText != null) attemptsText.text = "Intents: 0";
        if (timeText != null) timeText.text = "Temps: 0s";
        if (endPanel != null) endPanel.SetActive(false);
    }

    void Start()
    {
        GenerateBoard();
        gameActive = true;
        UpdateBestScoreText();
    }

    void Update()
    {
        if (gameActive)
        {
            timer += Time.deltaTime;
            if (timeText != null)
                timeText.text = "Temps: " + Mathf.Round(timer) + "s";
        }
    }

    // --------------------------
    // Generació del tauler
    // --------------------------
    void GenerateBoard()
    {
        int total = rows * cols;
        if (total % 2 != 0) { Debug.LogError("Rows*Cols ha de ser parell"); return; }

        int pairs = total / 2;
        if (uniqueMaterials.Length < pairs) Debug.LogWarning("No hi ha prou materials únics, es repetiran");

        List<Material> pool = new List<Material>();
        for (int i = 0; i < pairs; i++)
        {
            Material m = uniqueMaterials[i % uniqueMaterials.Length];
            pool.Add(m);
            pool.Add(m);
        }

        // Shuffle Fisher-Yates
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            Material tmp = pool[i];
            pool[i] = pool[r];
            pool[r] = tmp;
        }

        tokens = new GameObject[rows, cols];
        Vector3 startPos = new Vector3(-(cols - 1) * spacing / 2f, 0f, (rows - 1) * spacing / 2f);
        int idx = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Vector3 pos = startPos + new Vector3(j * spacing * 1.25f, 0f, -i * spacing);
                GameObject go = Instantiate(prefabToken, pos, Quaternion.identity);
                go.name = $"Token_{i}_{j}";

                Token token = go.GetComponentInChildren<Token>();
                if (token != null) token.mr.material = pool[idx];

                tokens[i, j] = go;
                idx++;
            }
        }
    }

    // --------------------------
    // Interacció amb fitxes
    // --------------------------
    public void TokenPressed(string tokenRootName)
    {
        if (!gameActive || locked) return;
        Token t = GetTokenByName(tokenRootName);
        if (t == null || t.IsDisappearing) return;

        PlaySound(clickClip);

        if (openedCount == 0)
        {
            firstName = tokenRootName;
            t.ShowToken();
            openedCount = 1;
        }
        else if (openedCount == 1)
        {
            if (firstName == tokenRootName) return;
            secondName = tokenRootName;
            t.ShowToken();
            openedCount = 2;

            attempts++;
            if (attemptsText != null) attemptsText.text = "Intents: " + attempts;

            locked = true;
            StartCoroutine(DelayedCheck(1f));
        }
    }

    IEnumerator DelayedCheck(float delay)
    {
        yield return new WaitForSeconds(delay);
        Token t1 = GetTokenByName(firstName);
        Token t2 = GetTokenByName(secondName);

        if (t1 == null || t2 == null) { ResetSelection(); yield break; }

        if (t1.mr.material.name == t2.mr.material.name)
        {
            PlaySound(matchCorrectClip);
            StartCoroutine(MatchAndRemove(t1, t2));
        }
        else
        {
            PlaySound(matchWrongClip);
            t1.HideToken();
            t2.HideToken();

            // Incrementar intents només quan fallem
            attempts++;
            if (attemptsText != null)
                attemptsText.text = "Intents: " + attempts;

            ResetSelection();
        }
    }

    IEnumerator MatchAndRemove(Token a, Token b)
    {
        a.MatchToken();
        b.MatchToken();
        yield return new WaitForSeconds(0.9f);

        RemoveTokenFromBoard(a.transform.root.name);
        RemoveTokenFromBoard(b.transform.root.name);
        ResetSelection();

        if (CheckWin()) EndGame();
    }

    void ResetSelection()
    {
        openedCount = 0;
        firstName = secondName = null;
        locked = false;
    }

    Token GetTokenByName(string rootName)
    {
        string name = rootName.Replace("(Clone)", "").Trim();
        string[] parts = name.Split('_');
        if (parts.Length < 3) return null;
        if (!int.TryParse(parts[1], out int i)) return null;
        if (!int.TryParse(parts[2], out int j)) return null;
        if (i < 0 || i >= rows || j < 0 || j >= cols) return null;
        GameObject go = tokens[i, j];
        if (go == null) return null;
        return go.GetComponentInChildren<Token>();
    }

    void RemoveTokenFromBoard(string rootName)
    {
        string name = rootName.Replace("(Clone)", "").Trim();
        string[] parts = name.Split('_');
        if (!int.TryParse(parts[1], out int i)) return;
        if (!int.TryParse(parts[2], out int j)) return;
        tokens[i, j] = null;
    }

    bool CheckWin()
    {
        foreach (var go in tokens)
            if (go != null) return false;
        return true;
    }

    void EndGame()
    {
        gameActive = false;
        // Calcular nova puntuació
        float score = timer * attempts;

        // Clau nova basada en la mida del tauler
        string key = $"BestScore_{rows}x{cols}";

        float prevBest = PlayerPrefs.GetFloat(key, Mathf.Infinity);

        // Com menys score, millor
        bool isNewBest = score < prevBest;

        if (isNewBest)
        {
            PlayerPrefs.SetFloat(key, score);
            PlaySound(newBestClip);
        }


        if (endPanel != null) endPanel.SetActive(true);
        if (endTimeText != null) endTimeText.text = "Temps: " + Mathf.Round(timer) + "s";
        if (endAttemptsText != null) endAttemptsText.text = "Intents: " + attempts;
        UpdateBestScoreText();
    }

    void UpdateBestScoreText()
    {
        string key = $"BestScore_{rows}x{cols}";
        float best = PlayerPrefs.GetFloat(key, Mathf.Infinity);

        if (best < Mathf.Infinity && bestScoreText != null)
            bestScoreText.text = $"Best Score: {Mathf.Round(best)}";
        else if (bestScoreText != null)
            bestScoreText.text = "";
    }


    public void HandleReveal()
    {
        if (revealUsed) return;
        revealUsed = true;
        if (revealButton != null) revealButton.interactable = false;
        StartCoroutine(RevealAllForSeconds(1f));
    }

    IEnumerator RevealAllForSeconds(float sec)
    {
        foreach (var go in tokens)
            if (go != null) go.GetComponentInChildren<Token>().ShowToken();
        yield return new WaitForSeconds(sec);
        foreach (var go in tokens)
            if (go != null) go.GetComponentInChildren<Token>().HideToken();
    }

    public void ReturnToMenu()
    {
        Debug.Log("Tornant al menú principal..."); // Comprovació al console
        if (Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogError("L'escena 'MainMenu' no existeix o no està a Build Settings!");
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }
}
