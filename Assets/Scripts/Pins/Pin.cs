using UnityEngine;
using DG.Tweening;

public class Pin : MonoBehaviour
{
    public PinData Data { get; private set; }

    float _holdTimer;
    bool _isDragging;
    bool _isPressedOnThisPin;

    public void Init(Vector2 position)
    {
        Data = new PinData
        {
            id = System.Guid.NewGuid().ToString(),
            title = "Новый пин",
            description = "Базовое описание пина",
            position = position
        };

        transform.position = position;
        AnimateSpawn();
    }

    public void SetData(PinData data)
    {
        Data = data;
        transform.position = data.position;
        AnimateSpawn();
    }

    void Update()
    {
        HandleDrag();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButton(0) && _isPressedOnThisPin)
        {
            _holdTimer += Time.deltaTime;
            if (_holdTimer > 0.4f)
                _isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_isDragging)
                PinManager.Instance.SaveAllAsync();

            _isDragging = false;
            _holdTimer = 0;
        }

        if (_isDragging)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = pos;
            Data.position = pos;
        }
    }

    void OnMouseUpAsButton()
    {
        if (UIManager.Instance != null && !_isDragging && !UIManager.Instance.IsAnyPanelOpen)
            UIManager.Instance.ShowPreview(this);
    }

    void OnMouseDown()
    {
        _isPressedOnThisPin = true;
    }

    void OnMouseUp()
    {
        _isPressedOnThisPin = false;
        _holdTimer = 0;
    }

    void AnimateSpawn()
    {
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(targetScale, 0.3f)
            .SetEase(Ease.OutBack);
    }
}