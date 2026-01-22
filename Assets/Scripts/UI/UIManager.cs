using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using SFB;

public class UIManager : MonoBehaviour //God class... для MVP сойдёт...
{
    public static UIManager Instance;

    public bool IsAnyPanelOpen => previewPanel.gameObject.activeSelf || detailsPanel.gameObject.activeSelf || editPanel.gameObject.activeSelf || _exitConfirmPanel.gameObject.activeSelf /*|| _mainMenuPanel.gameObject.activeSelf*/;
    public bool IsMainMenuActive => _mainMenuPanel.gameObject.activeSelf;

    [Header("Main Menu")]
    [SerializeField] private CanvasGroup _mainMenuPanel;
    [SerializeField] private float _mainMenuFadeTime = 0.5f;

    [Header("Exit Confirm")]
    [SerializeField] private CanvasGroup _exitConfirmPanel;
    [SerializeField] private float _exitFadeTime = 0.25f;

    [Header("Other Panels")]
    public CanvasGroup previewPanel;
    public CanvasGroup detailsPanel;
    public CanvasGroup editPanel;

    [Header("Preview UI")]
    public TMP_Text previewTitle;
    public RawImage previewImage;

    [Header("Details UI")]
    public TMP_Text detailsTitle;
    public TMP_Text detailsDescription;
    public RawImage detailsImage;

    [Header("Edit UI")]
    public TMP_InputField titleInput;
    public TMP_InputField descriptionInput;
    public RawImage editImage;

    private Pin _currentPin;

    [Header("RectPanels")]
    [SerializeField] private RectTransform _previewRect;
    [SerializeField] private RectTransform _detailsRect;
    [SerializeField] private RectTransform _editRect;

    [SerializeField] private float _offset = 20f;

    void Awake()
    {
        Instance = this;
        HideAll();

        // Новое гм:
        _mainMenuPanel.alpha = 1;
        _mainMenuPanel.gameObject.SetActive(true);
        _mainMenuPanel.blocksRaycasts = true;
        _mainMenuPanel.interactable = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_exitConfirmPanel.gameObject.activeSelf)
            {
                CancelExit();
                return;
            }

            ShowExitConfirm();
        }
    }

    public void ShowExitConfirm()
    {
        CloseAll();
        SetPinsActive(false);

        _exitConfirmPanel.gameObject.SetActive(true);
        _exitConfirmPanel.alpha = 0;
        _exitConfirmPanel.interactable = true;
        _exitConfirmPanel.blocksRaycasts = true;

        _exitConfirmPanel.DOFade(1, _exitFadeTime)
            .SetEase(Ease.OutQuad);
    }

    public void ConfirmExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void CancelExit()
    {
        _exitConfirmPanel.interactable = false;
        _exitConfirmPanel.blocksRaycasts = false;

        _exitConfirmPanel.DOFade(0, _exitFadeTime)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                _exitConfirmPanel.gameObject.SetActive(false);
                SetPinsActive(true);
            });
    }

    void SetPinsActive(bool active)
    {
        if (PinManager.Instance == null)
            return;

        foreach (Transform pin in PinManager.Instance.transform)
            pin.gameObject.SetActive(active);
    }

    void HideAll()
    {
        HidePanel(previewPanel, true);
        HidePanel(detailsPanel, true);
        HidePanel(editPanel, true);
    }

    public void ShowPreview(Pin pin)
    {
        _currentPin = pin;
        previewTitle.text = pin.Data.title;

        LoadPinImage(previewImage, pin.Data.imagePath);

        PositionPanelNearPin(_previewRect, pin.transform);
        ShowPanel(previewPanel);
    }

    public void OpenDetails()
    {
        detailsTitle.text = _currentPin.Data.title;
        detailsDescription.text = _currentPin.Data.description;

        LoadPinImage(detailsImage, _currentPin.Data.imagePath);

        HidePanel(previewPanel);
        ShowPanel(detailsPanel);
    }

    public void OpenEdit()
    {
        titleInput.text = _currentPin.Data.title;
        descriptionInput.text = _currentPin.Data.description;

        LoadPinImage(editImage, _currentPin.Data.imagePath);

        HidePanel(detailsPanel);
        ShowPanel(editPanel);
    }

    public void StartGame()
    {
        _mainMenuPanel.interactable = false;
        _mainMenuPanel.blocksRaycasts = false;

        _mainMenuPanel.DOFade(0, _mainMenuFadeTime).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            _mainMenuPanel.gameObject.SetActive(false);

            LoadPins();
        });
    }

    private void LoadPins()
    {
        StartCoroutine(LoadPinsRoutine());
    }

    private IEnumerator LoadPinsRoutine()
    {
        var task = PinManager.Instance.LoadPinsAsync();
        while (!task.IsCompleted)
            yield return null;
    }

    public void OnClickEditImage()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Выбрать картинку", "", "png", false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string path = paths[0];

            string folder = Path.Combine(Application.persistentDataPath, "Images");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string destPath = Path.Combine(folder, Path.GetFileName(path));
            File.Copy(path, destPath, true);

            _currentPin.Data.imagePath = destPath;

            LoadPinImage(editImage, destPath);
        }
    }

    public void SaveEdit()
    {
        _currentPin.Data.title = titleInput.text;
        _currentPin.Data.description = descriptionInput.text;
        PinManager.Instance.SaveAllAsync();
        CloseAll();
    }

    public void DeletePin()
    {
        PinManager.Instance.RemovePin(_currentPin);
        CloseAll();
    }

    public void CloseAll()
    {
        HidePanel(previewPanel);
        HidePanel(detailsPanel);
        HidePanel(editPanel);

        _currentPin = null;
    }

    void ShowPanel(CanvasGroup panel)
    {
        panel.gameObject.SetActive(true);
        panel.alpha = 0;
        panel.transform.localScale = Vector3.one * 0.9f;

        panel.DOFade(1, 0.25f);
        panel.transform.DOScale(1, 0.25f).SetEase(Ease.OutCubic);
    }

    void HidePanel(CanvasGroup panel, bool instant = false)
    {
        if (instant)
        {
            panel.alpha = 0;
            panel.gameObject.SetActive(false);
            return;
        }

        panel.DOFade(0, 0.2f).OnComplete(() => panel.gameObject.SetActive(false));
    }

    void PositionPanelNearPin(RectTransform panel, Transform pin)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(pin.position);

        float panelWidth = panel.rect.width;
        float panelHeight = panel.rect.height;

        bool openLeft = screenPos.x > Screen.width * 0.6f;
        bool openDown = screenPos.y > Screen.height * 0.7f;

        float x = openLeft ? screenPos.x - panelWidth - _offset : screenPos.x + _offset;
        float y = openDown ? screenPos.y - _offset : screenPos.y + panelHeight + _offset;

        panel.position = new Vector2(x, y);
    }

    private void LoadPinImage(RawImage rawImage, string imagePath)
    {
        if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
        {
            byte[] bytes = File.ReadAllBytes(imagePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            rawImage.texture = tex;
        }
        else
            rawImage.texture = null;
    }
}