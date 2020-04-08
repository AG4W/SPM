using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTime : MonoBehaviour
{
    [SerializeField] float timeScale;
    [SerializeField] float timeSlowDuration = 1;

    private bool active = false;
    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!active)
        {
            Time.timeScale = 1;

            if (Input.GetKeyDown(KeyCode.Q))
            {
                active = true;
            }
        }
        else
        {
            Time.timeScale = this.timeScale;
            timer += Time.deltaTime;
            if (timer > timeSlowDuration * timeScale)
            {
                active = false;
                timer = 0;
            }
        }
    }


}
