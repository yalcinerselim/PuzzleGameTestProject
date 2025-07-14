using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShapeController : MonoBehaviour
{
    // Bu s�n�f tek bir shape nesnesini kontrol etmek i�in de�il, sahnedeki t�m shape nesnelerini kontrol etmek i�in tasarlanm��t�r.
    // Orant�: scale = -0.5 * gridSize + 4
    // E�er shape gride oturmu�sa d�nd�r�lememelidir.

    [SerializeField] private List<GameObject> _shapePrefabList; 
    [SerializeField] private List<Shape> shapes;

    [SerializeField] private Transform _shapeSpawnArea;
    [SerializeField] private Transform _dragLayer;
    [SerializeField] private Transform _emptyLayer;
    [SerializeField] private Transform _grid;

    private Transform _parentAfterDrag;
    private Vector2 _locationBeforeDrag;
    private Transform _parentBeforeDrag;
    List<ShapeCell> shapeCells;

    private readonly List<Color> colors = new()
    {
        new (204, 0, 0),
        new (0, 153, 255),
        new (0, 204, 0),
        new (102, 0, 153),
        new (204, 51, 204),
        new (51, 204, 204),
        new (255, 204, 0),
        new (255, 102, 0),
    };

    private int[] levelShapes;

    private void Start()
    {
        // Burada level i�erisinde olu�turulacak �ekillerin ID leri rastgele olarak belirleniyor.
        levelShapes = new int[] { Random.Range(0, _shapePrefabList.Count - 2), Random.Range(0, _shapePrefabList.Count - 2), Random.Range(0, _shapePrefabList.Count - 2) }; 

        InstantiateShapes(levelShapes);
    }

    private void InstantiateShapes(int[] arr)
    {
        // Burada id ye g�re prefab listeden �ekiller olu�turuluyor.
        shapes = new List<Shape>();

        for (int i = 0; i < arr.Length; i++)
        {
            int id = arr[i];
            GameObject shapeInstance = Instantiate(_shapePrefabList.FirstOrDefault(p => p.GetComponent<Shape>().shapeID == id), _shapeSpawnArea.transform);
            Shape shape = shapeInstance.GetComponent<Shape>();
            shapes.Add(shape);

            shape.OnCreated += HandleShapeCreated;
            shape.OnDragBegin += HandleDragBegin;
            shape.OnDropped += HandleDropped;
        }
    }

    private void HandleShapeCreated(Shape shape)
    {
        SetShapeColor(shape);
        SetShapeScale(shape, 3);
    }

    void SetShapeColor(Shape shape)
    {
        shapeCells = shape.shapeCells;
        Color color = colors[shape.shapeID % colors.Count]; // Renkler listesi �zerinden d�ng�sel olarak renk atamas� yap�l�yor.
        foreach (ShapeCell cell in shapeCells)
        {
            cell.SetColor(color);
        }
    }
    void SetShapeScale(Shape shape, int gridSize)
    {
        shape.GetComponent<RectTransform>().localScale = Vector3.one * (-0.5f * gridSize + 4);
    }

    void HandleDragBegin(Shape shape)
    {
        // Burada shape nesnesi s�r�klenmeye ba�land���nda s�r�klenmeden �nceki anchored position ve parent kaydedilir.
        _locationBeforeDrag = shape.GetComponent<RectTransform>().anchoredPosition;
        _parentBeforeDrag = shape.transform.parent;

        // Burada shape s�r�klenirken _dragLayer'a child olarak ta��n�r. ��nk� Empty Layer Gridin alt�nda kal�yor.
        shape.transform.SetParent(_dragLayer, true);
        SetCanvasGroup(false);
        // Shape s�r�klenmeye ba�lad���nda, shape h�crelerinin alt�nda bulunan objeler e�er Grid cell ise , bu h�crelerin occupied durumlar� false olarak ayarlan�r.
        CheckObjectUnderShapeCellOnDragBegin(shape);
    }

    void HandleDropped(PointerEventData eventData, Shape shape)
    {
        if (eventData.pointerCurrentRaycast.gameObject == null)
        {
            Debug.LogWarning("Pointer current raycast game object is null, setting parent shape spawn area.");
            shape.transform.SetParent(_shapeSpawnArea, true);
            shape.transform.localPosition = Vector3.zero;
            SetCanvasGroup(true);
            return;
        }
        // Burada obje b�rka�ld��� noktada raycast sonucu null de�ilse, _parentAfterDrag de�i�keni g�ncellenir.
        _parentAfterDrag = eventData.pointerCurrentRaycast.gameObject.transform;
        SetParentAfterDrag(shape);
        SetCanvasGroup(true);
    }

    void SetCanvasGroup(bool status)
    {
        // Burada bir shape objesi s�r�klenirken b�t�n shape nesnelerinin CanvasGroup bile�eninin blocksRaycasts �zelli�i false olarak ayarlan�r.
        foreach (Shape shape in shapes)
        {
            shape.GetComponent<CanvasGroup>().blocksRaycasts = status;
        }
    }

    private void SetParentAfterDrag(Shape shape)
    {
        if (_parentAfterDrag.GetComponent<ShapeSpawnArea>() || _parentAfterDrag.GetComponent<EmptyLayer>())
        {
            shape.transform.SetParent(_parentAfterDrag, true);
        }
        else if (_parentAfterDrag.GetComponent<GridCell>())
        {
            bool canPlace = true;
            shapeCells = shape.shapeCells;
            foreach (ShapeCell cell in shapeCells)
            {
                List<RaycastResult> raycastResult = cell.ObjectUnderShapeCell();
                GameObject targetObject = raycastResult[0].gameObject;
                if (raycastResult.Count == 0 || raycastResult == null)
                {
                    Debug.Log("Raycast count 0, setting parent to empty layer.");
                    shape.transform.SetParent(_emptyLayer, true);
                    canPlace = false;
                    return;
                }
                else if (targetObject.GetComponent<ShapeSpawnArea>() || targetObject.GetComponent<EmptyLayer>())
                {
                    shape.transform.SetParent(targetObject.transform, true);
                    canPlace = false;
                    return;
                }
                else if (targetObject.GetComponent<GridCell>() && targetObject.GetComponent<GridCell>().IsOccupied())
                {
                    shape.transform.SetParent(_shapeSpawnArea, true);
                    shape.transform.localPosition = Vector3.zero;
                    canPlace = false;
                    return;
                }
            }
            if (canPlace)
            {
                foreach (ShapeCell cell in shapeCells)
                {
                    List<RaycastResult> raycastResults = cell.ObjectUnderShapeCell();
                    GameObject targetObject = raycastResults[0].gameObject;
                    cell.transform.position = targetObject.transform.position;
                    targetObject.GetComponent<GridCell>().SetIsOccupied(true);
                }
                shape.transform.SetParent(_dragLayer, true);
            }
        }
    }

    void CheckObjectUnderShapeCellOnDragBegin(Shape shape)
    {
        shapeCells = shape.shapeCells;
        foreach (ShapeCell cell in shapeCells)
        {
            List<RaycastResult> raycastResults = cell.ObjectUnderShapeCell();
            if (raycastResults.Count == 0) continue;
            GameObject targetObject = raycastResults[0].gameObject;
            if (targetObject.GetComponent<GridCell>() && targetObject.GetComponent<GridCell>().IsOccupied())
            {
                targetObject.GetComponent<GridCell>().SetIsOccupied(false);
            }
        }
    }
}

