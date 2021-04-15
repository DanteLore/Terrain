using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    protected virtual void OnValidate()
    {
        if(autoUpdate)
            UnityEditor.EditorApplication.update += ValuesUpdated;
    }

    public void ValuesUpdated() {
        UnityEditor.EditorApplication.update -= ValuesUpdated;
        if(OnValuesUpdated != null)
            OnValuesUpdated();
    }
}
