using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPlayer
{
    public void RandomMove(GameState state, out Pawn pawn, out Area place)
    {

        Movement[] possibleMovements = GameManager.Instance.GetAllMovements();
        if (possibleMovements != null && possibleMovements.Length > 0)
        {
            int r = Random.Range(0, possibleMovements.Length);
            Movement move = possibleMovements[r];
            pawn = move.PawnToMove;
            place = move.Destination;
        }
        else
        {
            pawn = null;
            place = null;
            Debug.LogWarning("Couldn't find a valid move.");
        }

    }
}
