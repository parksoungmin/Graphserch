using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Transactions;
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Rendering;

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
    public void BFS(Node node) // �ִ� ��� ã�� O
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

    // ��θ� �������Ͽ� path ����Ʈ ����
    private void BuildPath(Dictionary<Node, Node> parentMap, Node endNode)
    {
        var current = endNode;
        while (current != null)
        {
            path.Insert(0, current);
            current = parentMap[current];
        }
    }
    public void RecursiveDFS(Node node) // �ִ� ��� ã�� X
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

    public bool PathFindingBFS(Node start, Node end)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        queue.Enqueue(start);
        bool success = false;
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            if (currentNode == end)
            {
                success = true;
                break;
            }

            visited.Add(currentNode);
            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;
                queue.Enqueue(adjacent);
                adjacent.previous = currentNode;
            }
        }
        if (success)
        {
            return false;
        }
        Node step = end;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();
        return true;
    }

    public bool Dikjstra(Node start, Node end)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<Node>();
        var queue = new PriorityQueue<Node, int>(Comparer<int>.Create((x, y) => x.CompareTo(y)));

        var distances = new int[graph.nodes.Length];

        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = int.MaxValue;
        }

        distances[start.id] = 0;
        queue.Enqueue(start, distances[start.id]);
        bool success = false;
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            if (visited.Contains(currentNode))
            {
                continue;
            }
            if (currentNode == end)
            {
                success = true;
                break;
            }
            visited.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit)
                    continue;

                var newDistance = distances[currentNode.id] + adjacent.weight;
                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    adjacent.previous = currentNode;
                    queue.Enqueue(adjacent, newDistance);
                }
            }
        }

        if (!success)
        {
            return false;
        }
        Node step = end;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();
        
        return true;
    }

    private int Heuristic(Node a, Node b) // �߰߹�
    {
        int ax = a.id % graph.cols; // ���� ������ a�� x��ǥ�� ����
        int ay = a.id / graph.cols; // ���� ������ a�� y��ǥ�� ����


        int bx = b.id % graph.cols; // ���� ������ b�� x��ǥ�� ����
        int by = b.id / graph.cols; // ���� ������ b�� y��ǥ�� ����

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    public bool AStar(Node start, Node end)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<Node>();
        var queue = new PriorityQueue<Node, int>(Comparer<int>.Create((x, y) => x.CompareTo(y)));

        var distances = new int[graph.nodes.Length]; // Ȯ���� �Ÿ� ���
        var scores = new int[graph.nodes.Length];

        for (int i = 0; i < distances.Length; ++i)
        {
            scores[i] = distances[i] = int.MaxValue; // ������ ������������ ���� ��ģ��
        }

        distances[start.id] = 0;
        scores[start.id] = Heuristic(start,end);
        queue.Enqueue(start, distances[start.id]);

        bool success = false;
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            if (visited.Contains(currentNode))
            {
                continue;
            }
            if (currentNode == end)
            {
                success = true;
                break;
            }
            visited.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit)
                    continue;

                var newDistance = distances[currentNode.id] + adjacent.weight;
                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    scores[adjacent.id] = distances[adjacent.id] + Heuristic(adjacent, end);

                    adjacent.previous = currentNode;
                    queue.Enqueue(adjacent, scores[adjacent.id]);
                }
            }
        }

        if (!success)
        {
            return false;
        }
        Node step = end;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();

        return true;
    }

}
