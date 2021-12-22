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
    public Area Position => position;
    public ItemType Type => type;
}

