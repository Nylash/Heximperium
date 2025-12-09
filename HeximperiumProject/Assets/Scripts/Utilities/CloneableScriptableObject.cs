using UnityEngine;

public abstract class CloneableScriptableObject : ScriptableObject
{
    [SerializeField, HideInInspector]
    private ScriptableObject _originalAsset;

    public ScriptableObject OriginalAsset => _originalAsset ? _originalAsset : this;

    public void SetOriginalAsset(ScriptableObject original)
    {
        _originalAsset = original ? original : this;
    }

    public bool IsSameOriginalAs(CloneableScriptableObject other)
    {
        if (!other) return false;
        return ReferenceEquals(OriginalAsset, other.OriginalAsset);
    }
}
