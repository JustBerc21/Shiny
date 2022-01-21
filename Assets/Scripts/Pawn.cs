using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Pawn
{
    [SerializeField] private Area currentPlace;
    [SerializeField] private Item heldItem = null;
    [SerializeField] private ConnectionType color;
    [SerializeField] private TeamType team;
    public ConnectionType Color => color;
    public TeamType Team => team;

    public Area CurrentPlace
    {
        get => currentPlace;
        set => currentPlace = value;
    }


    public Item HeldItem
    {
        get => heldItem;
        set => heldItem = value;
    }


    public Pawn DeepCopy()
    {
        Pawn copy = new Pawn();
        copy.currentPlace = this.currentPlace;
        copy.color = this.color;
        return copy;
    }

}
