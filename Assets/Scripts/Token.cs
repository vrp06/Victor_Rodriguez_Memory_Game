using System.Collections;
using UnityEngine;

public class Token : MonoBehaviour
{
    private GameManager gameManager;
    public MeshRenderer mr;
    private Animator animator;
    private bool disappearing = false;

    public bool IsDisappearing => disappearing;

    void Start()
    {
        GameObject gm = GameObject.FindGameObjectWithTag("GameManager");
        if (gm != null) gameManager = gm.GetComponent<GameManager>();
        animator = GetComponent<Animator>();
    }

    void OnMouseDown()
    {
        // Notar: TokenPressed utilitza el nom del root per identificar la posició al tauler
        if (!disappearing && gameManager != null)
            gameManager.TokenPressed(transform.root.name);
    }

    public void ShowToken()
    {
        if (animator != null)
            animator.SetTrigger("Show");
    }

    public void HideToken()
    {
        if (animator != null)
            animator.SetTrigger("Hide");
            animator.SetTrigger("idle"); // dispara el trigger Idle després de Hide
    }

    public void MatchToken()
    {
        if (disappearing) return;
        disappearing = true;
        if (animator != null)
            animator.SetTrigger("Disappear");
        // destruir el root després d'una estona; GameManager també neteja la referència
        StartCoroutine(DestroyRootAfter(0.95f));
    }

    IEnumerator DestroyRootAfter(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(transform.root.gameObject);
    }
}
