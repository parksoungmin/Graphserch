using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int id;

    public List<Node> adjacents = new List<Node>();

    public bool CanVisit
    {
        get
        {
            return adjacents.Count > 0;
        }
    }
}
