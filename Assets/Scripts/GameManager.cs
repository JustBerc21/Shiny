using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Turns
    private int Turns = 1;
    private int TurnHalves;
    [SerializeField] private Text TurnText;
    private TeamType TeamTurn;

    //Score
    [SerializeField] int RedScore;
    [SerializeField] int BlueScore;
    [SerializeField] Text RedScoreText;
    [SerializeField] Text BlueScoreText;

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

    int selectedPawnTeam; //1 red, 2 blue
    int placeSelectedPawnTeam;
    int selectedPawnType; //1 raven, 2 racoon
    int placeSelectedPawnType;

    //Raccoon Theft
    Pawn StealablePawn;
    [SerializeField] private Item lastItem;

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
           currentGame.Pawns[i].CurrentPlace.transform.position + new Vector3(0, currentGame.Pawns[i].CurrentPlace.gameObject.GetComponent<BoxCollider>().size.y / 2, 0);
        }

        places = FindObjectsOfType<Area>();
        previousStates = new Stack<GameState>();
        currentStep = GameStep.WaitingForMovement;

        InvokeRepeating("CheckForRemoteMovements", 0, 1.0f);

        int r = UnityEngine.Random.Range(0, 2);
        if (r == 0) TeamTurn = TeamType.RED;
        else TeamTurn = TeamType.BLUE;

        Debug.Log(TeamTurn);
    }

    public void Update()
    {
        if (currentStep == GameStep.WaitingForMovement)
        {
            //random move
            if (Input.GetKeyUp(KeyCode.I))
            {
                _randomPlayer.RandomMove(currentGame, out var pawnToMove, out var placeToMove);
                if (pawnToMove != null && placeToMove != null)
                {
                    var listOfInterfacePawns = pawns.ToList();
                    var p = listOfInterfacePawns.Find(
                    ip => ip.GamePawn.CurrentPlace == pawnToMove.CurrentPlace);
                    MakeMove(p, placeToMove);
                }

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
            Vector3 p = Vector3.Lerp(_movingAnimationOrigin, _movingAnimationDestination + new Vector3(0, _movingAnimationPawn.GamePawn.CurrentPlace.gameObject.GetComponent<BoxCollider>().size.y / 2, 0), _movingAnimationTime / movingAnimationDuration);
            _movingAnimationPawn.transform.position = p;
            _movingAnimationPawn.gameObject.GetComponentInChildren<Animator>().SetBool("Walk", true);

            if (_movingAnimationTime > movingAnimationDuration)
            {
                Move(_movingAnimationPawn.GamePawn,
                _movingAnimationPlace);
                UpdateAllPawnInterfaces();
                _movingAnimationPawn.gameObject.GetComponentInChildren<Animator>().SetBool("Walk", false);
                currentStep = GameStep.WaitingForMovement;
            }
        }

        if (TeamTurn == TeamType.RED) TurnText.color = Color.red;
        else TurnText.color = Color.blue;
        if (TurnHalves >= 2)
        {
            Turns += 1;
            TurnHalves = 0;
        }
        TurnText.text = "Turn " + Turns;
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
        else if (pawn.GamePawn.Team == TeamTurn)
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
        MakeMove(_selectedPawn, place);
    }

    public bool CanMove(Pawn pawn, Area place)
    {
        if (pawn.Team == TeamType.RED)////
        {
            selectedPawnTeam = 1;
        }
        else if (pawn.Team == TeamType.BLUE)
        {
            selectedPawnTeam = 2;
        }

        if (pawn.Color == ConnectionType.NORMAL)////
        {
            selectedPawnType = 1;
        }
        else if (pawn.Color == ConnectionType.RACCOON)
        {
            selectedPawnType = 2;
        }

        foreach (var p in currentGame.Pawns)////
        {
            if (p.CurrentPlace == place)
            {
                if (p.Team == TeamType.RED)
                {
                    placeSelectedPawnTeam = 1;
                }
                else if (p.Team == TeamType.BLUE)
                {
                    placeSelectedPawnTeam = 2;
                }
                break;
            }
        }

        foreach (var p in currentGame.Pawns)////
        {
            if (p.CurrentPlace == place)
            {
                if (p.Color == ConnectionType.NORMAL)
                {
                    placeSelectedPawnType = 1;
                }
                else if (p.Color == ConnectionType.RACCOON)
                {
                    placeSelectedPawnType = 2;
                    StealablePawn = p;
                }
                break;
            }
        }

        foreach (var c in place.Connections)
        {
            if (c.Destination != pawn.CurrentPlace) continue;

            if (c.Type == ConnectionType.NORMAL || c.Type == pawn.Color)
            {
                if ((placeSelectedPawnTeam == selectedPawnTeam) || placeSelectedPawnTeam == 0 || selectedPawnTeam == 0)////
                {
                    placeSelectedPawnTeam = 0;
                    selectedPawnTeam = 0;
                    return true;
                }
                else if ((placeSelectedPawnTeam != selectedPawnTeam))////
                {
                    if ((placeSelectedPawnType == 1 && selectedPawnType == 1))////
                    {
                        placeSelectedPawnTeam = 0;
                        selectedPawnTeam = 0;
                        placeSelectedPawnType = 0;
                        selectedPawnType = 0;
                        return true;
                    }
                    else if ((selectedPawnType == 1 && placeSelectedPawnType == 2)) 
                    {
                        if (currentGame.Items.Contains(StealablePawn.HeldItem) && !currentGame.Items.Contains(pawn.HeldItem)) 
                        {
                            pawn.HeldItem = StealablePawn.HeldItem;
                            StealablePawn.HeldItem = null;
                        }
                        StealablePawn = null;
                        placeSelectedPawnTeam = 0;
                        selectedPawnTeam = 0;
                        placeSelectedPawnType = 0;
                        selectedPawnType = 0;
                        return true;
                    }
                }

            }
        }
        placeSelectedPawnType = 0;
        selectedPawnType = 0;
        placeSelectedPawnTeam = 0;
        selectedPawnTeam = 0;
        return false;
    }
    
    public void UpdatePawnInterface(PawnInterface pawn)
    {
        pawn.transform.position = pawn.GamePawn.CurrentPlace.transform.position + new Vector3(0, pawn.GamePawn.CurrentPlace.gameObject.GetComponent<BoxCollider>().size.y / 2, 0);
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


    private void UpdateAllPawnInterfaces()
    {
        for (var i = 0; i < pawns.Length; i++)
        {
            pawns[i].GamePawn = currentGame.Pawns[i];
            pawns[i].transform.position =
           currentGame.Pawns[i].CurrentPlace.transform.position + new Vector3(0, pawns[i].GamePawn.CurrentPlace.gameObject.GetComponent<BoxCollider>().size.y / 2, 0); ;
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

    //Move Animation

    public void MakeMove(PawnInterface pawn, Area place)
    {
        if (CanMove(pawn.GamePawn, place))
        {
            currentStep = GameStep.Moving;
            _movingAnimationPlace = place;
            _movingAnimationDestination = place.transform.position;
            _movingAnimationOrigin = pawn.transform.position;
            _movingAnimationPawn = pawn;
            _movingAnimationTime = 0.0f;
            _selectedPawn = null;
        }
    }

    //Move Code and Item Pickup

    public void Move(Pawn pawn, Area place)
    {
        previousStates.Push(currentGame.DeepCopy());
        pawn.CurrentPlace = place;
        

        if (currentGame.Items.Contains(pawn.HeldItem)) 
        {
            pawn.HeldItem.Position = pawn.CurrentPlace;
        }
        if (pawn.Team == pawn.CurrentPlace.TeamBase && currentGame.Items.Contains(pawn.HeldItem))
        {
            
            if (pawn.Team == TeamType.RED)
            {
                RedScore += pawn.HeldItem.ItemValue;
                currentGame.RedItems.Add(pawn.HeldItem);
            }
            if (pawn.Team == TeamType.BLUE)
            {
                BlueScore += pawn.HeldItem.ItemValue;
                currentGame.BlueItems.Add(pawn.HeldItem);
            }
            currentGame.Items.Remove(pawn.HeldItem);

            RedScoreText.text = "RED SCORE: " + RedScore;
            BlueScoreText.text = "BLUE SCORE: " + BlueScore;


        }

        if (pawn.Color == ConnectionType.RACCOON && pawn.Team != place.TeamBase && place.TeamBase != TeamType.NONE) 
        {

            if (place.TeamBase == TeamType.RED && currentGame.RedItems.Count >= 1)
            {
                foreach (var i in currentGame.RedItems)
                {
                    if (lastItem.Type != ItemType.BLACK) lastItem = i;
                    if (i.Type == ItemType.BLACK) lastItem = i;
                }
                if (!currentGame.Items.Contains(pawn.HeldItem)) pawn.HeldItem = lastItem;
                RedScore -= lastItem.ItemValue;
                RedScoreText.text = "RED SCORE: " + RedScore;
                currentGame.Items.Add(lastItem);
                currentGame.RedItems.Remove(lastItem);               
            }
            if (place.TeamBase == TeamType.BLUE && currentGame.BlueItems.Count >= 1)
            {
                foreach (var i in currentGame.BlueItems)
                {
                    if (lastItem.Type != ItemType.BLACK) lastItem = i;
                    if (i.Type == ItemType.BLACK) lastItem = i;
                }
                if (!currentGame.Items.Contains(pawn.HeldItem)) pawn.HeldItem = lastItem;
                BlueScore -= lastItem.ItemValue;
                BlueScoreText.text = "BLUE SCORE: " + BlueScore;
                currentGame.Items.Add(lastItem);
                currentGame.BlueItems.Remove(lastItem);
            }
        }

        for (var i = currentGame.Items.Count - 1; i >= 0; i--)
        {
            if (currentGame.Items[i].Position == place)
            {
                pawn.HeldItem = currentGame.Items[i];
                //currentGame.Items.RemoveAt(i);
            }
        }
        if (currentGame.Items.Count == 0)
        {
            Debug.Log("Game Ended.");
        }

        if (TeamTurn == TeamType.BLUE) TeamTurn = TeamType.RED;
        else TeamTurn = TeamType.BLUE;
        TurnHalves += 1;
    }

    //============================
    //      Remote Movement
    //============================

    private void CheckForRemoteMovements()
    {
        if (currentStep == GameStep.WaitingForMovement)
        {
            StartCoroutine(WaitForRequest("localhost:3000/getMove"));
        }
    }

    private IEnumerator WaitForRequest(string url)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);

        yield return req.SendWebRequest();
        if (!(req.result == UnityWebRequest.Result.ConnectionError))
        {
            if (req.result == UnityWebRequest.Result.Success)
            {
                //Debug.Log("WWW Ok!: " + req.downloadHandler.text);
                MakeRemoteMove(req.downloadHandler.text);
            }
        }
        else
        {
            //Debug.Log("WWW Error: " + req.error);
        }
    }
    private void MakeRemoteMove(string serverResult)
    {
        if (currentStep != GameStep.WaitingForMovement)
        {
            return;
        }
        try{
            string[] values = serverResult.Split(',');
            Debug.Log(values[0] + "" + values[1]);
            int p = int.Parse(values[0]);
            string d = values[1];
            PawnInterface pawnToMove = pawns[p];
            Area destination = GameObject.Find(d).GetComponent<Area>();

            MakeMove(pawnToMove, destination);
        }
        catch (Exception/* e*/)
        {
            //Debug.Log(e);
        }
    }
}
