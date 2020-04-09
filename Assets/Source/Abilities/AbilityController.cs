using UnityEngine;

public class AbilityController : MonoBehaviour
{
    [SerializeField]ForcePush push;

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
        abilities = new Ability[] { push };
    }
    void Update()
    {
        for (int i = 0; i < abilityHotkeys.Length; i++)
        {
            if (Input.GetKeyDown(abilityHotkeys[i]) && !abilities[i].hasCooldown)
            {
                Debug.Log("activating " + abilities[i].GetType().ToString());

                Context c = new Context();
                c.data.Add("caster", this.transform);

                abilities[i].Activate(c);
            }
        }

        for (int i = 0; i < abilities.Length; i++)
            abilities[i].Tick();
    }
}
