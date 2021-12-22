using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Pawn
{
    [SerializeField]
    private Area currentPlace;
    [SerializeField] private ConnectionType color;
    public ConnectionType Color => color;

    public Area CurrentPlace
    {
        get => currentPlace;
        set => currentPlace = value;
    }

    public Pawn DeepCopy()
    {
        Pawn copy = new Pawn();
        copy.currentPlace = this.currentPlace;
        copy.color = this.color;
        return copy;
    }

}
