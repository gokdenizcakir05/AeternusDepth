using UnityEngine;
using UnityEngine.EventSystems;

public class MossDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    private Door6PuzzleController puzzleController;
    private GameObject mossObject;
    private Vector2 lastPosition;

    public void Initialize(Door6PuzzleController controller, GameObject moss)
    {
        puzzleController = controller;
        mossObject = moss;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastPosition = ((RectTransform)transform).anchoredPosition;
        transform.SetAsLastSibling(); // Öne getir
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform rt = (RectTransform)transform;

        // Mouse pozisyonunu UI koordinatına çevir
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos
        );

        // Hareket miktarını hesapla
        Vector2 dragAmount = localPos - lastPosition;

        // Pozisyonu güncelle
        rt.anchoredPosition = localPos;

        // Controller'a haber ver
        if (puzzleController != null)
        {
            puzzleController.OnMossDragged(mossObject, dragAmount);
        }

        lastPosition = localPos;
    }
}