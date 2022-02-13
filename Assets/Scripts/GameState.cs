using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameState
{
    public bool Singleplayer = false;

    //Turns
    public int Turns = 1;
    public int TurnHalves;
    public TeamType TeamTurn;

    [SerializeField] private List<Pawn> pawns;
    //[SerializeField] private Item lastItem;
    //Score
    [SerializeField] public int RedScore;
    [SerializeField] public int BlueScore;

    public List<Pawn> Pawns => pawns;

    [SerializeField]
    private List<Item> items;
    [SerializeField] private List<Item> redItems;
    [SerializeField] private List<Item> blueItems;
    public List<Item> Items => items;

    /*public int BlueScore
    {
        get => blueScore;
        set => blueScore = value;
    }

    public int RedScore
    {
        get => redScore;
        set => redScore = value;
    }*/


    public List<Item> BlueItems
    {
        get => blueItems;
        set => blueItems = value;
    }
    public List<Item> RedItems
    {
        get => redItems;
        set => redItems = value;
    }

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

    public Movement[] GetAllMovements()
    {
        List<Movement> allMovements = new List<Movement>();
        foreach (var p in Pawns)
        {
            if (p.Team == TeamTurn)
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
        }
        return allMovements.ToArray();
    }



    public void Move(Pawn pawn, Area place)
    {
        
        pawn.CurrentPlace = place;


        if (Items.Contains(pawn.HeldItem))
        {
            pawn.HeldItem.Position = pawn.CurrentPlace;
        }
        if (pawn.Team == pawn.CurrentPlace.TeamBase && Items.Contains(pawn.HeldItem))
        {

            if (pawn.Team == TeamType.RED)
            {
                RedScore += pawn.HeldItem.ItemValue;
                RedItems.Add(pawn.HeldItem);
            }
            if (pawn.Team == TeamType.BLUE)
            {
                BlueScore += pawn.HeldItem.ItemValue;
                BlueItems.Add(pawn.HeldItem);
            }
            Items.Remove(pawn.HeldItem);


        }

        if (pawn.Color == ConnectionType.RACCOON && pawn.Team != place.TeamBase && place.TeamBase != TeamType.NONE)
        {
            
            if (place.TeamBase == TeamType.RED && RedItems.Count >= 1)
            {
                var lastItem = new Item();
                foreach (var i in RedItems)
                {
                    if (lastItem.Type != ItemType.BLACK) lastItem = i;
                    if (i.Type == ItemType.BLACK) lastItem = i;
                }
                if (!Items.Contains(pawn.HeldItem)) pawn.HeldItem = lastItem;
                RedScore -= lastItem.ItemValue;
                Items.Add(lastItem);
                RedItems.Remove(lastItem);
            }
            if (place.TeamBase == TeamType.BLUE && BlueItems.Count >= 1)
            {
                var lastItem = new Item();
                foreach (var i in BlueItems)
                {
                    if (lastItem.Type != ItemType.BLACK) lastItem = i;
                    if (i.Type == ItemType.BLACK) lastItem = i;
                }
                if (!Items.Contains(pawn.HeldItem)) pawn.HeldItem = lastItem;
                BlueScore -= lastItem.ItemValue;
                Items.Add(lastItem);
                BlueItems.Remove(lastItem);
            }
        }

        for (var i = Items.Count - 1; i >= 0; i--)
        {
            if (Items[i].Position == place)
            {
                var used = false;
                foreach (var p in Pawns) 
                {
                    if (p.HeldItem == Items[i]) used = true;
                }
                if(!used)pawn.HeldItem = Items[i];
                //currentGame.Items.RemoveAt(i);
            }
        }
        if (Items.Count == 0)
        {
            Debug.Log("Game Ended.");
        }
        TurnUpdate();
    }

    public bool GameEnded()
    {
        //return Items.Count == 0;
        return Pawns[0].HeldItem != null;
    }

    private void TurnUpdate() 
    {
        if (Singleplayer) Turns += 1;
        else
        {
            //Swap Team Turn
            if (TeamTurn == TeamType.BLUE) TeamTurn = TeamType.RED;
            else TeamTurn = TeamType.BLUE;
            TurnHalves += 1;
            if (TurnHalves >= 2)
            {
                Turns += 1;
                TurnHalves = 0;
            }
        }
    }
}
