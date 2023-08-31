using System;
using System.Collections;
using System.Collections.Generic;
using Ruby;
using UnityEngine;

public class Pad : MonoBehaviour
{
    public RubyAgent agent;


    private void OnCollisionEnter(Collision other)
    {
        agent.OnPadTouch(other);
    }
}
