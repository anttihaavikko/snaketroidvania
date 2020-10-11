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
    protected int moveCount;
    private readonly float moveDuration = 0.12f;

    public Vector3 Move(Vector3 pos)
    {
        var dir = pos - transform.position;
        if (dir != Vector3.zero)
            moveDirection = dir;

        Tweener.Instance.MoveTo(transform, RoundVector(pos), moveDuration, 0, TweenEasings.LinearInterpolation);

        if(moveDirection.magnitude > 0.5f)
            moveCount++;

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
        return moveCount > 1;
    }

    public void Reset(Vector3 pos)
    {
        transform.position = pos;
        moveCount = 0;

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
            tail.gameObject.name = "Tail " + tail.index;
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

        print("Get dir from " + gameObject.name);
        return -moveDirection;
    }

    public void ReverseOrder(SnakePart newTail)
    {
        if (tail)
            tail.ReverseOrder(this);

        tail = newTail;
        moveCount = 0;
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
