using UnityEngine;

public class AItester : MonoBehaviour
{
    HumanoidPawn[] pawns;

    bool alertStatus = false;

    void Start()
    {
        pawns = FindObjectsOfType<HumanoidPawn>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                for (int i = 0; i < pawns.Length; i++)
                    pawns[i]?.Raise(ActorEvent.SetTargetPosition, hit.point);
            }
        }
    }
}
