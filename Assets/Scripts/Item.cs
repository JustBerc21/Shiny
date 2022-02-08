using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Item
{
    [SerializeField]
    private Area position;
    [SerializeField]
    private ItemType type;
    [SerializeField]
    private int itemValue;
    [SerializeField]
    public GameObject itemObject;

    public Area Position
    {
        get => position;
        set => position = value;
    }
    public ItemType Type => type;
    public int ItemValue => itemValue;

}

