using System.Collections.Generic;
using UnityEngine;

namespace Ruby
{
    public class CamSceneFollower : MonoBehaviour
    {
        public List<Transform> _targets;
        public Transform _boundsGo;

        public float _minDistance = 2f;
        public float _maxDistance = 10f;
        
        [Range(.5f, 2f)]
        public float _distanceScalar = 1f;
    
        public float _rotationSpeed = 0;

        public Transform _backgroundObjectDirectionOverride;
        

        // Update is called once per frame
        void LateUpdate()
        {
            //calculate bounds
            Bounds b = new Bounds(_targets[0].position, Vector3.one / 10);
            foreach (var target in _targets)
                b.Encapsulate(target.position);
            if (_boundsGo)
            {
                _boundsGo.position = b.center;
                _boundsGo.localScale = b.size;
            }
            
        
            //rotation
            if (_backgroundObjectDirectionOverride != null)
                transform.rotation = Quaternion.LookRotation(_backgroundObjectDirectionOverride.position - transform.position);
            else
                transform.RotateAround(transform.position,Vector3.up, _rotationSpeed * Time.deltaTime);
            
            
            //calculate camera position
            
            float targetDistance = _distanceScalar * Mathf.Clamp((b.max - b.min).magnitude, _minDistance, _maxDistance);
            transform.position = b.center - transform.forward * targetDistance;
        
        }
    }
}
