using UnityEngine;

public class MapClickHandler : MonoBehaviour
{
    public Camera cam;

    void Update()
    {
        if (UIManager.Instance.IsAnyPanelOpen || UIManager.Instance.IsMainMenuActive) 
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = cam.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
            if (hit.collider != null && hit.collider.GetComponent<Pin>() != null)
                return;

            PinManager.Instance.CreatePin(pos);
        }
    }
}