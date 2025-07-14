using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PuzzleGameController : MonoBehaviour
{
    private PuzzleGameModel _gModel;

    public PuzzleGameModel GameModel => _gModel;



    private void Start()
    {
        string json = Path.Combine(Application.streamingAssetsPath, "Game159Params.json");


        if (File.Exists(json))
        {
            string jsonText = File.ReadAllText(json);
            GameData gameData = JsonConvert.DeserializeObject<GameData>(jsonText);
        }
        else
        {
            Debug.LogError("JSON file not found at path: " + json);
        }
    }

    public void Initialize()
    {
    }

}

[System.Serializable]
public class GameData
{
    public List<int> gridSize;
    public List<int> time;
    public List<List<int>> pieceIDs;
    public List<List<int>> pieceAngles;
}
