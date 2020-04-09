using UnityEngine;

public class AbilityController : MonoBehaviour
{
    [SerializeField]ForcePush push;
    [SerializeField]ForcePull pull;
    [SerializeField]SlowTime slow;

    Ability[] abilities;

    KeyCode[] abilityHotkeys = new KeyCode[]
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
        KeyCode.Alpha0,
    };

    void Start()
    {
        abilities = new Ability[] { push, pull, slow };
    }
    void Update()
    {
        for (int i = 0; i < abilities.Length; i++)
        {
            if (Input.GetKeyDown(abilityHotkeys[i]) && !abilities[i].HasCooldown)
                abilities[i].Activate();
            else if (Input.GetKeyUp(abilityHotkeys[i]))
                abilities[i].Deactivate();
        }

        for (int i = 0; i < abilities.Length; i++)
            if (abilities[i].IsActive)
                abilities[i].Tick();
    }
}
