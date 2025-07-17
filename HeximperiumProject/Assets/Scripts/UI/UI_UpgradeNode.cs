using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_UpgradeNode : MonoBehaviour
{
    [SerializeField] private UpgradeNodeData _nodeData;
    [SerializeField] private List<UI_UpgradeNode> _previousNodes;
    [SerializeField] private RectTransform _parent;
    [Tooltip("If false, the line will be drawn horizontally from the start point to the mid point, then vertically to the end point.")]
    [SerializeField] private bool _stepOnMid = true;


    private Button _btn;
    private List<UILineRenderer> _connectors = new List<UILineRenderer>();

    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(() => UpgradesManager.Instance.UnlockNode(_nodeData));
    }

    public void UpdateVisual()
    {
        //Update the button's interactable state and connectors' state based on the node's cost, prerequisites and exclusivity
    }

    //[ContextMenu("Create Connectors")] Right click in the inspector to create connectors manually
    public void CreateConnectors()
    {
        //Create connectors to previous nodes
        foreach (UI_UpgradeNode previousNode in _previousNodes)
        {
            UILineRenderer line = Instantiate(UIManager.Instance.LineRendererPrefab, _parent).GetComponent<UILineRenderer>();

            Vector2 startPoint = RectTransformUtility.WorldToScreenPoint(null, transform.position);
            Vector2 endPoint = RectTransformUtility.WorldToScreenPoint(null, previousNode.transform.position);

            Vector2 startLocal, endLocal;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, startPoint, null, out startLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, endPoint, null, out endLocal);

            //Check if the start and end points are aligned vertically
            if (!Mathf.Approximately(startLocal.y, endLocal.y))
            {
                float midX;
                if (_stepOnMid)
                    midX = (startLocal.x + endLocal.x) * 0.5f;
                else
                    midX = endLocal.x;

                // step: horizontal → vertical → horizontal
                Vector2 step1 = new Vector2(midX, startLocal.y);
                Vector2 step2 = new Vector2(midX, endLocal.y);

                line.points = new[] { startLocal, step1, step2, endLocal };
            }
            else
            {
                // flat line: just two points
                line.points = new[] { startLocal, endLocal };
            }

            line.SetVerticesDirty();

            line.gameObject.name = $"Line_{previousNode.gameObject.name}_to_{gameObject.name}";
            _connectors.Add(line);
        }
    }
}
