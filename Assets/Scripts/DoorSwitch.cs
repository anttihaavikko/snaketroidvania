using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorSwitch : MonoBehaviour
{
    public Door door;
    public LayerMask collisionMask;

    private bool isOn;

    // Start is called before the first frame update
    void Start()
    {
        door.Register(this);
    }

    public bool IsOn()
    {
        return isOn;
    }

    public void Toggle(bool state)
    {
        isOn = state;
        door.Check();
    }

    public bool IsStillOn()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 0.25f, collisionMask);
        var state = hits.Any();
        if (!state)
            Toggle(false);
        return state;
    }
}
