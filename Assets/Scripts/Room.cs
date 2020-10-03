using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    private List<GameObject> grabbed;

    // Start is called before the first frame update
    void Start()
    {
        grabbed = new List<GameObject>();
    }

    public void Grab(GameObject go)
    {
        go.SetActive(false);
        grabbed.Add(go);
    }

    public void RoomDone()
    {
        grabbed.Clear();
    }

    public void Reset()
    {
        grabbed.ForEach(g => g.SetActive(true));
        grabbed.Clear();
    }
}
