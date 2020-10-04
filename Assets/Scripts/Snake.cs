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
    public MapDisplay map;

    private Vector3 direction = Vector3.right;

    private Vector3 spawnPos;
    private Vector3 spawnDir;
    private int spawnLength;
    private bool immortal;
    private int length = 1;
    private List<DoorSwitch> switches;
    private bool willReverse;
    private bool hasReleasedSinceReverse = true;
    private bool changedDirection;
    private bool saveUsed;
    private Vector3 bufferDirection;
    private bool frozen;
    private bool stopped;
    private bool canStop;
    private bool showingMap;
    private bool paused;

    private bool hasTeleport;
    private bool hasReverse;
    private bool hasStop;
    private bool hasMap = true;
    private bool hasFullMap = true;

    private bool hasRevealed;

    // Start is called before the first frame update
    void Start()
    {
        index = 1;
        partPool = gameObject.AddComponent<PartPool>();
        partPool.SetPrefab(tailPrefab);
        switches = new List<DoorSwitch>();

        AddTail();
        AddTail();

        Invoke("StartMove", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (frozen)
            return;

        if((Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.M)) && hasMap)
        {
            ToggleMap();
        }

        var treshold = 0.25f;
        var ix = Input.GetAxisRaw("Horizontal");
        var iy = Input.GetAxisRaw("Vertical");

        var dir = direction;
        if(ix > treshold) dir = Vector3.right;
        else if (ix < -treshold) dir = Vector3.left;
        else if (iy > treshold) dir = Vector3.up;
        else if (iy < -treshold) dir = Vector3.down;

        var inp = new Vector3(ix, iy, 0);
        var released = inp.magnitude < treshold;

        if (released)
        {
            hasReleasedSinceReverse = true;
            canStop = true;
        }

        if(!released && dir == direction && canStop && hasStop)
        {
            CancelInvoke("StartMove");
            stopped = true;
            canStop = false;
            return;
        }

        if(released && stopped)
        {
            stopped = false;
            StartMove();
        }

        if (!hasReleasedSinceReverse)
            return;

        var useBuffer = true;

        if (dir != direction)
        {
            if(changedDirection && bufferDirection == Vector3.zero)
            {
                bufferDirection = dir;
                dir = direction;
                useBuffer = false;
            }

            changedDirection = true;

            var willNow = WillHit(dir);
            var willAfter = WillHit(direction + dir);

            if(willNow && !willAfter)
            {
                bufferDirection = dir;
                dir = direction;
                useBuffer = false;
            }
        }

        if(useBuffer && bufferDirection != Vector3.zero)
        {
            dir = bufferDirection;
            bufferDirection = Vector3.zero;
        }

        if (dir != -direction)
            direction = dir;
        else if (hasReverse)
            willReverse = true;

        if (Application.isEditor && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync("Main");
        }
    }

    void TogglePause(bool state)
    {
        paused = state;

        if(paused)
        {
            CancelInvoke("StartMove");
        }
        else
        {
            Invoke("StartMove", 0.2f);
        }
    }

    void ToggleMap()
    {
        showingMap = !showingMap;
        map.Toggle(showingMap);
        TogglePause(showingMap);
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
        if (willReverse)
        {
            Reverse();
        }

        var wasSaveUsed = saveUsed;
        var willCollide = WillHit(direction);

        if (!changedDirection || willCollide)
        {
            saveUsed = true;
        }

        changedDirection = false;

        var reverseMod = willReverse ? 0.2f : 0f;
        Invoke("StartMove", 0.2f + reverseMod);

        if (willCollide && immortal)
            frozen = true;

        if ((!willCollide || wasSaveUsed) && !CheckCollisions())
        {
            if(willCollide && !immortal && !hasTeleport)
            {
                Move(transform.position + direction * 0.25f);
                Invoke("Respawn", 0.2f);
                return;
            }

            Move(transform.position + direction);
            moveDirection = direction;
            saveUsed = false;
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

    bool WillHit(Vector3 dir)
    {
        var hits = Physics2D.OverlapCircleAll(transform.position + dir, 0.25f, collisionMask);
        return hits.Any(h => h.gameObject.tag == "Wall" || h.gameObject.tag == "Tail");
    }

    void ApplySkill(Power skill)
    {
        switch(skill)
        {
            case Power.Stop:
                hasStop = true;
                break;
            case Power.Reverse:
                hasReverse = true;
                break;
            case Power.Map:
                if (!hasMap)
                    hasMap = true;
                else
                    hasFullMap = true;
                break;
            case Power.Teleport:
                hasTeleport = true;
                break;
        }
    }

    void CancelSkill(Power skill)
    {
        switch (skill)
        {
            case Power.Stop:
                hasStop = false;
                break;
            case Power.Reverse:
                hasReverse = false;
                break;
            case Power.Map:
                if (hasFullMap)
                    hasFullMap = false;
                else
                    hasMap = false;
                break;
            case Power.Teleport:
                hasTeleport = false;
                break;
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
            if (h.tag == "Tail")
            {
                var part = h.GetComponent<SnakePart>();
                if(part.HasMoved())
                {
                    if(hasTeleport)
                    {
                        transform.position += direction * 1;
                    }
                    else
                    {
                        Respawn();
                        returnValue = true;
                    }
                }
            }

            if (h.tag == "Wall")
            {
                if(!immortal)
                {
                    Respawn();
                    returnValue = true;
                }
                else
                {
                    frozen = true;
                }
            }

            if (h.tag == "Pickup")
            {
                var pickup = h.GetComponent<Pickup>();
                AddTail();
                ApplySkill(pickup.power);

                if(length < 8)
                {
                    var x = Random.Range(-5, 6);
                    var y = Random.Range(-4, 5);
                    h.transform.position = new Vector3(x, y, 0);
                }
                else
                {
                    currentRoom.Grab(pickup);
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
                    frozen = false;
                    immortal = false;
                    currentRoom.MarkDone();

                    CancelInvoke("StartMove");
                    Invoke("StartMove", 0.7f);

                    if(!hasRevealed && hasFullMap)
                    {
                        print("Reveal all!");
                        hasRevealed = true;
                        currentRoom.RevealAll();
                    }
                }

                spawnPos = RoundVector(transform.position);

                spawnDir = direction;
                spawnLength = length;
                currentRoom = h.GetComponent<Room>();
                currentRoom.Reveal();
                Tweener.Instance.MoveTo(camRig, h.transform.position, 0.3f, 0, TweenEasings.BounceEaseOut);
			}
        }

        return returnValue;
    }

    void Respawn()
    {
        currentRoom.GetGrabbed().ForEach(p => CancelSkill(p.power));
        frozen = false;
        immortal = false;
        CancelInvoke("StartMove");
        Invoke("StartMove", 0.7f);
        direction = spawnDir;
        length = spawnLength;
        Chop(length);
        Reset(spawnPos);
        currentRoom.Reset();
        RepositionMids();
    }
}
