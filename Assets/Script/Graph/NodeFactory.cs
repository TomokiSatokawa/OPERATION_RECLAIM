using Graph;
using UnityEngine;

public class NodeFactory : MonoBehaviour
{
    [SerializeField] private NodeConnector _connector;
    [SerializeField] private ConditionGroupGenerator _generator;
    [SerializeField] private Transform _content;
    [SerializeField] private RectTransform _parentTransform;
    [SerializeField] private Camera _canvasCamera;

    public void CreateNode(Node node)
    {
        var clonedNode = Instantiate(node,_content);
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
               _parentTransform,
              screenCenter,
               _canvasCamera,
               out Vector2 position))
        {
            return;
        }

        clonedNode.Rect.anchoredPosition = position;
        clonedNode.Initialize(_connector, _generator);
        GraphDataContainer.SaveNode(clonedNode);
    }
}
