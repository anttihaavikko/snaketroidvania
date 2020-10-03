using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Snake : SnakePart
{
    public SnakePart tailPrefab;
    public LayerMask collisionMask;
	public Transform camRig;
    public Room currentRoom;

    private Vector3 direction = Vector3.right;

    private Vector3 spawnPos;
    private Vector3 spawnDir;
    private int allowedHits = 3;

    // Start is called before the first frame update
    void Start()
    {
        AddTail();
        AddTail();
        AddTail();

        Invoke("StartMove", 0.5f);
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
        Invoke("StartMove", 0.2f);
        if(!CheckCollisions())
        {
            Move(transform.position + direction);
        }
    }

    void AddTail()
    {
        var part = Instantiate(tailPrefab, transform.parent, true);
        Attach(part);
    }

    bool CheckCollisions()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 0.25f, collisionMask);
        //print(string.Join(",", hits.Select(h => h.name)));

        foreach (var h in hits)
        {
            if (h.tag == "Wall")
            {
                if(allowedHits > 0)
                {
                    allowedHits--;
                    return false;
                }

                Respawn();
                return true;
            }

            if (h.tag == "Pickup")
            {
                var x = Random.Range(-5, 6);
                var y = Random.Range(-4, 5);
                h.transform.position = new Vector3(x, y, 0);

                AddTail();
            }

            if(h.tag == "Room" && (!currentRoom || h.transform != currentRoom.transform))
			{
                spawnPos = transform.position;
                spawnDir = direction;
                currentRoom = h.GetComponent<Room>();
                Tweener.Instance.MoveTo(camRig, h.transform.position, 0.3f, 0, TweenEasings.BounceEaseOut);
			}
        }


        return false;
    }

    void Respawn()
    {
        CancelInvoke("StartMove");
        Invoke("StartMove", 0.7f);
        direction = spawnDir;
        Reset(spawnPos);
    }
}
