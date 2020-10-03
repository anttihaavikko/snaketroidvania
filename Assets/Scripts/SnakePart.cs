using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnakePart : MonoBehaviour
{
    public Transform mid;

    private SnakePart tail;

    public void Move(Vector3 pos)
    {
        Tweener.Instance.MoveTo(transform, pos, 0.1f, 0, TweenEasings.LinearInterpolation);
        //transform.position = pos;

        if (tail)
        {
            tail.Move(transform.position);

            if(mid)
            {
                Tweener.Instance.MoveTo(mid, (pos + transform.position) * 0.5f, 0.1f, 0, TweenEasings.LinearInterpolation);
            }
        }
    }

    public void Reset(Vector3 pos)
    {
        transform.position = pos;

        if (mid)
            mid.position = pos;

        if(tail)
            tail.Reset(pos);
    }

    public void Attach(SnakePart part)
    {
        if(tail)
        {
            tail.Attach(part);
        } else
        {
            part.transform.position = transform.position;
            tail = part;
        }
    }
}
