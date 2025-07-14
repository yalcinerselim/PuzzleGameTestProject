using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSystem : MonoBehaviour
{
    [SerializeField] private int gridSize;
    [SerializeField] private GameObject gridCellPrefab;
    [SerializeField] private Transform gridParent;

    private List<GridCell> _gridCells;

    private void Start()
    {
        InitializeGrid();
    }
    private void InitializeGrid()
    {
        SetGridLayoutGroup();

        // Grid hücrelerini oluþtur  
        for (int i = 0; i < gridSize * gridSize; i++)
        {
            Instantiate(gridCellPrefab, gridParent);
        }
    }
    private void SetGridLayoutGroup()
    {
        GridLayoutGroup layout = gridParent.GetComponent<GridLayoutGroup>();

        RectTransform rectTransform = gridParent.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("GridParent için RectTransform bileþeni bulunamadý.");
            return;
        }
        //float parentGridSize = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height);
        float parentGridSize = rectTransform.rect.width;
        float cellSize = SetCellSize(parentGridSize);

        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = gridSize;

        layout.cellSize = new Vector2(cellSize, cellSize);
        layout.spacing = Vector2.zero;
        layout.padding = new RectOffset(0, 0, 0, 0);
    }
    float SetCellSize(float parentSize)
    {
        return parentSize / gridSize * 0.8f;
    }
}
