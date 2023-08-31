using UnityEngine;
using Random = UnityEngine.Random;

namespace Ruby
{
    public class Sattelite : MonoBehaviour
    {
        Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if(_rb == null)
                _rb = gameObject.AddComponent<Rigidbody>();
        }

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
                Random.Range(4, 6),
                Random.Range(4, 6),
                Random.Range(4, 6)
            );
        
            //set random rotation
            transform.rotation = Random.rotation;
            
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        
        }
    }
}
