using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Shape : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
{
    // Hareketi sağlamaktan sorumlu olan sınıf.

    //-- Unity Actions --//
    public UnityAction<Shape> OnCreated;
    public UnityAction<Shape> OnDragBegin;
    public UnityAction<PointerEventData, Shape> OnDropped;
    //-- Unity Actions --//
    
    public int shapeID;
    public List<ShapeCell> shapeCells;

    //-- Private Variables --//
    private Vector2 _offset;
    private CanvasGroup _canvasGroup;
    [SerializeField]
    private bool _canRotate;
    //-- Private Variables --//

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        _canRotate = true;
        shapeCells = new List<ShapeCell>(GetComponentsInChildren<ShapeCell>());

        OnCreated?.Invoke(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        SetCanRotate(false);
        _offset = (Vector2)transform.position - eventData.position;
        OnDragBegin?.Invoke(this);
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position + _offset;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        OnDropped?.Invoke(eventData, this);
    }

    #region Rotation
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_canRotate)
            ChangeRotation(90);
    }

    private void ChangeRotation(int value)
    {
        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + value);
    }

    public void SetCanRotate(bool value)
    {
        _canRotate = value;
    }
    #endregion

}
