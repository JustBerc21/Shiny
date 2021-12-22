using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameState
{
    [SerializeField] private List<Pawn> pawns;

    public List<Pawn> Pawns => pawns;

    [SerializeField]
    private List<Item> items;
    public List<Item> Items => items;

    public GameState DeepCopy()
    {
        GameState copy = new GameState();
        copy.pawns = new List<Pawn>();
        for (var i = 0; i < this.pawns.Count; i++)
        {
            copy.pawns.Add(this.pawns[i].DeepCopy());
        }
        copy.items = new List<Item>(this.items);
        return copy;
    }

}
