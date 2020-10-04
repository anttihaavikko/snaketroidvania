using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public GameObject hider, hinter;
    public List<Room> neighbours;

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

    public void MarkDone()
    {
        grabbed.Clear();
    }

    public void Reset()
    {
        grabbed.ForEach(g => g.SetActive(true));
        grabbed.Clear();
    }

    public void Reveal()
    {
        hider.SetActive(false);
        hinter.SetActive(false);
        neighbours.ForEach(n => n.hider.SetActive(false));
    }
}
