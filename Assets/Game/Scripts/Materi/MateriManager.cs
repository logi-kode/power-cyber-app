using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MateriManager : MonoBehaviour
{
    [SerializeField] private List<MateriData> daftarMateri = new List<MateriData>();
    private List<GameObject> _materiButtons = new List<GameObject>();

    [SerializeField] private GameObject listPanel;
    [SerializeField] private GameObject kontenPanel;
    [SerializeField] private GameObject materiButtonPrefab;
    [SerializeField] private GameObject progressBarContainer;

    [SerializeField] private RectTransform listContainer;

    [SerializeField] private float buttonHeight = 80f;
    [SerializeField] private float buttonSpacing = 15f;
    [SerializeField] private float buttonPaddingTop = 20f;
    [SerializeField] private float buttonPaddingLeft = 20f;
    [SerializeField] private float buttonPaddingRight = 20f;
    [SerializeField] private float fadeDuration = 0.5f;

    [SerializeField] private TextMeshProUGUI judulKontenText;
    [SerializeField] private TextMeshProUGUI isiKontenText;
    [SerializeField] private TextMeshProUGUI progressPercentText;
    [SerializeField] private Image ilustrasiImage;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private CanvasGroup kontenCanvasGroup;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button backButton;

    private Coroutine _fadeCoroutine;

    private void Start()
    {
        ShowListPanel();
        GenerateMateriButtons();

        if (backButton)
            backButton.onClick.AddListener(ShowListPanel);

        if (progressBarContainer)
            progressBarContainer.SetActive(false);
    }

    private void Update()
    {
        if (kontenPanel.activeSelf && scrollRect != null)
            UpdateProgressBar();
    }

    public void ShowListPanel()
    {
        listPanel.SetActive(true);
        kontenPanel.SetActive(false);
        if (progressBarContainer)
            progressBarContainer.SetActive(false);
    }

    public void ShowKontenPanel(MateriData materi)
    {
        if (materi == null) return;

        listPanel.SetActive(false);
        kontenPanel.SetActive(true);

        if (judulKontenText) judulKontenText.text = materi.judulMateri;
        if (isiKontenText) isiKontenText.text = materi.isiKonten;

        if (ilustrasiImage)
        {
            ilustrasiImage.gameObject.SetActive(materi.ilustrasi != null);
            if (materi.ilustrasi != null)
                ilustrasiImage.sprite = materi.ilustrasi;
        }

        if (scrollRect)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        if (progressBarContainer)
        {
            progressBarContainer.SetActive(true);
            if (progressBarFill) progressBarFill.fillAmount = 0f;
            if (progressPercentText) progressPercentText.text = "0%";
        }

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeInKonten());
    }

    private void GenerateMateriButtons()
    {
        foreach (var btn in _materiButtons)
            if (btn != null) Destroy(btn);
        _materiButtons.Clear();

        if (listContainer == null || materiButtonPrefab == null)
        {
            return;
        }

        float containerWidth = listContainer.rect.width;
        float btnWidth = containerWidth - buttonPaddingLeft - buttonPaddingRight;

        float totalHeight = buttonPaddingTop
                          + (daftarMateri.Count * buttonHeight)
                          + ((daftarMateri.Count - 1) * buttonSpacing)
                          + buttonPaddingTop;
        listContainer.sizeDelta = new Vector2(listContainer.sizeDelta.x, totalHeight);

        for (int i = 0; i < daftarMateri.Count; i++)
        {
            int index = i;
            var materi = daftarMateri[i];

            GameObject btnObj = Instantiate(materiButtonPrefab, listContainer);
            _materiButtons.Add(btnObj);

          
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1); 
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);

            
            float posY = -(buttonPaddingTop + i * (buttonHeight + buttonSpacing));
            rt.anchoredPosition = new Vector2(buttonPaddingLeft, posY);
            rt.sizeDelta = new Vector2(btnWidth, buttonHeight);

           
            var btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText) btnText.text = materi.judulMateri;

            var btn = btnObj.GetComponent<Button>();
            if (btn)
                btn.onClick.AddListener(() => ShowKontenPanel(daftarMateri[index]));
        }
    }


    private void UpdateProgressBar()
    {
        if (progressBarFill == null || scrollRect == null) return;

        float scrollPos = 1f - scrollRect.verticalNormalizedPosition;
        float contentHeight = scrollRect.content.rect.height;
        float viewHeight = scrollRect.viewport.rect.height;

        if (contentHeight <= viewHeight) scrollPos = 1f;

        scrollPos = Mathf.Clamp01(scrollPos);
        progressBarFill.fillAmount = scrollPos;

        if (progressPercentText)
            progressPercentText.text = $"{Mathf.RoundToInt(scrollPos * 100)}%";
    }

    private IEnumerator FadeInKonten()
    {
        if (kontenCanvasGroup == null) yield break;

        kontenCanvasGroup.alpha = 0f;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            kontenCanvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        kontenCanvasGroup.alpha = 1f;
    }
}