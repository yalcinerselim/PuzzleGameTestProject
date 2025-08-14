using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShapeCell : MonoBehaviour
{
    [SerializeField] private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public List<RaycastResult> ObjectUnderShapeCell()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);

        eventData.position = RectTransformUtility.WorldToScreenPoint(null, transform.position);

        List<RaycastResult> results = new List<RaycastResult>();

        GraphicRaycaster raycaster = FindAnyObjectByType<GraphicRaycaster>();

        raycaster.Raycast(eventData, results);

        return results;
    }

    public void SetColor(Color color)
    {
        _image.color = new Color(color.r, color.g, color.b, color.a);
    }

}
