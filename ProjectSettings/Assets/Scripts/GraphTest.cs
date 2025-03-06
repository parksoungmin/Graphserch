using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class GraphTest : MonoBehaviour
{
    public UINode nodePrefab;

    public List<UINode> uiNodes;

    void Start()
    {
        int[,] map = new int[5, 5]
        {
            { 1, -1, 1, 1, 1},
            { 1, -1, 1, 1, 1},
            { 1, -1, 1, 1, 1},
            { 1, -1, 1, 1, 1},
            { 1,  1, 1, 1, 1}
        };

        var graph = new Graph();
        graph.Init(map);

        InitUiNodes(graph);

        var search = new GraphSearch();
        search.Init(graph);
        search.PathFinding(graph.nodes[24],graph.nodes[7]);

        for (int i = 0; i < search.path.Count; i++)
        {
            var node = search.path[i];
            var color = Color.Lerp(Color.red, Color.green, (float)i / search.path.Count);
            uiNodes[node.id].SetColor(color);
            uiNodes[node.id].SetText($"ID: {node.id}\n {i}");
        }
    }

    private void InitUiNodes(Graph graph)
    {
        foreach (var node in graph.nodes)
        {
            var uiNode = Instantiate(nodePrefab,transform);
            uiNode.SetNode(node);
            uiNodes.Add(uiNode);
        }
    }
}
