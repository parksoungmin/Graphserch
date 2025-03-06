using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINode : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;

    private Node Node;

    public void SetNode(Node node)
    {
        this.Node = node;
        SetColor(node.CanVisit ? Color.white : Color.gray);
        SetText($"ID: {node.id}");
    }
    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }


}
