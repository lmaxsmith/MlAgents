using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


        #region ==== Parameters ====------------------


        [Tooltip("Force to apply when moving")] [Range(.1f, 4)]
        public float _moveForceSet = 1f;

        public float MoveForce => _moveForceSet / 100;

        [Tooltip("Speed to pitch up or down")] [Range(.1f, 4)]
        public float _rotationSpeedSet = 1f;
        
        public float RotationSpeed => _rotationSpeedSet / 1000;

        [Tooltip("Maximum distance from station before episode ends")]
        public float _maxDistance = 10f;

        [Tooltip("Maximum collision Velocity allowed.")]
        public float _maxCollisionVelocity = 2f;
        
        [Tooltip("Defauly true. Improves training. Onle select false for debugging.")]
        public bool _randomizeOnReset = true;
        
        [Header("Model Stuff")] 
        [Tooltip("Animation and effect of thruster being on.")] [SerializeField]
        private GameObject _thruster; 
        [Tooltip("Animation and effect of reverse thruster being on.")] [SerializeField]
        private GameObject _reverseThruster;

        [Header("Fuel")]
        public float _thrusterFuelSeconds = 20;
        public float _thrusterFuelSecondsReverse = 3;
        
        [Range(0,1)]
        public float _thrusterFuel = 1;
        [Range(0,1)]
        public float _thrusterFuelReverse = 1;
        
  
        
        #region == Autoreference ==----

        // The rigidbody of the agent
        private Rigidbody _rigidbody;
        private Camera _agentCamera;
    
        private Sattelite _satellite;

        private Station _station;

        #endregion ----/Autoreference ==

        #endregion -----------------/Parameters ====






        #region ==== Monobehavior ====------------------


        
        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _agentCamera = GetComponentInChildren<Camera>();
            _satellite = FindObjectOfType<Sattelite>();
            _station = FindObjectOfType<Station>();
            
            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }
    
        // Start is called before the first frame update
        void Start()
        {
            Reset();
        }




        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private void Reset()
        {

            if (_randomizeOnReset)
            {
                transform.position = _station.transform.position + new Vector3(
                    Random.Range(-2, 4),
                    Random.Range(2, 6),
                    Random.Range(2, 6)
                );
                transform.rotation = Random.rotation;
            }
            else
            {
                transform.position = _startPosition;
                transform.rotation = _startRotation;
            }
            
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            _thruster.SetActive(false);
            _reverseThruster.SetActive(false);
            
            _thrusterFuel = 1;
            _thrusterFuelReverse = 1;
            
            _satellite.Reset();
        }


        private void Update()
        {
            foreach (var ray in _rays)
            {
                bool isHit = ray.Value.collider != null;
                Debug.DrawRay(
                    ray.Key.origin, 
                    ray.Key.direction * (isHit ? ray.Value.distance : 10f), 
                    isHit ? Color.red : Color.green);
            }
        }

        void FixedUpdate()
        {
            CheckConditionals();
            //AutoStabalize();
        }
        
        

        #endregion -----------------/Monobehavior ====

        
        
        
        #region ==== Agent Implementation ====------------------


        public override void OnEpisodeBegin()
        {
            Reset();
            base.OnEpisodeBegin();
        }
        

        #region == Observations ==----
        
        public override void CollectObservations(VectorSensor sensor)
        {
            Transform tform = transform;
            Transform stationTform = _station.transform;
            Transform satteliteTform = _satellite.transform;
            
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
            sensor.AddObservation(_thrusterFuel);
            
            // Thruster Fuel Reverse - 1 observation
            sensor.AddObservation(_thrusterFuelReverse);

            float offsetScale = .1f;
            Vector3 forward = tform.forward;
            Vector3 right = tform.right;
            Vector3 up = tform.up;
            
            _rays = new Dictionary<Ray, RaycastHit>();
            // Four forward Raycasts - 4 observations
            sensor.AddObservation(SensorRayCast(forward, new Vector3(.5f, .5f, .52f) * offsetScale));
            sensor.AddObservation(SensorRayCast(forward, new Vector3(-.5f, .5f, .52f) * offsetScale));
            sensor.AddObservation(SensorRayCast(forward, new Vector3(.5f, -.5f, .52f) * offsetScale));
            sensor.AddObservation(SensorRayCast(forward, new Vector3(-.5f, -.5f, .52f) * offsetScale));
            
            // Raycasts in each direction - 6 observations
            sensor.AddObservation(SensorRayCast(forward));
            sensor.AddObservation(SensorRayCast(-forward));
            sensor.AddObservation(SensorRayCast(right));
            sensor.AddObservation(SensorRayCast(-right));
            sensor.AddObservation(SensorRayCast(up));
            sensor.AddObservation(SensorRayCast(-up));
            
            
            //total observations: 32
        }

        private Dictionary<Ray, RaycastHit> _rays = new Dictionary<Ray, RaycastHit>();
        
        private float RayInDirection(Vector3 direction)
        {
            RaycastHit hit = default;
            Ray ray = new Ray(transform.position, direction);
            
            Physics.Raycast(ray, out hit, 10f);
            _rays.Add(ray, hit);
            return hit.collider ? hit.distance : 10f;
        }

        private float SensorRayCast(Vector3 direction, Vector3 offset = default)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.TransformPoint(offset), direction);

            Physics.Raycast(ray, out hit, 10f);
            _rays.Add(ray, hit);
            return hit.collider ? hit.distance : 10f;
        }

        

        #endregion ----/Observations ==
        


        #region == Scoring and Rewards ==----


        public void Goal(float velocity, bool isPad)
        {
            Debug.Log($"Gooooooal!! {velocity} m/s");
            hitWasGood = true;
            
            if (isPad)
                if( isPad && velocity < _maxCollisionVelocity)
                    Reward(RewardLevel.BigGood);
                else
                    Reward(RewardLevel.SmallGood);
            
            EndEpisode();
        }

        private bool hitWasGood; //this solution sucks

        // private async void OnCollisionEnter(Collision other)
        // {
        //     Debug.Log($"Oh shit did I just crash???");
        //     await Task.Delay(1000);
        //     if(hitWasGood)
        //     {
        //         hitWasGood = false;
        //         Debug.Log($"NVM sall good.");
        //         return;
        //     }
        //     
        //     Debug.Log("Shit yeah I crashed. at  ");
        //     KillRound();
        // }


        public float closestSoFar = float.MaxValue;
        public float closenessIncrement = .1f;

        private void CheckConditionals()
        {
            Vector3 position = transform.position;
            Vector3 satellitePosition = _satellite.transform.position;
            
            // if (Vector3.Distance(position, _station.transform.position) > _maxDistance * 2)
            //     KillRound();
            if(Vector3.Distance(position, satellitePosition) > _maxDistance * 2)
                KillRound();

            //reward for inching closer.
            float currentDistance = Vector3.Distance(position, satellitePosition);
            if (closestSoFar - currentDistance > closenessIncrement)
            {
                Reward(RewardLevel.SmallGood);
                closestSoFar = currentDistance;
            }
            
            if(_thrusterFuel < 0)
                OutOfGas(0);
            if(_thrusterFuelReverse < 0)
                OutOfGas(1);
        }

        //Awkward method for tracking when we first run out of gas. 
        //A more elegant solution for all this would be a gas tank class.
        private void OutOfGas(int gasType)
        {
            if (hasGasList[gasType])
            {
                Reward(RewardLevel.SmallBad);
                hasGasList[gasType] = false;
            }
        }

        private List<bool> hasGasList = new List<bool>(){false, false};

        private void Reward(RewardLevel level, float scalar = 1f)
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
            Reward(RewardLevel.SmallBad);
            EndEpisode();
            Reset();
        }

        public void GiveUp()
        {
            Reward(RewardLevel.SmallBad);
            Reward(RewardLevel.BigGood, Closeness());
        }
        
        /// <summary>
        /// score between 1/(1-4) representing how close the craft finished to the satellite. Helps the incremental search. 
        /// </summary>
        /// <returns></returns>
        float Closeness() => 1 / Mathf.Clamp(Vector3.Distance(transform.position, _satellite.transform.position), 1, 4);

        

        #endregion ----/Scoring and Rewards ==



        #region == Action ==----


        public override void Heuristic(in ActionBuffers actionsOut)
        {
            int thrust = Input.GetKey(KeyCode.UpArrow) ? 1 : Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
            int xRotation = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            int yRotation = Input.GetKey(KeyCode.A) ? 1 : Input.GetKey(KeyCode.D) ? -1 : 0;
            int zRotation = Input.GetKey(KeyCode.Q) ? 1 : Input.GetKey(KeyCode.E) ? -1 : 0;

            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut[0] = thrust + 1;
            discreteActionsOut[1] = xRotation + 1;
            discreteActionsOut[2] = yRotation + 1;
            discreteActionsOut[3] = zRotation + 1;
        }
        
        
        /// <summary>
        /// Universal Action receiver. Receives from model and heuristic.
        /// Sends to specific handlers for continuous and discrete actions.
        /// </summary>
        /// <param name="actions"></param>
        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            
            HandleActionsContinuous(actions.ContinuousActions);
            HandleActionsDiscrete(actions.DiscreteActions.ToArray());
            
            lastControl = DateTime.Now;
        }



        /// <summary>
        /// Continuous actions. That's 
        /// </summary>
        /// <param name="actions"></param>
        void HandleActionsContinuous(ActionSegment<float> actions)
        {
            //do nothing
        }
        
        /// <summary>
        /// Action labelling:
        /// 1. Thrust
        /// 2. Rotate x
        /// 3. Rotate y
        /// 4. Rotate z
        /// </summary>
        /// <param name="actions"></param>
        void HandleActionsDiscrete(int[] actions)
        {
            SetThrust(actions[0] - 1);
            
            // rotate x
            _rigidbody.AddTorque(transform.right * RotationSpeed * (actions[1] - 1), ForceMode.Force);
            // rotate y
            _rigidbody.AddTorque(transform.up * RotationSpeed * (actions[2] - 1), ForceMode.Force);
            // rotate z
            _rigidbody.AddTorque(transform.forward * RotationSpeed * (actions[3] - 1), ForceMode.Force);
            
        }
        
        DateTime lastControl = DateTime.Now;
        private TimeSpan controlDeltaTime => DateTime.Now - lastControl;


        private void SetThrust(int thrust)
        {
            switch (thrust)
            {
                case 1:
                    _rigidbody.AddForce(transform.forward * MoveForce);
                    _thruster.SetActive(true);
                    _thrusterFuel -= controlDeltaTime.Milliseconds / _thrusterFuelSeconds / 1000;
                    break;
                case 0:
                    _thruster.SetActive(false);
                    _reverseThruster.SetActive(false);
                    break;
                case -1:
                    _rigidbody.AddForce(-transform.forward * MoveForce * .2f);
                    _reverseThruster.SetActive(true);
                    _thrusterFuelReverse -= controlDeltaTime.Milliseconds / _thrusterFuelSecondsReverse / 1000;
                    break;
            }
        }

        

        #endregion ----/Action ==
        
        

        #endregion -----------------/Agent Implementation ====
    }

    public enum RewardLevel
    {
        BigGood, BigBad, SmallGood, SmallBad
    }



    
}