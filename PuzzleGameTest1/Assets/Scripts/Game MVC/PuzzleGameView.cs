using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PuzzleGameView : MonoBehaviour
{
    public UnityAction<List<GridCell>> InitialiseComplete;
    public UnityAction<int> ShapePlaced;
    
    #region Serialized fields
    [SerializeField] private PuzzleGameController controller;
    
    [SerializeField] private RectTransform gameArea;
    
    [SerializeField] private List<GameObject> _shapePrefabList;
    [SerializeField] private List<Shape> shapes;

    [SerializeField] private RectTransform _shapeSpawnArea;
    [SerializeField] private Transform _dragLayer;
    [SerializeField] private Transform _emptyLayer;
    [SerializeField] private GameObject _backgroundImg;
    [SerializeField] private Text countDownText;
    [SerializeField] private Transform _grid;
    [SerializeField] private GameObject gridCellPrefab;
    [SerializeField] private Text timerText;
    #endregion

    #region Private fields
    private Transform _parentAfterDrag;
    private List<ShapeCell> shapeCells;
    private List<int> levelShapes;
    private int gridSize;
    private List<int> shapeAngels;
    private List<GridCell> _gridCells = new List<GridCell>();
    private int colorSequence = 0;
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
    #endregion
    
    private void Awake()
    {
        controller.GameStart += LevelUIInitialise;
        controller.LevelUp += LevelClear;
        controller.GameEnd += GameEnd;
    }

    private void Start()
    {
        MoveScene();
    }

    private void MoveScene()
    {
     	gameArea.anchoredPosition -= new Vector2(0f, 1910f);
        Tween.Position(
           target: gameArea,
           endValue: new Vector3(540f, 970f, 0f),
           duration: 1.2f,
           Ease.Default
        );
    }

    private void LevelUIInitialise(PuzzleGameModel model)
    {
        gridSize = model.gridSize;
        shapeAngels = model.shapeAngels;
        levelShapes = model.levelShapeIDs;
        controller.inGameCountDown = model.gameTime;
        InitialiseGrid();
        InitialiseShapes();
        countDownText.raycastTarget = true;
        InitialiseComplete?.Invoke(_gridCells);
    }

    private void LevelClear()
    {
        foreach (var shape in shapes)
        {
            shape.OnCreated -= HandleShapeCreated;
            shape.OnDragBegin -=  HandleDragBegin;
            shape.OnDropped -= HandleDropped;
        }
        foreach (var shape in shapes)
        {
            Destroy(shape.gameObject);
        }
        shapes.Clear();
        
        foreach (var gridCell in controller._gridCells)
        {
            Destroy(gridCell.gameObject);
        }
        controller._gridCells.Clear();
        
        controller.occupiedCells = 0;
        timerText.text = "Score: 150";
        colorSequence = 0;
        controller.inGameCountDown = 0;
    }
    public void GameOver()
    {
        timerText.text = "Score: " + controller.score;
        
        countDownText.text = "Game Over";
        
        countDownText.raycastTarget = true;
    }

    private void GameEnd()
    {
        countDownText.text = "Game End\n\nScore: 150";
        countDownText.raycastTarget = true;
        timerText.text = "";
    }

    #region CountDown Methods
    public void ChangeCountDownText(float value)
    {
        if (value == 0)
        {
            countDownText.text = "GO!";
            return;
        }
        else if (value < 0)
        {
            countDownText.raycastTarget = false;
            countDownText.text = "";
            return;
        }
        countDownText.text = value.ToString();
    }

    public void ChangeInGameCountDownText(float value)
    {
        timerText.text = value.ToString();
    }
    
    #endregion
    
    #region Grid Methods
    private void InitialiseGrid()
    {
        SetGridLayoutGroup();
        
        for (var i = 0; i < gridSize * gridSize; i++)
        {
            GameObject cellInstance = Instantiate(gridCellPrefab, _grid);
            GridCell cell = cellInstance.GetComponent<GridCell>();
            _gridCells.Add(cell);
        }
    }

    private void SetGridLayoutGroup()
    {
        GridLayoutGroup layout = _grid.GetComponent<GridLayoutGroup>();
        RectTransform rectTransform = layout.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            _grid.AddComponent<RectTransform>();
        }
        var parentGridSize = rectTransform.rect.width;
        var cellSize = parentGridSize / gridSize * 0.8f;
        
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = gridSize;

        layout.cellSize = new Vector2(cellSize, cellSize);
        layout.spacing = Vector2.zero;
        layout.padding = new RectOffset(0, 0, 0, 0);
    }
    #endregion

    #region Shape Methods
    private void InitialiseShapes()
    {
        // Burada id ye gore prefab listeden sekiller olusturuluyor.
        shapes = new List<Shape>();
        int i = 0;
        Rect rect = _shapeSpawnArea.rect;
        foreach (var shapeID in levelShapes)
        {
            float randomX = Random.Range(rect.xMin + 10, rect.xMax - 10);
            float randomY = Random.Range(rect.yMin + 10, rect.yMax - 10);
            Vector3 randomPos = new Vector3(randomX, randomY, 1);
            
            Quaternion rotation = Quaternion.Euler(0,0, shapeAngels[i]);
            GameObject shapeInstance = Instantiate(_shapePrefabList.FirstOrDefault(p => p.GetComponent<Shape>().shapeID == shapeID), _shapeSpawnArea.transform);
            Shape shape = shapeInstance.GetComponent<Shape>();
            shapes.Add(shape);
            
            var shapeRect = shape.GetComponent<RectTransform>();
            shapeRect.localPosition = randomPos;
            shapeRect.localRotation = rotation;
            
            shape.OnCreated += HandleShapeCreated;
            shape.OnDragBegin += HandleDragBegin;
            shape.OnDropped += HandleDropped;
            i += 1;
        }
        
    }

    private void HandleShapeCreated(Shape shape)
    {
        //SetShapeScale(shape, gridSize);
        SetShapeColor(shape);
    }

    void SetShapeColor(Shape shape)
    {
        if (colorSequence == colors.Count)
        {
            colorSequence = 0;
        }
        shapeCells = shape.shapeCells;
        Color color = colors[colorSequence];
        foreach (ShapeCell cell in shapeCells)
        {
            cell.SetColor(color);
        }
        colorSequence += 1;
    }
    void SetShapeScale(Shape shape, int gridSize)
    {
        shape.GetComponent<RectTransform>().localScale = Vector3.one * (-0.5f * gridSize + 4);
    }

    void HandleDragBegin(Shape shape)
    {
        shape.SetCanRotate(false);
        
        shape.transform.SetParent(_dragLayer, true);
        SetCanvasGroup(false);
        SetShapeScale(shape, gridSize);
        
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
        // Burada obje bırakıldığı noktada raycast sonucu null değilse, _parentAfterDrag değişkeni güncellenir.
        _parentAfterDrag = eventData.pointerCurrentRaycast.gameObject.transform;
        SetParentAfterDrag(shape);
        SetCanvasGroup(true);
    }

    void SetCanvasGroup(bool status)
    {
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
            shape.SetCanRotate(true);
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
                    shape.SetCanRotate(true);
                    return;
                }
                else if (targetObject.GetComponent<ShapeSpawnArea>() || targetObject.GetComponent<EmptyLayer>())
                {
                    shape.transform.SetParent(targetObject.transform, true);
                    canPlace = false;
                    shape.SetCanRotate(true);
                    return;
                }
                else if (targetObject.GetComponent<GridCell>() && targetObject.GetComponent<GridCell>().IsOccupied())
                {
                    shape.transform.SetParent(_shapeSpawnArea, true);
                    shape.transform.localPosition = Vector3.zero;
                    canPlace = false;
                    shape.SetCanRotate(true);
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
                shape.SetCanRotate(false);
                ShapePlaced?.Invoke(gridSize);
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
    #endregion
}
