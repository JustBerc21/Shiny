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
    [SerializeField] private Text TurnText;

    //Score
    [SerializeField] Text RedScoreText;
    [SerializeField] Text BlueScoreText;

    [SerializeField] private GameState currentGame;
    public GameState CurrentGame => currentGame;
    [SerializeField] private List<PawnInterface> pawns;
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
    

    public static GameManager Instance { get; private set; }
    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Multiple Game Managers.");
        }
        Instance = this;

        for (var i = 0; i < pawns.Count; i++)
        {
            pawns[i].GamePawn = currentGame.Pawns[i];
            pawns[i].transform.position =
           currentGame.Pawns[i].CurrentPlace.transform.position + new Vector3(0, currentGame.Pawns[i].CurrentPlace.gameObject.GetComponent<BoxCollider>().size.y / 2, 0);
        }

        places = FindObjectsOfType<Area>();
        previousStates = new Stack<GameState>();
        currentStep = GameStep.WaitingForMovement;

        InvokeRepeating("CheckForRemoteMovements", 0, 1.0f);


        //teams
        int r = UnityEngine.Random.Range(0, 2);
        if (r == 0 || currentGame.Singleplayer) currentGame.TeamTurn = TeamType.RED;
        else currentGame.TeamTurn = TeamType.BLUE;

        //Items
        foreach (var i in currentGame.Items)
        {
            int rp = UnityEngine.Random.Range(0, places.Length);
            i.Position = places[rp];
            foreach (var i2 in currentGame.Items)
            {
                int rp2 = UnityEngine.Random.Range(0, places.Length);
                int rp3 = UnityEngine.Random.Range(0, places.Length);
                if (i2.Position == i.Position) i.Position = places[rp2];
                if (i.Position.TeamBase != TeamType.NONE) i.Position = places[rp3];
            }
            if (i.itemObject != null) i.itemObject.transform.position = i.Position.transform.position + new Vector3(0,1,0);
        }
    }

    /// 
    /// UPDATE
    ///
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
            if (Input.GetKeyUp(KeyCode.A))
            {
                MakeAIMove();
            }

            if (Input.GetKeyUp(KeyCode.M))
            {
                MiniMax();
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

        TurnTextUpdate();
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
        else if (pawn.GamePawn.Team == currentGame.TeamTurn)
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
        for (var i = 0; i < pawns.Count; i++)
        {
            pawns[i].GamePawn = currentGame.Pawns[i];
            pawns[i].transform.position =
           currentGame.Pawns[i].CurrentPlace.transform.position + new Vector3(0, pawns[i].GamePawn.CurrentPlace.gameObject.GetComponent<BoxCollider>().size.y / 2, 0); ;
        }

        foreach (var i in currentGame.Items)
        {
            if (i.itemObject != null)i.itemObject.transform.position = i.Position.transform.position + new Vector3(0, 1, 0);
        }
        foreach (var i in currentGame.RedItems) 
        {
            int o = 0;
            i.itemObject.transform.position = new Vector3(0, 1, -20 - o);
            o += 2;
        }
        foreach (var i in currentGame.BlueItems)
        {
            int o = 0;
            i.itemObject.transform.position = new Vector3(0, 1, 20 + o);
            o += 2;
        }
    }

    private void TurnTextUpdate() 
    {
        if (currentGame.TeamTurn == TeamType.RED) TurnText.color = Color.red;
        else TurnText.color = Color.blue;
        TurnText.text = "Turn " + currentGame.Turns;
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
        currentGame.Move(pawn, place);

        //Score Update
        RedScoreText.text = "RED SCORE: " + currentGame.RedScore;
        BlueScoreText.text = "BLUE SCORE: " + currentGame.BlueScore;

        if (currentGame.GameEnded())
        {
            Debug.Log("Game Ended");
        }
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


    //
    //AI MOVE
    //
    public void MakeAIMove()
    {
        // Duplicates the current state
        var initialState = currentGame.DeepCopy();

        // List of open and closed nodes
        List<Node> openNodes = new List<Node>();
        List<Node> closedNodes = new List<Node>();

        // Initial Node, which is added to the list of open nodes
        Node initialNode = new Node(initialState);
        initialNode.CalculateHeuristicValue_FullGame();
        openNodes.Add(initialNode);

        var iterationsLimit = 1000;
        int count = 0;
        while (openNodes.Count > 0)
        {
            count++;

            // Debug Info 
            Debug.Log(count + ") Open Nodes");
            foreach (Node n in openNodes)
            {
                Debug.Log(n);
            }
            Debug.Log(count + ") Closed Nodes");
            foreach (Node n in closedNodes)
            {
                Debug.Log(n);
            }
            Debug.Log("-----------------------------------");


            // Picks a node from the open set (which is set as the current) and moves it to the closed set
            // !!! Currently picks a random one instead of the best estimative (A*)
            var bestNode = openNodes[UnityEngine.Random.Range(0, openNodes.Count)];
            openNodes.Remove(bestNode);
            closedNodes.Add(bestNode);


            // Calculates all possible movements of the current node
            var allMovements = bestNode.TheGameState.GetAllMovements();

            foreach (Movement m in allMovements)
            {
                // Creates a new node (the state is same as the node we are expanding)
                Node newNode = new Node(bestNode.TheGameState.DeepCopy());

                // Register the set of movements to reach this place
                // (the movements of the previous node plus the new movement)
                newNode.PreviousMovements.AddRange(bestNode.PreviousMovements);
                newNode.PreviousMovements.Add(m);


                // --- Makes the game move ---

                // Finds the pawn
                var pawn = newNode.TheGameState.Pawns.Find(p => p.CurrentPlace == m.PawnToMove.CurrentPlace);

                newNode.TheGameState.Move(pawn, m.Destination);


                // Is this the desired state? (all items collected, meaning end of the game)
                if (newNode.TheGameState.GameEnded())
                {
                    Movement selectedMove;
                    if (bestNode.PreviousMovements.Count > 0)
                    {
                        selectedMove = bestNode.PreviousMovements[0];
                    }
                    else
                    {
                        selectedMove = m;
                    }

                    print("Found Solution! Make this move " + selectedMove.Origin + " " + selectedMove.Destination);
                    print("Number of nodes " + closedNodes.Count);

                    // This is it (pick the move)
                    // Note: the identified pawn is in the game state clone
                    // we need to identify the corresponding pawn in the game's current state
                    // (the pawn that has the same position)
                    var pi = pawns.Find(p => p.GamePawn.CurrentPlace == selectedMove.Origin);
                    MakeMove(pi, selectedMove.Destination);

                    return;
                }

                // Is this node in the open set? If so, ignore.
                if (openNodes.Any(n => n.TheGameState.Equals(newNode.TheGameState)))
                {
                    continue;
                }

                // Is this node in the closed set? If so, ignore.
                if (closedNodes.Any(n => n.TheGameState.Equals(newNode.TheGameState)))
                {
                    continue;
                }

                // Otherwise, adds the generated node to the set of open nodes for further analysis
                openNodes.Add(newNode);
                newNode.CalculateHeuristicValue_FullGame();
            }

            if (count >= iterationsLimit)
            {
                Debug.Log("Too many interations");
                break;
            }

        }

        // If the flow reaches this place, there is no solution
        Debug.Log("Can't find a move");
    }

    //
    //MINIMAX
    //

    private void MiniMax()
    {

        // Duplicates the current state
        var initialState = currentGame.DeepCopy();

        // List of open and closed nodes
        List<Node> openNodes = new List<Node>();
        List<Node> closedNodes = new List<Node>();

        Node initialNode = new Node(initialState);
        openNodes.Add(initialNode);

        var maxDepth = 5;
        while (openNodes.Count > 0)
        {

            // Debug Info 
            //Debug.Log("Open Nodes");
            //foreach (Node n in openNodes)
            //{
            //    Debug.Log(n);
            //}
            //Debug.Log("Closed Nodes");
            //foreach (Node n in closedNodes)
            //{
            //    Debug.Log(n);
            //}
            //Debug.Log("-----------------------------------");


            // Picks the first node in the open set (this works as a breadth-first)
            var bestNode = openNodes[0];
            openNodes.Remove(bestNode);
            closedNodes.Add(bestNode);

            // Skips expansion for leaf nodes (game ended or max depth)
            if (bestNode.TheGameState.GameEnded() || bestNode.PreviousMovements.Count > maxDepth)
            {
                continue;
            }


            // Calculates all possible movements of the current node
            var allMovements = bestNode.TheGameState.GetAllMovements();

            foreach (Movement m in allMovements)
            {
                // Creates a new node (the state is same as the node we are expanding)
                Node newNode = new Node(bestNode.TheGameState.DeepCopy());
                bestNode.Children.Add(newNode);

                // Register the set of movements to reach this place
                // (the movements of the previous node plus the new movement)
                newNode.PreviousMovements.AddRange(bestNode.PreviousMovements);
                newNode.PreviousMovements.Add(m);


                // Makes the game move
                // First finds the pawn...
                var pawn = newNode.TheGameState.Pawns.Find(p => p.CurrentPlace == m.PawnToMove.CurrentPlace);
                // ... and then orders the move
                newNode.TheGameState.Move(pawn, m.Destination);


                // Otherwise, adds the generated node to the set of open nodes for further analysis
                openNodes.Add(newNode);

            }

        }

        // Prints all nodes
        Debug.Log("Tree expanded");
        foreach (Node n in closedNodes)
        {
            Debug.Log(n);
        }




        // Gets the best score for player 2 within the moves
        int bestScorePlayer1 = closedNodes.Max(n => n.TheGameState.BlueScore);

        // Picks a random node with the best score
        List<Node> bestMoves = closedNodes.FindAll(n => n.TheGameState.RedScore == bestScorePlayer1);
        Node randomBestNode = bestMoves[UnityEngine.Random.Range(0, bestMoves.Count)];

        // Makes that move
        Movement selectedMove = randomBestNode.PreviousMovements[0];
        var pi = pawns.Find(p => p.GamePawn.CurrentPlace == selectedMove.PawnToMove.CurrentPlace);
        MakeMove(pi, selectedMove.Destination);

    }
    
}
