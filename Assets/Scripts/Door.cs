using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Vector3 direction = Vector3.up;

    private List<DoorSwitch> switches;
    private Vector3 closedPos;


    void Awake()
    {
        switches = new List<DoorSwitch>();
        closedPos = transform.position;
    }

    public void Register(DoorSwitch sw)
    {
        switches.Add(sw);
    }

    public void Check()
    {
        var open = switches.All(s => s.IsOn());
        var target = open ? closedPos + direction : closedPos;
        Tweener.Instance.MoveTo(transform, target, 0.2f, 0, TweenEasings.BounceEaseOut);
    }
}
