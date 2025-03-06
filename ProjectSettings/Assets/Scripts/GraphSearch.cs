using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GraphSearch
{
    private Graph graph;

    public List<Node> path = new List<Node>();
    public void Init(Graph graph)
    {
        this.graph = graph;
    }

    public void DFS(Node node)
    {
        path.Clear();
        var visited = new HashSet<Node>();
        var stack = new Stack<Node>();

        stack.Push(node);
        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();
            path.Add(currentNode);
            visited.Add(currentNode);
            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent) || stack.Contains(adjacent))
                    continue;

                stack.Push(adjacent);
            }
        }
    }
    public void BFS(Node node) // 최단 경로 찾기 O
    {
        path.Clear();

        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        queue.Enqueue(node);
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            path.Add(currentNode);
            visited.Add(currentNode);
            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent) || queue.Contains(adjacent))
                    continue;

                queue.Enqueue(adjacent);
            }
        }
    }
    public void BFS(Node startNode, Node endNode)
    {
        path.Clear();

        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();
        var parentMap = new Dictionary<Node, Node>();

        queue.Enqueue(startNode); 
        visited.Add(startNode);
        parentMap[startNode] = null;

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();

            if (currentNode == endNode)
            {
                BuildPath(parentMap, endNode);
                return;
            }

            
            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                queue.Enqueue(adjacent);
                visited.Add(adjacent);
                parentMap[adjacent] = currentNode;
            }
        }
    }

    // 경로를 역추적하여 path 리스트 구성
    private void BuildPath(Dictionary<Node, Node> parentMap, Node endNode)
    {
        var current = endNode;
        while (current != null)
        {
            path.Insert(0, current);
            current = parentMap[current];
        }
    }
    public void RecursiveDFS(Node node) // 최단 경로 찾기 X
    {
        path.Clear();
        var visited = new HashSet<Node>();
        RecursiveDFS(node, visited);
    }
    public void RecursiveDFS(Node node, HashSet<Node> visited)
    {
        path.Add(node);
        visited.Add(node);

        foreach (var adjacent in node.adjacents)
        {
            if (!adjacent.CanVisit || visited.Contains(adjacent))
                continue;

            RecursiveDFS(adjacent, visited);
        }
    }
    public void PathFinding(Node start, Node end)
    {
        BFS(start, end);
    }
}
