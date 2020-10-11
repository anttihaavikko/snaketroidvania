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
    public EffectCamera cam;
    public List<Appearer> messages;
    public SpeechBubble bubble;
    public Camera mapCam;
    public LayerMask enhancedMapMask;
    public List<Appearer> menuStuff;

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
    private bool canReverse;

    private bool hasTeleport;
    private bool hasReverse;
    private bool hasStop;
    private bool hasMap;
    private bool hasEnhancedMap;
    private bool hasFullMap;

    private bool hasRevealed;
    private bool hasEnhanced;
    private bool startedMusic;
    private bool yeahed;
    private bool hasDied;

    private bool ended;

    private bool aiControl = true;
    private int aiSteps = 3;

    private bool canStart;
    private bool justReversed;

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

        map.gameObject.SetActive(true);

        if (Application.isEditor)
        {
            hasMap = true;
            hasFullMap = true;
            hasTeleport = false;
            hasReverse = true;
            hasStop = true;
        }

        Invoke("EnableStart", 2f);
    }

    void EnableStart()
    {
        canStart = true;
    }

    void AiPickDirection()
    {
        if(!AiSafeMove(direction) || aiSteps <= 0)
        {
            changedDirection = true;
            var left = Quaternion.Euler(0, 0, 90f) * direction;
            var right = Quaternion.Euler(0, 0, -90f) * direction;
            var roll = Random.value < 0.5f;
            var first = roll ? left : right;
            var second = roll ? right : left;
            var best = AiSafeMove(first) ? first : second;
            direction = best;
            aiSteps = Random.Range(2, 10);
        }

        aiSteps--;
    }

    bool AiSafeMove(Vector3 dir)
    {
        var hits = Physics2D.OverlapCircleAll(transform.position + dir, 0.25f, collisionMask);
        return !hits.Any(h => h.gameObject.tag == "Wall" || h.gameObject.tag == "Pickup");
    }

    // Update is called once per frame
    void Update()
    {
        if(aiControl)
        {
            if(canStart && Input.anyKeyDown)
            {
                var idx = 0;
                menuStuff.ForEach(ms => {
                    this.StartCoroutine(ms.Hide, idx * 0.15f);
                    idx++;
                });
                aiControl = false;
            }

            return;
        }

        if(bubble.IsShown() && bubble.done && Input.anyKeyDown)
        {
            HideMessage();
            return;
        }

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
        else if (hasReverse && dir != Vector3.zero)
        {
            willReverse = true;
        }

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
        justReversed = true;
        canReverse = false;
        direction = GetReverseDirection();
        transform.position = GetReverseSpot();
        willReverse = false;
        var oldTail = tail;
        tail = GetEnd();
        oldTail.ReverseOrder(null);
        hasReleasedSinceReverse = false;
        RepositionMids();
        Reindex(0);

        CancelInvoke("StartMove");
        Invoke("StartMove", 0.4f);

        AudioManager.Instance.PlayEffectAt(9, transform.position, 1f);
        AudioManager.Instance.PlayEffectAt(1, transform.position, 0.571f);
        AudioManager.Instance.PlayEffectAt(14, transform.position, 0.816f);

        EffectManager.Instance.AddEffect(1, transform.position);
    }

    void StartMove()
    {
        if(aiControl)
        {
            AiPickDirection();
        }

        if (willReverse && canReverse)
        {
            Reverse();
        }

        var wasSaveUsed = saveUsed;
        var willCollide = WillHit(direction);

        if (changedDirection)
        {
            var vol = 0.4f;
            AudioManager.Instance.PlayEffectAt(17, transform.position, 2f * vol);
            AudioManager.Instance.PlayEffectAt(16, transform.position, 0.62f * vol);
            AudioManager.Instance.PlayEffectAt(10, transform.position, 0.751f * vol);
        }
        else
        {
            var vol = 0.3f;
            AudioManager.Instance.PlayEffectAt(8, transform.position, 2f * vol);
            AudioManager.Instance.PlayEffectAt(4, transform.position, 0.171f * vol);
        }

        saveUsed |= !changedDirection || willCollide;

        changedDirection = false;

        var reverseMod = willReverse ? 0.2f : 0f;
        Invoke("StartMove", 0.2f + reverseMod);

        frozen |= willCollide && immortal;

        if ((!willCollide || wasSaveUsed) && !CheckCollisions())
        {
            if(willCollide && !immortal && !hasTeleport)
            {
                //Move(transform.position + direction * 0.25f);
                Invoke("Respawn", 0.2f);
                return;
            }

            Move(transform.position + direction);
            moveDirection = direction;
            saveUsed = false;
            canReverse = true;
        }
    }

    void AddTail()
    {
        length++;
        var part = partPool.Get();
        part.transform.position = transform.position;
        part.partPool = partPool;
        Attach(part);

        immortal |= length == 8;
    }

    bool WillHit(Vector3 dir)
    {
        var hits = Physics2D.OverlapCircleAll(transform.position + dir, 0.25f, collisionMask);
        return hits.Any(h => IsDeadly(h.gameObject));
    }

    bool IsDeadly(GameObject go)
    {
        if (go.tag == "Wall")
            return true;

        if(go.tag == "Tail" && !justReversed)
        {
            var tail = go.GetComponent<SnakePart>();
            return tail.HasMoved();
        }

        return false;
    }

    void ApplySkill(Power skill)
    {
        switch(skill)
        {
            case Power.Stop:
                hasStop = true;
                ShowMessage("Aquired (patience)!\n\n(Hold forward) to get some thinking time...");
                break;
            case Power.Reverse:
                hasReverse = true;
                ShowMessage("Aquired (reverse)!\n\nPress (back) to turn around...");
                break;
            case Power.Map:
                if (!hasMap)
                {
                    hasMap = true;
                    ShowMessage("Aquired (map)!\n\nPress (tab) or (m)\n to view...");
                }
                else if (!hasEnhancedMap)
                {
                    hasEnhancedMap = true;
                    ShowMessage("Aquired\n(enhanced map)!\n\nGet out of the room to update...");
                }
                else
                {
                    hasFullMap = true;
                    ShowMessage("Aquired (full map)!\n\nGet out of the room to update...");
                }
                break;
            case Power.Teleport:
                ShowMessage("Aquired special forbidden skill\n(wormhole)!");
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
                else if (hasEnhancedMap)
                    hasEnhancedMap = false;
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
        if(currentRoom)
        {
            var diff = currentRoom.transform.position - transform.position;

            if(diff.magnitude > 20)
            {
                Respawn();
                return true;
            }
        }

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
                if(part.HasMoved() && !justReversed)
                {
                    if(hasTeleport)
                    {
                        EffectManager.Instance.AddEffect(1, transform.position);

                        transform.position += direction * 1;
                        AudioManager.Instance.PlayEffectAt(9, transform.position, 1f);
                        AudioManager.Instance.PlayEffectAt(1, transform.position, 0.571f);
                        AudioManager.Instance.PlayEffectAt(11, transform.position, 0.8f);

                        EffectManager.Instance.AddEffect(1, transform.position);
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
                    Burrow();
                    frozen = true;
                }
            }

            if (h.tag == "Pickup")
            {
                var pickup = h.GetComponent<Pickup>();
                AddTail();
                ApplySkill(pickup.power);

                EffectManager.Instance.AddEffect(1, transform.position);
                EffectManager.Instance.AddEffect(3, transform.position);

                cam.BaseEffect(0.1f);

                var vol = 0.8f;
                AudioManager.Instance.PlayEffectAt(8, transform.position, 2f * vol);
                AudioManager.Instance.PlayEffectAt(17, transform.position, 2f * vol);
                AudioManager.Instance.PlayEffectAt(21, transform.position, 0.743f * vol);
                AudioManager.Instance.PlayEffectAt(23, transform.position, 0.506f * vol);
                AudioManager.Instance.PlayEffectAt(31, transform.position, 1f * vol);
                AudioManager.Instance.PlayEffectAt(29, transform.position, 1.273f * vol);
                AudioManager.Instance.PlayEffectAt(26, transform.position, 1f * vol);
                AudioManager.Instance.PlayEffectAt(22, transform.position, 1f * vol);

                if(pickup.help)
                {
                    pickup.help.Show();
                }

                if (length == 5)
                    messages[0].Show(true);

                if (length == 7)
                    messages[1].Show(true);

                if (length == 8)
                    messages[2].Show(true);

                if (length < 8)
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
                var vol = 0.7f;
                AudioManager.Instance.PlayEffectAt(7, transform.position, 1f * vol);
                AudioManager.Instance.PlayEffectAt(5, transform.position, 1f * vol);
                AudioManager.Instance.PlayEffectAt(9, transform.position, 1f * vol);

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
                        hasRevealed = true;
                        currentRoom.RevealAll();
                    }

                    if(!hasEnhanced && hasEnhancedMap)
                    {
                        hasEnhanced = true;
                        mapCam.cullingMask = enhancedMapMask;
                    }

                    if(!yeahed)
                    {
                        yeahed = true;
                        messages[3].Show(true);
                    }
                }

                spawnPos = RoundVector(transform.position);

                spawnDir = direction;
                spawnLength = length;
                currentRoom = h.GetComponent<Room>();
                currentRoom.Reveal();
                Tweener.Instance.MoveTo(camRig, h.transform.position, 0.3f, 0, TweenEasings.BounceEaseOut);

                if (currentRoom.last)
                {
                    messages[5].Show(false);
                    frozen = true;
                    ended = true;
                }
            }
        }

        justReversed = false;

        return returnValue;
    }

    void Respawn()
    {
        CancelInvoke("Respawn");

        Explode();
        cam.BaseEffect(0.5f);

        AudioManager.Instance.PlayEffectAt(2, transform.position, 1.216f);
        AudioManager.Instance.PlayEffectAt(6, transform.position, 0.825f);
        AudioManager.Instance.PlayEffectAt(15, transform.position, 0.694f);
        AudioManager.Instance.PlayEffectAt(21, transform.position, 1.494f);
        AudioManager.Instance.PlayEffectAt(18, transform.position, 1f);
        AudioManager.Instance.PlayEffectAt(13, transform.position, 1.249f);

        if(ended)
        {
            CancelInvoke("StartMove");
            currentRoom.ShowEnd();
            gameObject.SetActive(false);
            return;
        }

        currentRoom.GetGrabbed().ForEach(p => {
            CancelSkill(p.power);
            if (p.help)
            {
                p.help.Hide();
            }
        });
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

        EffectManager.Instance.AddEffect(0, transform.position);

        if (!hasDied)
        {
            hasDied = true;
            messages[4].Show(true);
        }
    }

    void ShowMessage(string message)
    {
        CancelInvoke("StartMove");
        stopped = true;
        frozen = true;
        bubble.ShowMessage(message);
    }

    void HideMessage()
    {
        Invoke("StartMove", 1f);
        stopped = false;
        frozen = false;
        bubble.Hide();
    }

    void Burrow()
    {
        cam.BaseEffect(0.5f);
        EffectManager.Instance.AddEffect(0, transform.position);
        EffectManager.Instance.AddEffect(2, transform.position);
        EffectManager.Instance.AddEffect(3, transform.position);

        AudioManager.Instance.PlayEffectAt(2, transform.position, 1.216f);
        AudioManager.Instance.PlayEffectAt(6, transform.position, 0.825f);
        AudioManager.Instance.PlayEffectAt(21, transform.position, 1.494f);
        AudioManager.Instance.PlayEffectAt(18, transform.position, 1f);

        if (!startedMusic)
        {
            startedMusic = true;
            AudioManager.Instance.curMusic.Play();
        }
    }
}
