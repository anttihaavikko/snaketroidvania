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
    private int allowedHits;
    private int length = 1;

    // Start is called before the first frame update
    void Start()
    {
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

        if(Application.isEditor && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync("Main");
        }
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
        length++;
        var part = Instantiate(tailPrefab, transform.parent, true);
        Attach(part);

        if(length == 8)
        {
            allowedHits = 4;
        }
    }

    bool CheckCollisions()
    {
        var returnValue = false;
        var hits = Physics2D.OverlapCircleAll(transform.position, 0.25f, collisionMask);
        //print(string.Join(",", hits.Select(h => h.name)));

        var insideWall = hits.Any(h => h.gameObject.tag == "Wall");

        foreach (var h in hits)
        {
            if (h.tag == "Wall")
            {
                if(allowedHits > 0)
                {
                    //print("Inside wall, allowed " + allowedHits);
                    allowedHits--;
                }
                else
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

            if(!insideWall && h.tag == "Room" && (!currentRoom || h.transform != currentRoom.transform))
			{
                //print("Activate room");
                spawnPos = transform.position;
                spawnDir = direction;
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
        Reset(spawnPos);
        currentRoom.Reset();
    }
}
