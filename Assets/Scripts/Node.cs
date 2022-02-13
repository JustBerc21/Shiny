using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class represents a node to be used with AI algorithms
/// </summary>

public class Node
{
    private GameState theGameState;

    public GameState TheGameState => theGameState;


    private float heuristic = 0;


    private List<Movement> previousMovements = new List<Movement>();

    public List<Movement> PreviousMovements => previousMovements;



    private List<Node> children = new List<Node>();

    public List<Node> Children => children;


    public int Score { get; set; }



    public Node (GameState gs)
    {
        theGameState = gs;
    }



    // In this example, the heuristic is calculated considering a goal place
    public void CalculateHeuristicValue_ShortestPath (Area goal)
    {
        Vector3 pawnPosition = theGameState.Pawns[0].CurrentPlace.transform.position;
        Vector3 goalPosition = goal.transform.position;

        // The heuristic is the Manhatan distance to the place
        // (divided by 2 because in the scene, the positions are 2 units apart in the "virtual grid")
        heuristic = (Mathf.Abs(pawnPosition.x - goalPosition.x) +
            Mathf.Abs(pawnPosition.z - goalPosition.z)) / 2;
        
    }

    public void CalculateHeuristicValue_FullGame ()
    {
        heuristic = 1;
        // Basic heuristic could be the number of remaining items to catch
    }


    public float GetEstimatedCost ()
    {
        return GetCurrentCost() + heuristic;
    }


    private float GetCurrentCost ()
    {
        return previousMovements.Count;
    }


    public string GetPath ()
    {
        string s = previousMovements[0].Origin.ToString();
        foreach (var m in previousMovements)
        {
            s += m.Destination + " ";
        }
        return s;
    }


    public override string ToString()
    {
        string s = "";

        s += " Depth: " + previousMovements.Count;
        s += " Score: " + Score;
        s += " State: " + theGameState.ToString();
        s += " Last Move: " + (previousMovements.Count > 0 ? previousMovements[previousMovements.Count - 1].ToString() : "");

        return s;
   
    }


}
