using UnityEngine;

[System.Serializable]
public struct VitalValuePair
{
    [SerializeField]VitalType type;
    [SerializeField]float value;

    public VitalType Type { get { return type; } }
    public float Value { get { return value; } }

    public VitalValuePair(VitalType type, float value)
    {
        this.type = type;
        this.value = value;
    }
}