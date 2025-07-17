using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_UpgradeNode : MonoBehaviour
{
    [SerializeField] private UpgradeNodeData _nodeData;
    [Tooltip("Let empty if this node is the first.")]
    [SerializeField] private List<UI_UpgradeNode> _previousNodes;
    [SerializeField] private RectTransform _parent;
    [Tooltip("If false, the line will be drawn horizontally from the start point to the mid point, then vertically to the end point.")]
    [SerializeField] private bool _stepOnMid = true;
    [SerializeField] private List<UILineRenderer> _connectors = new List<UILineRenderer>();

    private Button _btn;
    private Image _img;
    private Image _imageExclusiveMarker;

    public UpgradeNodeData NodeData { get => _nodeData; }

    private void Awake()
    {
        _btn = GetComponent<Button>();
        _img = GetComponent<Image>();
        _btn.onClick.AddListener(() => UpgradesManager.Instance.UnlockNode(this));

        UpdateVisual();

        if(_nodeData.ExclusiveNode != null)
        {
            AddExclusiveMarker();
            UpgradesManager.Instance.OnNodeUnlocked += node =>
            {
                if (node.NodeData != _nodeData.ExclusiveNode) return;
                _imageExclusiveMarker.sprite = UIManager.Instance.MarkerExclusiveUpgradeLocked;
            };
        }
    }

    public void UpdateVisual()
    {
        switch (UpgradesManager.Instance.CanUnlockNode(_nodeData))
        {
            case UpgradeStatus.LockedByPrerequisites:
                _btn.interactable = false;
                _img.color = UIManager.Instance.ColorLocked;
                UpdateLinesColor(UIManager.Instance.ColorLocked);
                break;
            case UpgradeStatus.LockedByExclusive:
                _btn.interactable = false;
                _img.color = UIManager.Instance.ColorLocked;
                UpdateLinesColor(UIManager.Instance.ColorLocked);
                break;
            case UpgradeStatus.CantAfford:
                _btn.interactable = false;
                _img.color = UIManager.Instance.ColorCantAfford;
                UpdateLinesColor(UIManager.Instance.ColorCantAfford);
                break;
            case UpgradeStatus.Unlocked:
                _btn.interactable = false;
                _img.color = UIManager.Instance.ColorUnlocked;
                _img.sprite = UIManager.Instance.SpriteUnlocked;
                UpdateLinesColor(UIManager.Instance.ColorUnlocked);
                if(_imageExclusiveMarker != null)
                    Destroy(_imageExclusiveMarker.gameObject);
                break;
            case UpgradeStatus.Unlockable:
                _btn.interactable = true;
                _img.color = Color.white;
                UpdateLinesColor(Color.white);
                break;
            default:
                break;
        }
    }

    public void CreateConnectors()
    {
        foreach (UILineRenderer line in _connectors)
        {
            if (line != null)
                DestroyImmediate(line.gameObject);
        }
        _connectors.Clear();

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

    private void AddExclusiveMarker()
    {
        // 1. Get your RectTransform and its parent
        RectTransform rt = GetComponent<RectTransform>();
        RectTransform parent = rt.parent as RectTransform;

        // normalized between bottom‑left and top‑left
        Vector2 normAnchor = new Vector2(
            rt.anchorMin.x,
            (rt.anchorMin.y + rt.anchorMax.y) * 0.5f
        );

        // 3. Compute that point in the parent’s local space
        Vector2 localPoint = Vector2.Scale(parent.rect.size, normAnchor) + parent.rect.min;

        // 4. Transform to world space
        Vector3 worldPoint = parent.TransformPoint(localPoint);

        // 5. Convert to screen‐space (pixels) respecting your Canvas mode
        Canvas canvas = rt.GetComponentInParent<Canvas>();
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                     ? null
                     : canvas.worldCamera;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPoint);

        // screenPos now holds the anchor’s position in screen pixels
        GameObject marker = Instantiate(UIManager.Instance.MarkerExclusiveUpgrade, rt.parent, false);
        marker.transform.position = screenPos;

        _imageExclusiveMarker = marker.GetComponent<Image>();

        Utilities.ReanchorToCurrentRect(marker.GetComponent<RectTransform>());
    }

    private void UpdateLinesColor(Color targetColor)
    {
        if (_connectors.Count < 2)
        {
            foreach (UILineRenderer line in _connectors)
                    line.color = targetColor;
        }
        else//If there are multiple connectors it means that the node is connected to multiple previous nodes
        {
            for (int i = 0; i < _connectors.Count; i++)
            {
                //If the previous node is locked by exclusive, the line should be colored as locked, otherwise the target color is coherent with the node status
                if (UpgradesManager.Instance.CanUnlockNode(_previousNodes[i].NodeData) == UpgradeStatus.LockedByExclusive)
                    _connectors[i].color = UIManager.Instance.ColorLocked;
                else
                    _connectors[i].color = targetColor;
            }
        }
    }
}
