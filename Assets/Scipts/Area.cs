using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Area : MonoBehaviour
{
    [SerializeField]
    private List<Connection> connections;

    public List<Connection> Connections => connections;

    private void OnDrawGizmos()
    {
        foreach (var p in connections)
        {
            if (p.Destination == null)
                continue;
            
        var gizmoPathColor = Color.white;
            if (p.Type == ConnectionType.RACCOON)
            {
                gizmoPathColor = Color.blue;
            }
            Gizmos.color = gizmoPathColor;
            Gizmos.DrawLine(transform.position, p.Destination.transform.position);
        }
    }

    private void OnMouseUp()
    {
        GameManager.Instance.Click(this);
    }

    //MAKE ALL CONNECTIONS 2 WAY
    private void Awake()
    {
        foreach (var p in connections)
        {
            List<Connection> pConnections = p.Destination.Connections;
            if (!pConnections.Exists(c => c.Destination == this))
            {
                pConnections.Add(new Connection(this, p.Type));
            }
        }
    }
}
