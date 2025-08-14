using UnityEngine;
using UnityEngine.Events;

public class GridCell : MonoBehaviour
{
    public int gridCellID;

    [SerializeField] private bool isOccupied = false;
    
    public void SetIsOccupied(bool value)
    {
        isOccupied = value; 
    }

    public bool IsOccupied()
    {
        return isOccupied;
    }

}
