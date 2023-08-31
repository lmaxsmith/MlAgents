using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sattelite : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Reset()
    {
        //set randome postion 2-6 units away from station
        transform.position = FindObjectOfType<Station>().transform.position + new Vector3(
            Random.Range(2, 6),
            Random.Range(2, 6),
            Random.Range(2, 6)
        );
        
        //set random rotation
        transform.rotation = Random.rotation;
    }
}
