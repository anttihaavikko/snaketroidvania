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

    private bool hasMoved;

    public void Move(Vector3 pos)
    {
        moveDirection = pos - transform.position;
        Tweener.Instance.MoveTo(transform, RoundVector(pos), 0.12f, 0, TweenEasings.LinearInterpolation);
        //transform.position = pos;

        hasMoved = moveDirection.magnitude > 0.5f;

        if (tail)
        {
            tail.Move(transform.position);

            if(mid)
            {
                Tweener.Instance.MoveTo(mid, (transform.position + pos) * 0.5f, 0.12f, 0, TweenEasings.LinearInterpolation);
            }
        }
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
        } else
        {
            part.transform.position = transform.position;
            tail = part;
            tail.index = index + 1;
            mid.parent = transform.parent;
            part.moveDirection = moveDirection;
            mid.gameObject.SetActive(true);
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
            mid.gameObject.SetActive(true);
            mid.position = (transform.position + tail.transform.position) * 0.5f;
            tail.RepositionMids();
            return;
        }
            
        mid.gameObject.SetActive(false);
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
}
