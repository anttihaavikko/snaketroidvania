using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Door : MonoBehaviour
{
    public LayerMask collisionMask;
    public Vector3 direction = Vector3.up;

    private List<DoorSwitch> switches;
    private Vector3 closedPos;
    private bool previousState;

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
        var hits = Physics2D.OverlapCircleAll(closedPos, 0.25f, collisionMask);

        if (hits.Any())
        {
            Invoke("Check", 0.5f);
            return;
        }

        var open = switches.All(s => s.IsOn());
        var target = open ? closedPos + direction : closedPos;
        Tweener.Instance.MoveTo(transform, target, 0.2f, 0, TweenEasings.BounceEaseOut);

        if(open != previousState)
        {
            AudioManager.Instance.PlayEffectAt(12, transform.position, 1.045f);
            AudioManager.Instance.PlayEffectAt(9, transform.position, 1f);
            AudioManager.Instance.PlayEffectAt(14, transform.position, 1f);
        }

        previousState = open;
    }
}
