using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField]string header = "REPLACE ME";
    public string Header { get { return header; } }
    //basklass
    //kommer lite skit här sen
    void Start()
    {
        Initalize();
    }

    protected virtual void Initalize()
    {

    }
}
