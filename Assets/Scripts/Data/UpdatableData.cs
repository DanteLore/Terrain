using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    protected virtual void OnValidate()
    {
        if(autoUpdate)
            ValuesUpdated();
    }

    public void ValuesUpdated() {
        if(OnValuesUpdated != null)
            OnValuesUpdated();
    }
}
