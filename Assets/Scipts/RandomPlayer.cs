using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPlayer
{
    public void RandomMove(GameState state)
    {

        Movement[] possibleMovements = GameManager.Instance.GetAllMovements();
        if (possibleMovements != null && possibleMovements.Length > 0)
        {
            int r = Random.Range(0, possibleMovements.Length);
            Movement move = possibleMovements[r];
            GameManager.Instance.Move(move.PawnToMove, move.Destination);
        }
        else
        {
            Debug.LogWarning("Couldn't find a valid move.");
        }

    }
}
