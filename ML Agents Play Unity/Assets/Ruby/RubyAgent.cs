using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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

        [Tooltip("Maximum collision Velocity allowed.")]
        public float _maxCollisionVelocity = 2f;
        
        [Header("Model Stuff")] 
        [Tooltip("Animation and effect of thruster being on.")] [SerializeField]
        private GameObject _thruster; 
        [Tooltip("Animation and effect of reverse thruster being on.")] [SerializeField]
        private GameObject _reverseThruster;

        [Range(0,1)]
        public float ThrusterFuel = 1;
        [Range(0,1)]
        public float ThrusterFuelReverse = 1;
        
        #endregion
        // The rigidbody of the agent
         private Rigidbody _rigidbody;
        private Camera _agentCamera;
    
        private Sattelite _sattelite;

        private Station _station;



        #region Setup

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _agentCamera = GetComponentInChildren<Camera>();
            _sattelite = FindObjectOfType<Sattelite>();
            _station = FindObjectOfType<Station>();
        }
    
        // Start is called before the first frame update
        void Start()
        {
            
        }

        public override void OnEpisodeBegin()
        {
            Reset();
            
            base.OnEpisodeBegin();
        }

        private void Reset()
        {
            transform.position = _station.transform.position + new Vector3(
                Random.Range(-2, -6),
                Random.Range(-2, -6),
                Random.Range(-2, -6)
            );
            transform.rotation = Random.rotation;
            
            ThrusterFuel = 1;
            ThrusterFuelReverse = 1;
            
            _sattelite.Reset();
        }


        // Update is called once per frame
        void FixedUpdate()
        {
            CheckConditionals();
        }
        

        #endregion


        #region Observations

        public override void CollectObservations(VectorSensor sensor)
        {
            Transform tform = transform;
            Transform stationTform = _station.transform;
            Transform satteliteTform = _sattelite.transform;
            
            Vector3 position = tform.position;
            Vector3 stationPosition = stationTform.position;
            Vector3 sattelitePosition = satteliteTform.position;
            
            // Distance from station - 1 observation
            sensor.AddObservation(Vector3.Distance(position, stationPosition));
            
            // Distance from sattelite - 1 observation
            sensor.AddObservation(Vector3.Distance(position, sattelitePosition));
            
            // Agent velocity - 3 observations
            sensor.AddObservation(tform.InverseTransformDirection(_rigidbody.velocity));
            
            // Agent angular velocity - 3 observations
            sensor.AddObservation(tform.InverseTransformDirection(_rigidbody.angularVelocity));
            
            // Direction to station - 3 observations
            sensor.AddObservation(tform.InverseTransformDirection(stationPosition - position).normalized);
            
            // Direction to sattelite - 3 observations
            sensor.AddObservation(tform.InverseTransformDirection(sattelitePosition - position).normalized);
            
            // Relative Station Rotation - 3 observations
            sensor.AddObservation(tform.InverseTransformDirection(stationTform.eulerAngles - tform.eulerAngles).normalized);
            
            // Relative Sattelite Rotation - 3 observations
            sensor.AddObservation(tform.InverseTransformDirection(satteliteTform.eulerAngles - tform.eulerAngles).normalized);
            
            // Thruster Fuel - 1 observation
            sensor.AddObservation(ThrusterFuel);
            
            // Thruster Fuel Reverse - 1 observation
            sensor.AddObservation(ThrusterFuelReverse);
            
            // Four forward Raycasts - 4 observations
            sensor.AddObservation(RaycastOffset(tform.forward, new Vector3(.5f, .5f, .52f), tform));
            sensor.AddObservation(RaycastOffset(tform.forward, new Vector3(-.5f, .5f, .52f), tform));
            sensor.AddObservation(RaycastOffset(tform.forward, new Vector3(.5f, -.5f, .52f), tform));
            sensor.AddObservation(RaycastOffset(tform.forward, new Vector3(-.5f, -.5f, .52f), tform));
            
            // Raycasts in each direction - 6 observations
            sensor.AddObservation(RayInDirection(tform.forward));
            sensor.AddObservation(RayInDirection(-tform.forward));
            sensor.AddObservation(RayInDirection(tform.right));
            sensor.AddObservation(RayInDirection(-tform.right));
            sensor.AddObservation(RayInDirection(tform.up));
            sensor.AddObservation(RayInDirection(-tform.up));
            
            
            //total observations: 32
        }
        
        private float RayInDirection(Vector3 direction, Vector3 offset = default)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, 10f))
            {
                Debug.DrawRay(transform.position, direction * hit.distance, Color.yellow);
                return hit.distance;
            }
            else
            {
                Debug.DrawRay(transform.position, direction * 10f, Color.white);
                return 10f;
            }
        }

        private float RaycastOffset(Vector3 direction, Vector3 offset, Transform tform)
        {
            RaycastHit hit;
            Vector3 startPosition = transform.TransformPoint(offset);
            if(Physics.Raycast(startPosition, direction, out hit, 10f))
            {
                Debug.DrawRay(transform.position + offset, direction * hit.distance, Color.yellow);
                return hit.distance;
            }
            else
            {
                Debug.DrawRay(transform.position + offset, direction * 10f, Color.white);
                return 10f;
            }
        }

        #endregion
        

        #region Scoring

        private void OnCollisionEnter(Collision col)
        {
            if(col.relativeVelocity.magnitude > _maxCollisionVelocity)
            {
                Debug.Log("Big Crash!");
                Reward(RewardLevel.BigBad);
            }
            else
            {
                Debug.Log("Small Crash!");
                Reward(RewardLevel.SmallBad);
            }
        }

        public void OnPadTouch(Collision col)
        {
            if (col.relativeVelocity.magnitude < _maxCollisionVelocity)
            {
                Debug.Log("Small Touch!");
                Reward(RewardLevel.BigGood);
            }
            else
            {
                Debug.Log("Big Touch!");
                Reward(RewardLevel.SmallGood);
            }
        }


        private void CheckConditionals()
        {
            if (Vector3.Distance(transform.position, _station.transform.position) > _maxDistance)
                KillRound();
            if(Vector3.Distance(transform.position, _sattelite.transform.position) > _maxDistance)
                KillRound();
        }
        

        private void Reward(RewardLevel level)
        {
            switch (level)
            {
                case RewardLevel.BigGood:
                    AddReward(1f);
                    break;
                case RewardLevel.BigBad:
                    AddReward(-1f);
                    break;
                case RewardLevel.SmallGood:
                    AddReward(.1f);
                    break;
                case RewardLevel.SmallBad:
                    AddReward(-.1f);
                    break;
            }
        }

        public void KillRound()
        {
            Reward(RewardLevel.BigBad);
            EndEpisode();
            Reset();
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
        /// None currently
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
            SetThrust(actions[0] - 1);
            
            // rotate x
            _rigidbody.AddTorque(transform.right * moveForce * (actions[1] - 1));
            // rotate y
            _rigidbody.AddTorque(transform.up * moveForce * (actions[2] - 1));
            // rotate z
            _rigidbody.AddTorque(transform.forward * moveForce * (actions[3] - 1));
            
        }

        private void SetThrust(int thrust)
        {
            switch (thrust)
            {
                case 1:
                    _rigidbody.AddForce(transform.forward * moveForce);
                    _thruster.SetActive(true);
                    ThrusterFuel -= .001f;
                    break;
                case 0:
                    _thruster.SetActive(false);
                    _reverseThruster.SetActive(false);
                    break;
                case -1:
                    _rigidbody.AddForce(-transform.forward * moveForce * .2f);
                    _reverseThruster.SetActive(true);
                    ThrusterFuelReverse -= .01f;
                    break;
            }
        }

        #endregion
        
        
    }

    public enum RewardLevel
    {
        BigGood, BigBad, SmallGood, SmallBad
    }



    
}