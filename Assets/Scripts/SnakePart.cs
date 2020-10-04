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
    protected Vector3 moveDirection;

    //private Vector3 target, tailTarget;
    private bool hasMoved;
    private readonly float moveDuration = 0.12f;

    public Vector3 Move(Vector3 pos)
    {
        moveDirection = pos - transform.position;
        Tweener.Instance.MoveTo(transform, RoundVector(pos), moveDuration, 0, TweenEasings.LinearInterpolation);

        hasMoved = moveDirection.magnitude > 0.5f;

        if (tail)
        {
            var tp = tail.Move(transform.position);
            var sum = RoundVector(pos + tp);
            Tweener.Instance.MoveTo(mid, sum * 0.5f, moveDuration, 0, TweenEasings.LinearInterpolation);
        }
        else
        {
            Tweener.Instance.MoveTo(mid, pos, moveDuration, 0, TweenEasings.LinearInterpolation);
        }

        return RoundVector(pos);
    }

    public bool HasMoved()
    {
        return hasMoved;
    }

    public void Reset(Vector3 pos)
    {
        transform.position = pos;
        hasMoved = false;

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
        }
        else
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
        hasMoved = false;
    }

    public void RepositionMids()
    {
        if (tail)
        {
            mid.position = (transform.position + tail.transform.position) * 0.5f;
            tail.RepositionMids();
        }
    }

    public void Reindex(int prev)
    {
        index = prev + 1;
        if (tail)
            tail.Reindex(index);
    }

    public static Vector3 RoundVector(Vector3 v)
    {
        return new Vector3(
            Mathf.Round(v.x),
            Mathf.Round(v.y),
            Mathf.Round(v.z)
        );
    }

    public void Explode()
    {
        EffectManager.Instance.AddEffect(0, transform.position);
        EffectManager.Instance.AddEffect(2, transform.position);
        EffectManager.Instance.AddEffect(3, transform.position);
        EffectManager.Instance.AddEffect(4, transform.position);

        if (tail)
            tail.Explode();
    }
}
