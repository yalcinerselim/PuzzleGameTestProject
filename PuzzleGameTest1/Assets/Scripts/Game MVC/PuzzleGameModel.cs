using UnityEngine;
using System.Collections.Generic;

public class PuzzleGameModel
{
    List<int> gridSize;
    List<int> gameTime;
    List<List<int>> levelShapeIDs;
    List<List<int>> shapeAngels;

    public void Initialize(GameData gameData)
    {
        gridSize = gameData.gridSize;
        gameTime = gameData.time;
        levelShapeIDs = gameData.pieceIDs;
        shapeAngels = gameData.pieceAngles;
    }
}
