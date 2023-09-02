using UnityEngine;

namespace Ruby
{
    /// <summary>
    /// Should be placed along with a collider on the an object to detect collisions and pass relevant data.
    /// </summary>
    public class TouchSensor : MonoBehaviour
    {
        public RubyAgent _agent;

        public bool okToTouch = true;
        

        private void OnCollisionEnter(Collision col)
        {
            Debug.Log($"Collision detected between {gameObject.name} and {col.gameObject.name}");
        }
    }
}
