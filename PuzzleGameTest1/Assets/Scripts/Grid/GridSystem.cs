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
            Debug.LogError("GridParent i�in RectTransform bile�eni bulunamad�.");
            return;
        }
        float parentGridSize = rectTransform.rect.width;
        float cellSize = parentGridSize / gridSize * 0.8f;

        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = gridSize;

        layout.cellSize = new Vector2(cellSize, cellSize);
        layout.spacing = Vector2.zero;
        layout.padding = new RectOffset(0, 0, 0, 0);
    }
}
