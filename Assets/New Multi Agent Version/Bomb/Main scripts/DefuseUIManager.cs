using UnityEngine;
using UnityEngine.UI;

public class DefuseUIManager : MonoBehaviour
{
    public static DefuseUIManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject defuseUI;
    [SerializeField] private Slider progressBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Hide();
    }

    public void Show()
    {
        if (defuseUI != null)
            defuseUI.SetActive(true);
    }

    public void Hide()
    {
        if (defuseUI != null)
            defuseUI.SetActive(false);

        if (progressBar != null)
            progressBar.value = 0;
    }

    public void SetProgress(float value)
    {
        if (progressBar != null)
            progressBar.value = Mathf.Clamp01(value);
    }
}