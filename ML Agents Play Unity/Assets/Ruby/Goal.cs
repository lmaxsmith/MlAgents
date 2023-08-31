using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Goal : MonoBehaviour
{
    private Station _station;
    
    // Start is called before the first frame update
    void Awake()
    {
        _station = FindObjectOfType<Station>();
    }

    private void Start()
    {
        
    }

    public void Reset()
    {
        //set randome postion 2-6 units away from station
        transform.position = _station.transform.position + new Vector3(
            Random.Range(2, 6),
            Random.Range(2, 6),
            Random.Range(2, 6)
        );
    }
}
