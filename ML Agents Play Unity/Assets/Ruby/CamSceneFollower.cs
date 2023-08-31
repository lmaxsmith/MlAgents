using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamSceneFollower : MonoBehaviour
{
    public List<Transform> _targets;
    public Transform BoundsGO;

    public float minDistance = 2f;
    public float maxDistance = 10f;

    public float rotationSpeed = 0;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //calculate bounds
        Bounds b = new Bounds(_targets[0].position, Vector3.one);
        foreach (var target in _targets)
            b.Encapsulate(target.position);
        BoundsGO.position = b.center;
        BoundsGO.localScale = b.size;
        
        //calculate camera position
        transform.RotateAround(transform.position,Vector3.up, rotationSpeed * Time.deltaTime);
        float targetDistance = Mathf.Clamp((b.max - b.min).magnitude, minDistance, maxDistance);
        transform.position = b.center - transform.forward * targetDistance;
        
    }
}
