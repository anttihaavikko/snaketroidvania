using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomScale : MonoBehaviour
{
    public float min = 0.9f;
    public float max = 1.1f;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale *= Random.Range(min, max);
    }
}
