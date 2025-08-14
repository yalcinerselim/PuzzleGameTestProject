using System.Collections.Generic;

public class PuzzleGameModel
{
    public int gridSize;
    public int gameTime;
    public List<int> levelShapeIDs;
    public List<int> shapeAngels;

    public void Initialise(GameData gameData, int level)
    {
        gridSize = gameData.gridSize[level - 1];
        gameTime = gameData.time[level - 1];
        levelShapeIDs = gameData.pieceIDs[level - 1];
        shapeAngels = gameData.pieceAngles[level - 1];
    }
}
