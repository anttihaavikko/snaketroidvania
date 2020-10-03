using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class Snake : SnakePart
{
    public SnakePart tailPrefab;
    public LayerMask collisionMask;
	public Transform camRig;
    public Room currentRoom;

    private Vector3 direction = Vector3.right;

    private Vector3 spawnPos;
    private Vector3 spawnDir;
    private int spawnLength;
    private bool immortal;
    private int length = 1;
    private List<DoorSwitch> switches;
    private bool willReverse;
    private bool hasReleasedSinceReverse = true;

    // Start is called before the first frame update
    void Start()
    {
        index = 1;
        partPool = new PartPool();
        partPool.SetPrefab(tailPrefab);
        switches = new List<DoorSwitch>();

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

        var inp = new Vector3(ix, iy, 0);
        if (inp.magnitude < treshold)
        {
            hasReleasedSinceReverse = true;
        }

        if (!hasReleasedSinceReverse)
            return;

        if (dir != -direction)
            direction = dir;
        else
            willReverse = true;

        if (Application.isEditor && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync("Main");
        }
    }

    void Reverse()
    {
        direction = GetReverseDirection();
        transform.position = GetReverseSpot();
        willReverse = false;
        var oldTail = tail;
        tail = GetEnd();
        oldTail.ReverseOrder(null);
        hasReleasedSinceReverse = false;
        RepositionMids();
        Reindex(0);
    }

    void StartMove()
    {
        Invoke("StartMove", willReverse ? 0.4f : 0.2f);

        if (willReverse)
        {
            Reverse();
        }

        if(!CheckCollisions())
        {
            Move(transform.position + direction);
            moveDirection = direction;
        }
    }

    void AddTail()
    {
        length++;
        var part = partPool.Get();
        part.transform.position = transform.position;
        part.partPool = partPool;
        Attach(part);

        if(length == 8)
        {
            immortal = true;
        }
    }

    bool CheckCollisions()
    {
        var returnValue = false;
        var hits = Physics2D.OverlapCircleAll(transform.position, 0.25f, collisionMask);
        //print(string.Join(",", hits.Select(h => h.name)));

        switches.RemoveAll(ds => !ds.IsStillOn());

        var insideWall = hits.Any(h => h.gameObject.tag == "Wall");

        foreach (var h in hits)
        {
            if (h.tag == "Wall")
            {
                if(!immortal)
                {
                    Respawn();
                    returnValue = true;
                }
            }

            if (h.tag == "Pickup")
            {
                AddTail();

                if(length < 8)
                {
                    var x = Random.Range(-5, 6);
                    var y = Random.Range(-4, 5);
                    h.transform.position = new Vector3(x, y, 0);
                }
                else
                {
                    currentRoom.Grab(h.gameObject);
                }
            }

            if (h.tag == "Switch")
            {
                var ds = h.GetComponent<DoorSwitch>();
                switches.Add(ds);
                ds.Toggle(true);
            }

            if (!insideWall && h.tag == "Room" && (!currentRoom || h.transform != currentRoom.transform))
			{
                if(currentRoom)
                {
                    immortal = false;
                    currentRoom.MarkDone();

                    CancelInvoke("StartMove");
                    Invoke("StartMove", 0.7f);
                }

                spawnPos = transform.position;
                spawnDir = direction;
                spawnLength = length;
                currentRoom = h.GetComponent<Room>();
                Tweener.Instance.MoveTo(camRig, h.transform.position, 0.3f, 0, TweenEasings.BounceEaseOut);
			}
        }


        return returnValue;
    }

    void Respawn()
    {
        CancelInvoke("StartMove");
        Invoke("StartMove", 0.7f);
        direction = spawnDir;
        length = spawnLength;
        Chop(length);
        Reset(spawnPos);
        currentRoom.Reset();
    }
}
