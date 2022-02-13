using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement
{
    private Area origin;
    private Area destination;
    private Pawn pawnToMove;
    public Area Origin => origin;
     public Area Destination => destination;
    public Pawn PawnToMove => pawnToMove;

    public Movement(Area areaOrigin, Area areaDestination, Pawn pawn)
    {
        origin = areaOrigin;
        destination = areaDestination;
        pawnToMove = pawn;
    }

    public override string ToString()
    {
        return origin + " -> " + destination;
    }
}
