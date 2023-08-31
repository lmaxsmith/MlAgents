using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Ruby
{
    public class RubyAgent : Agent
    {

        #region Parameters

        [Tooltip("Force to apply when moving")]
        public float moveForce = 2f;

        [Tooltip("Speed to pitch up or down")]
        public float pitchSpeed = 100f;

        [Tooltip("Speed to rotate around the up axis")]
        public float yawSpeed = 100f;
    
        [Tooltip("Speed to rotate around the forward axis")]
        public float rollSpeed = 100f;

        [Tooltip("Whether this is training mode or gameplay mode")]
        public bool trainingMode;

        [Tooltip("Maximum distance from station before episode ends")]
        public float _maxDistance = 10f;

        [Header("Model Stuff")] [SerializeField]
        private GameObject _engineOn;
        
        #endregion
        // The rigidbody of the agent
        new private Rigidbody _rigidbody;
        private Camera _agentCamera;
    
        private Sattelite _sattelite;

        private Station _station;
        
        

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _agentCamera = GetComponentInChildren<Camera>();
            _sattelite = GetComponent<Sattelite>();
            _station = GetComponent<Station>();
        }
    
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void FixedUpdate()
        {
        
        }



        #region Scoring

        private void CheckConditionals()
        {
            if (Vector3.Distance(transform.position, _station.transform.position) < 10f)
                Reward(RewardLevel.BigBad);
        
        
        }

        private void Reward(RewardLevel level)
        {
        
        }
        

        #endregion
        
        
        #region Actions

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            
            HandleActionsContinuous(actions.ContinuousActions);
            HandleActionsDiscrete(actions.DiscreteActions);
        }

        /// <summary>
        /// Action labelling:
        /// 1. Pitch up
        /// 2. Rotate Right
        /// 3. Roll Right
        /// 
        /// </summary>
        /// <param name="actions"></param>
        void HandleActionsContinuous(ActionSegment<float> actions)
        {
            
        }
        
        /// <summary>
        /// Action labelling:
        /// 1. Thrust
        /// 2. Rotate x
        /// 3. Rotate y
        /// 4. Rotate z
        /// </summary>
        /// <param name="actions"></param>
        void HandleActionsDiscrete(ActionSegment<int> actions)
        {
            if(actions[0] == 1) //thrust forward
                _rigidbody.AddForce(transform.forward * moveForce);
            else if(actions[2] == 1)
                _rigidbody.AddTorque(transform.right * moveForce);
            else if(actions[3] == 1)
                _rigidbody.AddTorque(transform.up * moveForce);
            else if(actions[4] == 1)
                _rigidbody.AddTorque(transform.forward * moveForce);
        }

        private void StartThrustForward()
        {
            
        }

        #endregion
        
        
    }

    public enum RewardLevel
    {
        BigGood, BigBad, SmallGood, SmallBad
    }



    
}