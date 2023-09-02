using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ruby
{
    public class Sattelite : MonoBehaviour
    {
        Rigidbody _rb;

        public Vector3 _areaMin;
        public Vector3 _areaMax;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if(_rb == null)
                _rb = gameObject.AddComponent<Rigidbody>();
        }

    
        public void Reset()
        {
            //set randome postion 2-6 units away from station
            transform.position = FindObjectOfType<Station>().transform.position + new Vector3(
                Random.Range(_areaMin.x, _areaMax.x),
                Random.Range(_areaMin.y, _areaMax.y),
                Random.Range(_areaMin.z, _areaMax.z)
            );
        
            //set random rotation
            transform.rotation = Random.rotation;
            
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        
        }

        private void OnCollisionEnter(Collision col)
        {
            float velocity = col.relativeVelocity.magnitude;
            Pad pad = col.collider.GetComponent<Pad>();
            bool isPad = pad != null;

            pad._agent.Goal(velocity, isPad);
        }
    }
}
