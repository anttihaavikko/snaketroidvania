using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Snake : SnakePart
{
    public SnakePart tailPrefab;
    public LayerMask collisionMask;

    private Vector3 direction = Vector3.right;

    // Start is called before the first frame update
    void Start()
    {
        AddTail();
        AddTail();
        AddTail();

        Invoke("StartMove", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        var treshold = 0.25f;
        var ix = Input.GetAxisRaw("Horizontal");
        var iy = Input.GetAxisRaw("Vertical");

        var dir = direction;
        if(ix > treshold) dir = Vector3.right;
        else if (ix < -treshold) dir = Vector3.left;
        else if (iy > treshold) dir = Vector3.up;
        else if (iy < -treshold) dir = Vector3.down;

        if (dir != -direction)
            direction = dir;
    }

    void StartMove()
    {
        Move(transform.position + direction);
        Invoke("CheckCollisions", 0.1f);
        Invoke("StartMove", 0.2f);
    }

    void AddTail()
    {
        var part = Instantiate(tailPrefab, transform.parent, true);
        Attach(part);
    }

    void CheckCollisions()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 0.25f, collisionMask);
        //print(string.Join(",", hits.Select(h => h.name)));

        foreach(var h in hits)
        {
            if(h.tag == "Pickup")
            {
                var x = Random.Range(-6, 7);
                var y = Random.Range(-4, 5);
                h.transform.position = new Vector3(x, y, 0);

                AddTail();
            }
        }
    }
}
