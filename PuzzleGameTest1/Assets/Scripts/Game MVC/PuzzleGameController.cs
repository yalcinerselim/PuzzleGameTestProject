using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using PrimeTween;

public class PuzzleGameController : MonoBehaviour
{
    public UnityAction<PuzzleGameModel> GameStart;
    public UnityAction LevelUp;
    public UnityAction GameEnd;
    
    [SerializeField] private PuzzleGameView _puzzleGameView;
    private PuzzleGameModel _gModel;
    
    public List<GridCell> _gridCells;
    public float occupiedCells;
    
    [SerializeField] public int _level;
    private GameData _gameData;
    
    public int gameStartCountDown;
    public int inGameCountDown;
    
    public int score;
    
    private Tween _countDownTween;

    private void Awake()
    {
        _puzzleGameView.InitialiseComplete += LevelStart;
        _puzzleGameView.ShapePlaced += CheckOccupiedCells;
    }

    private void Start()
    {
        _level = 1;
        string json = Path.Combine(Application.streamingAssetsPath, "Game159Params.json");
        _gModel = new PuzzleGameModel();

        if (!File.Exists(json))
        {
            Debug.LogError("JSON file not found at path: " + json);
            return;
        }
        string jsonText = File.ReadAllText(json);
        _gameData = JsonConvert.DeserializeObject<GameData>(jsonText);
        GetLevelData(_gameData, _level);
    }

    private void GetLevelData(GameData gameData, int level)
    {
        gameStartCountDown = 3;
        _gModel.Initialise(gameData, level);
        GameStart?.Invoke(_gModel);
    }

    private void LevelStart(List<GridCell> gridCells)
    {
        _gridCells = gridCells;
        StartCountDown();
    }

    #region CountDown Methods
    private void StartCountDown()
    {
        Tween handle = Tween.Custom(
            startValue: gameStartCountDown,
            endValue: -1,
            duration: gameStartCountDown,
            onValueChange: value =>
            {
                _puzzleGameView.ChangeCountDownText(Mathf.CeilToInt(value));
            },
            ease: Ease.Linear
        );
        handle.OnComplete(() =>
        {
            _puzzleGameView.ChangeCountDownText(-1);
            StartInGameCountDown();
        });
    }

    private void StartInGameCountDown()
    {
        _countDownTween = Tween.Custom(
            startValue: inGameCountDown,
            endValue: 0,
            duration: inGameCountDown,
            onValueChange: value =>
            {
                _puzzleGameView.ChangeInGameCountDownText(Mathf.CeilToInt(value));
            },
            ease: Ease.Linear
        );
        _countDownTween.OnComplete(() =>
        {
            CalculateScore();
            _puzzleGameView.GameOver();
        });
    }
    #endregion
    
    void CheckOccupiedCells(int gridSize)
    {
        occupiedCells = 0;
        foreach (var gridCell in _gridCells)
        {
            if (gridCell.IsOccupied())
                occupiedCells++;
        }
        if (Mathf.RoundToInt(occupiedCells) == gridSize * gridSize)
        {
            _countDownTween.Complete();
            LevelEnd();
        }
    }

    private void LevelEnd()
    {
        if (_level >= 13)
        {
            GameEnd?.Invoke();
            return;
        }
        LevelUp?.Invoke();
        _level++;
        GetLevelData(_gameData, _level);
    }

    private void CalculateScore()
    { 
        score = Mathf.FloorToInt(occupiedCells / _gridCells.Count * 100);  
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
