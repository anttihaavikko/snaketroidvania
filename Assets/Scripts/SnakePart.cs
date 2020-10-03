using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnakePart : MonoBehaviour
{
    public Transform mid;
    public int index;
    public PartPool partPool;

    protected SnakePart tail;
    private Vector3 moveDirection;

    public void Move(Vector3 pos)
    {
        moveDirection = pos - transform.position;
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
            tail.index = index + 1;
            mid.parent = transform.parent;
            part.moveDirection = moveDirection;
        }
    }

    public void Chop(int len)
    {
        if (tail)
            tail.Chop(len);

        if (len <= index)
            tail = null;

        if (len < index)
        {
            mid.parent = transform;
            partPool.ReturnToPool(this);
        }
    }

    public SnakePart GetEnd()
    {
        if (tail)
            return tail.GetEnd();

        return this;
    }

    public Vector3 GetReverseSpot()
    {
        if(tail)
            return tail.GetReverseSpot();

        return transform.position;
    }

    public Vector3 GetReverseDirection()
    {
        if (tail)
            return tail.GetReverseDirection();

        return -moveDirection;
    }

    public void ReverseOrder(SnakePart newTail)
    {
        if (tail)
            tail.ReverseOrder(this);

        tail = newTail;
    }

    public void RepositionMids()
    {
        if (tail)
        {
            mid.gameObject.SetActive(true);
            mid.position = (transform.position + tail.transform.position) * 0.5f;
            tail.RepositionMids();
            return;
        }
            
        mid.gameObject.SetActive(false);
    }
}
