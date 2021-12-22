using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Connection
{
    [SerializeField]
    private Area destination;
    [SerializeField]
    private ConnectionType type;
    public Area Destination => destination;
    public ConnectionType Type => type;

    public Connection(Area destination, ConnectionType type)
    {
        this.destination = destination;
        this.type = type;
    }
}

