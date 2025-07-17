using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeTree
{
    public GameObject treeObject;
    public List<UI_UpgradeNode> nodes = new List<UI_UpgradeNode>();
}
