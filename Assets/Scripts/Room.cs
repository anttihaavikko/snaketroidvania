using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    public GameObject hider, hinter;
    public List<Room> neighbours;
    public bool revealed;

    private List<Pickup> grabbed;

    // Start is called before the first frame update
    void Start()
    {
        grabbed = new List<Pickup>();
    }

    public void Grab(Pickup go)
    {
        go.gameObject.SetActive(false);
        grabbed.Add(go);
    }

    public void MarkDone()
    {
        grabbed.Clear();
    }

    public void Reset()
    {
        grabbed.ForEach(g => g.gameObject.SetActive(true));
        grabbed.Clear();
    }

    public void Reveal()
    {
        hider.SetActive(false);
        hinter.SetActive(false);
        neighbours.ForEach(n => n.hider.SetActive(false));
    }

    public List<Pickup> GetGrabbed()
    {
        return grabbed;
    }

    public void RevealAll()
    {
        revealed = true;
        Reveal();
        var nonRevealed = neighbours.Where(n => !n.revealed).ToList();
        if(nonRevealed.Any())
        {
            nonRevealed.ForEach(n => n.RevealAll());
        }
    }
}
