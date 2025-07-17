using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UI_UpgradeNode))]
[CanEditMultipleObjects]
public class UIUpgradeNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Create Connectors"))
        {
            foreach (var obj in targets)
            {
                var node = obj as UI_UpgradeNode;
                if (node == null) continue;

                Undo.RecordObject(node, "Create Connectors");
                node.CreateConnectors();
                EditorUtility.SetDirty(node);
            }
        }
    }
}
