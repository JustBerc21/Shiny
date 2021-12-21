using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spot : MonoBehaviour
{
    [SerializeField]  List<GameObject> connections;


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;

        foreach (GameObject i in connections) 
        {
            Gizmos.DrawLine(transform.position, i.gameObject.transform.position);
        }
    }
}
