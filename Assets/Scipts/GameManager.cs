using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameState currentGame;
    [SerializeField] private PawnInterface[] pawns;
    private Area[] places;
    private PawnInterface _selectedPawn;
    private RandomPlayer _randomPlayer = new RandomPlayer();
    private Stack<GameState> previousStates;
    private GameStep currentStep;
    [SerializeField]
    private float movingAnimationDuration = 1.0f;
    private PawnInterface _movingAnimationPawn;
    private Vector3 _movingAnimationOrigin;
    private Vector3 _movingAnimationDestination;
    private float _movingAnimationTime;
    private Area _movingAnimationPlace;

    public static GameManager Instance { get; private set; }
    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Multiple Game Managers.");
        }
        Instance = this;

        for (var i = 0; i < pawns.Length; i++)
        {
            pawns[i].GamePawn = currentGame.Pawns[i];
            pawns[i].transform.position =
           currentGame.Pawns[i].CurrentPlace.transform.position + new Vector3(0, 1.5f, 0);
        }

        places = FindObjectsOfType<Area>();
        previousStates = new Stack<GameState>();
        currentStep = GameStep.WaitingForMovement;

    }

    public void Click(PawnInterface pawn)
    {
        if (currentStep != GameStep.WaitingForMovement)
        {
            return;
        }

        if (pawn == _selectedPawn)
        {
            _selectedPawn = null;
        }
        else
        {
            _selectedPawn = pawn;
        }
    }

    public void Click(Area place)
    {
        if (currentStep != GameStep.WaitingForMovement)
        {
            return;
        }
        if (_selectedPawn == null)
        {
            return;
        }
        if (CanMove(_selectedPawn.GamePawn, place))
        {
            currentStep = GameStep.Moving;
            _movingAnimationPlace = place;
            _movingAnimationDestination = place.transform.position + new Vector3(0, 1.5f, 0);
            _movingAnimationOrigin = _selectedPawn.transform.position;
            _movingAnimationPawn = _selectedPawn;
            _movingAnimationTime = 0.0f;
            _selectedPawn = null;
        }
    }
    public bool CanMove(Pawn pawn, Area place)
    {
        foreach (var c in place.Connections)
        {
            if (c.Destination != pawn.CurrentPlace) continue;
            if (c.Type == ConnectionType.NORMAL || c.Type == pawn.Color)
            {
                return true;
            }
        }
        return false;
    }
    public void Move(Pawn pawn, Area place)
    {
        previousStates.Push(currentGame.DeepCopy());
        pawn.CurrentPlace = place;
        for (var i = currentGame.Items.Count - 1; i >= 0; i--)
        {
            if (currentGame.Items[i].Position == place)
            {
                currentGame.Items.RemoveAt(i);
            }
        }
        if (currentGame.Items.Count == 0)
        {
            Debug.Log("Game Ended.");
        }
    }
    public void UpdatePawnInterface(PawnInterface pawn)
    {
        pawn.transform.position = pawn.GamePawn.CurrentPlace.transform.position + new Vector3(0, 1.5f, 0);
    }


    private void OnDrawGizmos()
    {
        if (_selectedPawn != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_selectedPawn.transform.position, 1.0f);
        }

        foreach (var i in currentGame.Items)
        {
            if (i.Position != null)
            {
                Gizmos.color = i.Type == ItemType.BLACK ? Color.black : Color.white;
                Gizmos.DrawCube(i.Position.gameObject.transform.position,new Vector3(0.5f, 2.0f, 0.5f));
            }
        }
    }

    public void Update()
    {
        if (currentStep == GameStep.WaitingForMovement)
        {
            //random move
            if (Input.GetKeyUp(KeyCode.I))
            {
                _randomPlayer.RandomMove(currentGame);
                UpdateAllPawnInterfaces();
            }
            //undo
            if (Input.GetKeyUp(KeyCode.Z) && previousStates.Count > 0)
            {
                currentGame = previousStates.Pop();
                UpdateAllPawnInterfaces();
            }
            //save
            if (Input.GetKeyUp(KeyCode.S))
            {
                string json = JsonUtility.ToJson(currentGame);
                File.WriteAllText("save.json", json);
            }
            //load
            if (Input.GetKeyUp(KeyCode.L))
            {
                string json = File.ReadAllText("save.json");
                currentGame = JsonUtility.FromJson<GameState>(json);
                UpdateAllPawnInterfaces();
            }
        }
        
        if (currentStep == GameStep.Moving)
        {
            _movingAnimationTime += Time.deltaTime;
            Vector3 p = Vector3.Lerp(_movingAnimationOrigin, _movingAnimationDestination, _movingAnimationTime / movingAnimationDuration);
            _movingAnimationPawn.transform.position = p;

            if (_movingAnimationTime > movingAnimationDuration)
            {
                Move(_movingAnimationPawn.GamePawn,
                _movingAnimationPlace);
                UpdateAllPawnInterfaces();
                currentStep = GameStep.WaitingForMovement;
            }
        }
    }

    private void UpdateAllPawnInterfaces()
    {
        for (var i = 0; i < pawns.Length; i++)
        {
            pawns[i].GamePawn = currentGame.Pawns[i];
            pawns[i].transform.position =
           currentGame.Pawns[i].CurrentPlace.transform.position + new Vector3(0, 1.5f, 0);
        }
    }


    public Movement[] GetAllMovements()
    {
        List<Movement> allMovements = new List<Movement>();
        foreach (var p in currentGame.Pawns)
        {
            foreach (var c in p.CurrentPlace.Connections)
            {
                var m = new Movement(p.CurrentPlace, c.Destination, p);
                if (c.Type == p.Color)
                {
                    allMovements.Add(m);
                } 
            }
        }
        return allMovements.ToArray();
    }

}
