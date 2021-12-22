using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnInterface : MonoBehaviour
{
    private Pawn gamePawn;

    public Pawn GamePawn
    {
        get => gamePawn;
        set => gamePawn = value;
    }

    private void OnMouseUp()
    {
        GameManager.Instance.Click(this);
    }

}
