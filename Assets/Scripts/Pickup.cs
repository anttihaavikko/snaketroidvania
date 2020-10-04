using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Power
{
    Grow,
    Map,
    Teleport,
    Reverse,
    None,
    Stop
};

public class Pickup : MonoBehaviour
{
    public Power power = Power.Grow;
}
