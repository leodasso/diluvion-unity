using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace Diluvion.Ships
{

    public enum Throttle
    {
        backFull,
        backHalf,
        stop,
        slow,
        half,
        full,
        overdrive
    }

    public enum ShipMake
    {
        Icer,
        Dweller,
        Royal,
        HugeRoyal
    }


    public class ShipMover : MonoBehaviour
    {
        public ShipEngine engine;
        public Throttle throttle = Throttle.stop;

        [Range(0, 1)]
        public float clampedEnginePower = 1;

        public float heatTolerance;
        public float heat;
        public bool overheated;

        public ShipAnimator shipAnim;

        public float extraEnginePower;
        public float lerpRotationSpeed = 5;
        public bool testing;
        const bool debug = false;

        public float rotationInt = 0.001f;  //start at 0, then raise until it stops flip-flopping
        public float errorInt = 0.001f;
        public delegate void Docked();
        public Docked docked;

        public ParticleSystem ascendingSpoutParticle;
        public ParticleSystem descendingBubbleParticle;

        List<AscendingParticlePlacer> _ascendingSpoutSpots;
        List<DescendingParticlePlacer> _descendingBubbleSpots;
        
        List<ParticlePlacer> _placerSpots;
        List<ParticleSystem> _ascendingSpoutParticleList = new List<ParticleSystem>();
        List<ParticleSystem> _descendingBubbleParticleList = new List<ParticleSystem>();
        float _ascendingSpoutEmissionMax;
        float _descendingBubbleEmisionMax;

        public bool overdriveInstalled;
        public float overdriveMultiplier = 1;

        Bridge _myBridge;
        float _enginePower;
        float _enginePowerNormalized;
        bool _inControl = true;
        float _ballastPressure;
        float _ballastSfXtime = 0;
        float _maxBallastPressure = 1;
        Vector3 _lerpRelDir;
        Vector3 _localVelocity;
        Vector3 _localXyVelocity;
        Vector3 _relDir;
        VectorPid _errorController;
        VectorPid _headingController;
        float _swooshSoundTime;
        const float SwooshSoundtimer = 3;
        float[] _throttleLevels = { -1f, -.5f, 0, .25f, .5f, 1.0f, 2f };
        int _throttleSteps = System.Enum.GetValues(typeof(Throttle)).Length - 1;//gets the value List from the enum and sets it up as the step clamp
        float _ballastPower = 2f;
        static float overdriveCost = .2f;  // in air tanks / second
      

        public static float OverdriveCost(float multiplier)
        {
            return overdriveCost / (1 + (Mathf.Clamp(multiplier, 0, 999)));
        }

        void Awake ()
        {
            extraEnginePower = 0;
        }

        void Start()
        { 
            GetBridge();
            //Get the width of the ship
            Quaternion oldRot = transform.rotation;
            transform.rotation = Quaternion.identity;
            transform.rotation = oldRot;
            _inControl = true;
            _errorController = new VectorPid(engine.correctionProp, errorInt, engine.correctionDer);
            _headingController = new VectorPid(engine.rotationProp, rotationInt, engine.rotationDer);
          
        }


        public void ChangeClamp(float by)
        {
            clampedEnginePower += by;
        }
        
        
        AngularVelocityDrawer avd;
        AngularVelocityDrawer AngleDrawer()
        {
            if (avd != null) return avd;
            avd = GetComponent<AngularVelocityDrawer>();
            if (avd == null)
                avd = gameObject.AddComponent<AngularVelocityDrawer>();
            return avd;

        }

       
        void OnEnable()
        {
            ShipAnim();
            StartAmbientSounds();
            NullThrottle();
            ShipManeuver(transform.forward);
        }

        public void StartAmbientSounds()
        {
            SpiderSound.MakeSound("Play_Ship_Movement", gameObject);
            SpiderSound.MakeSound("Play_" + engine.engineSound, gameObject);
        }

        #region SafeGetFunctions
        public ShipAnimator ShipAnim()
        {
            if (shipAnim != null) return shipAnim;
            shipAnim = GetComponent<ShipAnimator>();
            return shipAnim;
        }
        public Bridge GetBridge()
        {
            if (_myBridge != null) return _myBridge;
            _myBridge = GetComponent<Bridge>();
            return _myBridge;
        }

        Rigidbody _rigidbody;
        public Rigidbody myRigidbody()
        {
            if (_rigidbody) return _rigidbody;
            _rigidbody = GetComponent<Rigidbody>();
            return _rigidbody;
        }
        #endregion

        bool MoverDebug()
        {
            return debug && Application.isEditor;
        }
        

        public float MaxSpeedPerSecond()
        {
            if (!engine) return 0;
            float topPower = (engine.maxEnginePower * clampedEnginePower);
            return (topPower / myRigidbody().drag - Time.fixedDeltaTime * topPower) /
                   GetComponentInChildren<Rigidbody>(true).mass;
        }

        
        public float ActualSpeed()
        {
            return myRigidbody().velocity.magnitude;
        }

        public bool InControl()
        {
            return _inControl;
        }

        public float NormalizedOverheatAmount()
        {
            return heat / heatTolerance;
        }

        /// <summary>
        /// Applies coolant to the engines, reducing their heat.
        /// </summary>
        /// <returns>Returns the amount of coolant used.</returns>
        public float ApplyCoolant(float coolantAmount)
        {
            if (heat <= 0) return 0;
            float coolantToUse = Mathf.Clamp(coolantAmount, 0, heat);
            heat -= coolantToUse;
            heat = Mathf.Clamp(heat, 0, heatTolerance);
            SpiderSound.MakeSound("Play_Airtanks_Filled", gameObject);
            return coolantToUse;
        }

        /// <summary>
        /// Adds to the engine power. The amount is relative to the default engine power, so if amount = 1, it will add 100%
        /// more power.
        /// </summary>
        public void SetExtraEnginePower(float amount)
        {
            extraEnginePower = amount;
            SetEngineValues();
        }

        public void RecalculateHeatTolerance()
        {
            heatTolerance = 0;
            foreach (var chunk in GetBridge().bonusChunks)
            {
                if (chunk is BonusEngine)
                {
                    BonusEngine bonusEngine = chunk as BonusEngine;
                    heatTolerance += bonusEngine.heatTolerance;
                }
            }

            SetEngineValues();
        }
        

        public void ShipManeuver(Vector3 relativeDir, float clampedVerticalMovement = 1)
        {
            //relDir = Vector3.Normalize(relativeDir);
            _relDir = Calc.SetMaxDimension(relativeDir, 1);

            float yDir = Mathf.Clamp(_relDir.y, -clampedVerticalMovement, clampedVerticalMovement);
            _relDir = new Vector3(_relDir.x, yDir, _relDir.z);

            float forwardDir = Mathf.Abs(_enginePower) / _enginePower;
            if (ShipAnim() == null) return;
            if (Mathf.Abs(_enginePower) > 2) ShipAnim().MoveRudders(_relDir * forwardDir);
        }


        /// <summary>
        /// ShipGlobal with an offset to move around things FOR AI
        /// </summary>
        public void ShipManouverGlobal(Vector3 globalPos, Vector3 avoidanceDir)
        {
            Vector3 relativeDir = globalPos - transform.position;
            
            //TODO Add a Clamped Angle towards relativeDir, that locks at 90 left or right
            Vector3 offsetDir = relativeDir.normalized + avoidanceDir.normalized;
            ShipManeuver(offsetDir);
        }

        /// <summary>
        /// Points ship towards a global position
        /// </summary>
        public void ShipManouverGlobal(Vector3 globalPos)
        {
            Vector3 relativeDir = globalPos - transform.position;
            Vector3 offsetDir = relativeDir.normalized;
            ShipManeuver(offsetDir);
        }

        Avoider avoider;
        Avoider Avoider()
        {
            if (avoider != null) return avoider;
            if (GetComponent<Avoider>())
                return avoider = GetComponent<Avoider>();
            else
                return avoider = gameObject.AddComponent<Avoider>();
        }

        #region engine 

        /// <summary>
        /// Returns a value between 0.5 and 5 depending on how fast we are going
        /// </summary>
        /// <returns></returns>
        public float EngineNoiseMultiplier()
        {
            return Mathf.Clamp(Mathf.Abs(_throttleLevels[(int)throttle]), 0.5f, 5);
        }

        Throttle memThrottle = Throttle.stop;
        /// <summary>
        /// Sets throttle to stop, and remembers what the throttle was set to.
        /// </summary>
        public void NullThrottle()
        {
            memThrottle = throttle;

            if (shipAnim)
                shipAnim.targetPropSpeed = 0;
            _lerpRelDir = transform.forward;
            SetThrottle(Throttle.stop);
        }

        public void ResumeThrottle()
        {
            SetThrottle(memThrottle);
        }
        

        void SetTargetThrottle()
        {
            currentTargetThrottle = (float)throttle;
            if (!lerpingThrottleSound)
                StartCoroutine(LerpThrottleSound());
        }

        public void ThrottleDelta(int steps)
        {
            int throttleLevel = (int)throttle;
            int newThrottleInt = throttleLevel + steps;
            newThrottleInt = Mathf.Clamp(newThrottleInt, 0, _throttleSteps);
            
            // Get the new throttle setting
            Throttle newThrottle = (Throttle) newThrottleInt;
            
            OverdriveAudioEvent(newThrottle);

            if (throttleLevel + steps <= _throttleSteps && throttleLevel + steps >= 0)
                if (steps > 0)
                {
                    if (_myBridge != null)
                        SpiderSound.MakeSound("Play_Speed_Adjust_Raise", gameObject);
                    shipAnim.ShakePropellers();
                }
                else
                {
                    if (_myBridge != null)
                        SpiderSound.MakeSound("Play_Speed_Adjust_Lower", gameObject);
                }

            //set the value
            throttle = newThrottle;

            SetEngineValues();
        }

        public void FullThrottle()
        {
            if (throttle == Throttle.full) return;
            SetThrottle(Throttle.full);
        }

        public void FullReverse()
        {
            if (throttle == Throttle.backFull) return;
            SetThrottle(Throttle.backFull);
        }

        //Override for AI
        public void SetThrottle(int steps)
        {
            int newThrottle = steps;

            //clamp the throttle value
            newThrottle = Mathf.Clamp(newThrottle, 0, _throttleSteps);
            
            OverdriveAudioEvent((Throttle)newThrottle);
            
            //set the value
            throttle = (Throttle)newThrottle;

            SetEngineValues();
        }

        /// <summary>
        /// No Repeat throttle, wont call again if already same throttle
        /// </summary>
        /// <param name="newThrottle"></param>
        public bool NoRepeatThrottle(Throttle newThrottle)
        {
            if (throttle == newThrottle) return false;
            SetThrottle(newThrottle);
            return true;
        }

        //Set throttle for the movement
        public void SetThrottle(Throttle newThrottle)
        {       
            OverdriveAudioEvent(newThrottle);
            throttle = newThrottle;
            SetEngineValues();
        }

        /// <summary>
        /// Plays an audio event based on if the throttle is in overdrive or not
        /// </summary>
        void OverdriveAudioEvent(Throttle newThrottle)
        {
            if (!_myBridge) return;
            if (_myBridge.IsPlayer() == false) return;
            
            if (newThrottle == Throttle.overdrive && throttle != Throttle.overdrive)
                // Play overdrive start sound
                SpiderSound.MakeSound(engine.overdriveStart.ToString(), gameObject);

            if (newThrottle != Throttle.overdrive && throttle == Throttle.overdrive)
                // playe overdrive stop sound
                SpiderSound.MakeSound(engine.overdriveStop.ToString(), gameObject);
        }

        //Set throttle for the movement
        public void SetThrottle(Throttle newThrottle, float percentage)
        {
            // Debug.Log(newThrottle + " is the target throttle " + percentage + " is the percentage");
            throttle = ThrottleDivision(newThrottle, percentage);

            SetEngineValues();
        }

        Throttle ThrottleDivision(Throttle targetThrottle, float percentage)
        {
            int forwardOrBackward = 3;
            if ((int)targetThrottle < 3) // the target throttle is less than slow
                forwardOrBackward = 0; // set it to a backwards number
            int returnThrottle = Mathf.FloorToInt((int)targetThrottle * 1.0f * percentage);
            //Debug.Log((Throttle)returnThrottle + " is the return throttle " + (int)targetThrottle * 1.0f * percentage + " is the int");
            return (Throttle)(forwardOrBackward + returnThrottle);
        }

        ///Sets engine power and propeller speeds based on the current throttle
        void SetEngineValues()
        {
            SetTargetThrottle();
            int tLevel = (int)throttle;
            tLevel = Mathf.Clamp(tLevel, 0, _throttleSteps);

            //get the power level
            _enginePowerNormalized = _throttleLevels[tLevel];
      
            //apply it to engines
            _enginePower = _enginePowerNormalized * engine.maxEnginePower;
            // add extra power
            _enginePower += _enginePower * extraEnginePower;

            //Debug.Log("Extra engine power: " + extraEnginePower + "%");
            SetPropellerSpeed(_enginePowerNormalized * 50);

            //Push hashtag to helmsman
            if (!_myBridge) return;
            if (!_myBridge.IsPlayer()) return;
            _myBridge.crewManager.BroadcastHashtag("throttle");
            
            // display effects for overdrive
            if (shipAnim) shipAnim.SetOverdriveParticles(throttle == Throttle.overdrive);
        }

        //Sets animation for the propellers
        public void SetPropellerSpeed(float speed)
        {
            if (!GetComponent<ShipAnimator>()) return;
            GetComponent<ShipAnimator>().targetPropSpeed = speed;
        }
        #endregion

        public void PilotSideView(float angle)
        {
            transform.localEulerAngles = new Vector3(angle, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }

        Vector3 ShipDirection(Vector3 inputDirection, float speedMultiplier)
        {

            return Vector3.Slerp(_lerpRelDir, inputDirection, Time.deltaTime * speedMultiplier);//move towards it
        }

        #region AngularTorqueDrawers
        #if UNITY_EDITOR
        AngularInstance angularInput;
        AngularInstance AngularInput
        {
            get
            {
                if (angularInput != null) return angularInput;
                angularInput = AngleDrawer().Add();
                angularInput.label = "Angular Input";
                angularInput.myColor = Color.yellow;
                return angularInput;
            }
        }
        
        AngularInstance rotationErrorDrawer;

        AngularInstance RotationErrorDrawer
        {
            get
            {
                if (rotationErrorDrawer != null) return rotationErrorDrawer;
                rotationErrorDrawer = AngleDrawer().Add();
                rotationErrorDrawer.label = "Error";
                rotationErrorDrawer.myColor = Color.red;
                return rotationErrorDrawer;
            }
        }
        
        AngularInstance headingCorrectionDrawer;

        AngularInstance HeadingCorrectionDrawer
        {
            get
            {
                if (headingCorrectionDrawer != null) return headingCorrectionDrawer;
                headingCorrectionDrawer = AngleDrawer().Add();
                headingCorrectionDrawer.label = "Heading";
                headingCorrectionDrawer.myColor = Color.green;
                return headingCorrectionDrawer;
            }
        }
        
        AngularInstance angularVelocityDrawer;

        AngularInstance AngularVelocityDrawer
        {
            get
            {
                if (angularVelocityDrawer != null) return angularVelocityDrawer;
                angularVelocityDrawer = AngleDrawer().Add();
                angularVelocityDrawer.label = "Angular";
                angularVelocityDrawer.myColor = Color.blue;
                return angularVelocityDrawer;
            }
        }
        #endif
        #endregion
        void PIDRotate()
        {
            //TODO add a case for the PID to rotate towards the dock's tangent angle
            if (_errorController == null) return;
            if (_headingController == null) return;
            float forwardVelocity = Mathf.Abs(transform.InverseTransformDirection(myRigidbody().velocity).z);

            if (forwardVelocity > engine.minVelForTurn)
            {
                _lerpRelDir = ShipDirection(_relDir, lerpRotationSpeed / 5 * Mathf.Clamp(forwardVelocity, 0.6f, 5));
            }
            else
            {
                Vector3 flatForward = new Vector3(transform.forward.x, 0, transform.forward.z);//TODO Get the dock's tangent angle and set the flat forward to that
                _lerpRelDir = ShipDirection(flatForward, lerpRotationSpeed);//move towards it
            }

           
            Vector3 angularVelocityError = transform.InverseTransformDirection(myRigidbody().angularVelocity) * -1;
            
            #if UNITY_EDITOR
            if (MoverDebug())
            {
                AngularInput.SetAngularTorque(angularVelocityError);
            }
            #endif
            
          
            Vector3 angularVelocityCorrection = Vector3.zero;

            if (testing)
                angularVelocityCorrection = _errorController.Update(angularVelocityError, Time.deltaTime, engine.correctionProp, errorInt, engine.correctionDer); 
            else
                angularVelocityCorrection = _errorController.Update(angularVelocityError, Time.deltaTime);
            
            
            #if UNITY_EDITOR
            if (MoverDebug())
            {
                RotationErrorDrawer.SetAngularTorque(angularVelocityCorrection);
            }
            #endif
            

            Vector3 errorTorque = transform.TransformDirection(angularVelocityCorrection * engine.rotationDamping);
            

            // Sets the up Value so the ship stays upright
            Vector3 stabilityCross = transform.InverseTransformDirection(Vector3.Cross(transform.up, Vector3.up) * engine.stability);


            //adds the z rotation to the upright vector in worldspace
            Vector3 localOrientation = transform.forward;
            Vector3 targetOrientation = new Vector3(_lerpRelDir.x, _lerpRelDir.y, _lerpRelDir.z);
            
          //  Vector3 turnOrientation = 

            //local space target final rotation
            Vector3 headingError = transform.InverseTransformDirection(Vector3.Cross(localOrientation, targetOrientation));
            Vector3 headingErrorConstraint = new Vector3(headingError.x, headingError.y, headingError.z + stabilityCross.z);

            //applies the PID controller
            Vector3 headingCorrection = Vector3.zero;
            if (testing)
                headingCorrection = _headingController.Update(headingErrorConstraint, Time.deltaTime, engine.rotationProp, rotationInt, engine.rotationProp);
            else
                headingCorrection = _headingController.Update(headingErrorConstraint, Time.deltaTime);

            
            #if UNITY_EDITOR
            if (MoverDebug())
            {
                HeadingCorrectionDrawer.SetAngularTorque(headingCorrection);
                AngularVelocityDrawer.SetAngularTorque(myRigidbody().angularVelocity);
            }
            #endif
            
            //Only allows turning if moving over a certain speed
            myRigidbody().AddRelativeTorque(headingCorrection * engine.rotationSpeed + errorTorque);
        }
            

        #region audio
        
        bool allowTurnCreak = true;
        bool lerpingThrottleSound;
        float currentTargetThrottle = 2;
        float throttleLerpValue = 2;
        
        
        /// <summary>
        /// Increases or decreases throttle the given amount of steps.  Will automatically clamp to min 
        /// and max throttle values.
        /// </summary>
        IEnumerator LerpThrottleSound()
        {
            lerpingThrottleSound = true;

            while (throttleLerpValue != currentTargetThrottle)
            {
                yield return new WaitForEndOfFrame();
                throttleLerpValue = Mathf.MoveTowards(throttleLerpValue, currentTargetThrottle, 2 * Time.deltaTime);

                if (_myBridge != null)
                    SpiderSound.TweakRTPC("Ship_Throttle", throttleLerpValue, gameObject);
            }
            lerpingThrottleSound = false;
        }
        
        void MakeTurningSound()
        {
            //TODO make WWISE Swooshing RTPC

            if (_swooshSoundTime > 0 && allowTurnCreak == false)
            {
                _swooshSoundTime -= Time.deltaTime;
            }
            else
            {
                _swooshSoundTime = SwooshSoundtimer;
                allowTurnCreak = true;
            }

            float turnRate = myRigidbody().angularVelocity.magnitude;
            if (_myBridge != null)
                SpiderSound.TweakRTPC("Ship_Turn", turnRate, gameObject);
            if (turnRate < 1.5f) return;
            if (!allowTurnCreak) return;
            //Debug.Log("TurnCreakNoise");
            allowTurnCreak = false;
            if (_myBridge != null)
                SpiderSound.MakeSound("Play_Turn_Creak", gameObject);

        }

        //Controls the Update movement sounds
        void MovementSounds()
        {
            if (_myBridge == null) return;
            if (!_myBridge.IsPlayer()) return;

            SpiderSound.TweakRTPC("Ship_Speed", myRigidbody().velocity.sqrMagnitude, gameObject);

            //TODO WWISE LOOP (cant play it in update)
            MakeTurningSound();
        }
        
        void StopAmbientSounds()
        {
            if (!Application.isPlaying) return;
            if (_myBridge == null) return;
            SpiderSound.MakeSound("Stop_Ship_Movement", gameObject);
            SpiderSound.MakeSound("Stop_" + engine.engineSound.ToString(), gameObject);
        }
        
        #endregion

      

        #region updates

        /*
        private AirTanks _airTanks;

        AirTanks AirTank()
        {
            if (_airTanks) return _airTanks;
            _airTanks = GetComponent<AirTanks>();
            return _airTanks;
        }
        
        bool AirForOverdrive()
        {
            if (!AirTank()) return false;
            return AirTank().air > 0;
        }
        */

        bool OverdriveAvailable()
        {
            if (overheated) return false;
            return NormalizedOverheatAmount() < 1;
        }
        
        void Update()
        {
            BuyoancyControl();
            MovementSounds();

            // Overdrive Behavior
            if (throttle == Throttle.overdrive)
            {
                if ( OverdriveAvailable())
                {
                    // heat up engines
                    heat += Time.deltaTime / 2;
                }
                else SetThrottle(Throttle.full);
            }

            if (heat > 0) heat -= Time.deltaTime / 30;

            // get local velocity
            if (_inControl)
            {
                //the ship's local velocity
                _localVelocity = transform.InverseTransformDirection(myRigidbody().velocity);

                //the drag from the ship moving sideways / up and down
                _localXyVelocity = new Vector3(_localVelocity.x, 0, 0);
            }
        }

        void FixedUpdate()
        {
            if (!_inControl) return;

            PIDRotate();
          
            //adds force based on the engine output
            myRigidbody().AddRelativeForce(new Vector3(0, 0, _enginePower * clampedEnginePower));

            //add force for strafe drag
            myRigidbody().AddRelativeForce(-_localXyVelocity * engine.strafeDrag);
        }

        bool ascending = false;
        bool descending = false;
        public void ChangeBallast(float updown)
        {
            if (!enabled) return;

            myRigidbody().AddForce(new Vector3(0, updown * myRigidbody().mass, 0) * Time.deltaTime * 40);

            // The rest of the stuff is effects - needed only for player ship
            if (_myBridge == null) return;
            if (!_myBridge.IsPlayer()) return;

            //when changing depth, increase ballast pressure
            _ballastPressure = updown;
        }

        #region BallastPArticles
        GameObject asp;
        GameObject AscendingSpoutParticle()
        {
            if (asp != null) return asp;
            asp = Resources.Load("effects/AscendingParticleFX") as GameObject;
            _ascendingSpoutEmissionMax = asp.GetComponent<ParticleSystem>().emission.rate.constantMax;
            return asp;
        }

        GameObject dbp;
        GameObject DescendingBubbleParticle()
        {
            if (dbp != null) return dbp;
            dbp = Resources.Load("effects/DescendingParticleFX") as GameObject;
            _descendingBubbleEmisionMax = dbp.GetComponent<ParticleSystem>().emission.rate.constantMax;
            return dbp;
        }

        List<ParticlePlacer> ParticlePlacerSpots()
        {
            if (_placerSpots != null) return _placerSpots;
            _placerSpots = new List<ParticlePlacer>(GetComponentsInChildren<ParticlePlacer>());
            if (_placerSpots.Count < 1)
            {
                GameObject defaultSpots = Instantiate(Resources.Load("effects/BallastParticles"), transform.position, transform.rotation) as GameObject;
                defaultSpots.transform.SetParent(transform);
                _placerSpots = new List<ParticlePlacer>(defaultSpots.GetComponentsInChildren<ParticlePlacer>());
            }
            return _placerSpots;
        }

        List<AscendingParticlePlacer> AscendingSpoutSpots()
        {
            if (_ascendingSpoutSpots != null) return _ascendingSpoutSpots;
            _ascendingSpoutSpots = new List<AscendingParticlePlacer>();
            foreach (ParticlePlacer ascendingSpot in ParticlePlacerSpots())
            {
                AscendingParticlePlacer a = ascendingSpot as AscendingParticlePlacer;
                if (a == null) continue;
                _ascendingSpoutSpots.Add(a);
            }
            return _ascendingSpoutSpots;
        }

        List<DescendingParticlePlacer> DescendingBubbleSpots()
        {
            if (_descendingBubbleSpots != null) return _descendingBubbleSpots;
            _descendingBubbleSpots = new List<DescendingParticlePlacer>();
            foreach (ParticlePlacer descendingSpot in ParticlePlacerSpots())
            {
                DescendingParticlePlacer d = descendingSpot as DescendingParticlePlacer;
                if (d == null) continue;
                _descendingBubbleSpots.Add(d);
            }
            return _descendingBubbleSpots;
        }

        float ascendingEmissionRate;
        public void AscendingSpoutFX()
        {
            if (_ascendingSpoutParticleList.Count < 1)
            {
                ascendingEmissionRate = _ascendingSpoutEmissionMax;
                foreach (AscendingParticlePlacer a in AscendingSpoutSpots())
                {
                    Transform t = a.transform;
                    GameObject p = Instantiate(AscendingSpoutParticle(), t.transform.position, t.transform.rotation) as GameObject;
                    p.transform.SetParent(t);
                    _ascendingSpoutParticleList.Add(p.GetComponent<ParticleSystem>());
                }
            }

        }

        float descendingEmissionRate;
        public void DescendingBubbleFX()
        {
            if (_descendingBubbleParticleList.Count < 1)
            {
                descendingEmissionRate = _descendingBubbleEmisionMax;
                foreach (DescendingParticlePlacer d in DescendingBubbleSpots())
                {
                    Transform t = d.transform;
                    GameObject p = Instantiate(DescendingBubbleParticle(), t.transform.position, t.transform.rotation) as GameObject;
                    p.transform.SetParent(t);
                    _descendingBubbleParticleList.Add(p.GetComponent<ParticleSystem>());
                }
            }
        }

        void SetRate(ParticleSystem p, float target)
        {
            var em = p.emission;
            var rate = new ParticleSystem.MinMaxCurve(target);
            em.rate = rate;
        }


        public float SetAscendingEmissionTarget(float target)
        {
            if (_ascendingSpoutParticleList == null) return 0;
            ascendingEmissionRate = Mathf.MoveTowards(ascendingEmissionRate, target, Time.deltaTime * _ascendingSpoutEmissionMax);

            if (ascendingEmissionRate == target) return ascendingEmissionRate;

            foreach (ParticleSystem p in _ascendingSpoutParticleList)
            {
                SetRate(p, ascendingEmissionRate);
            }
            return ascendingEmissionRate;
        }

        public float SetDescendingEmissionTarget(float target)
        {
            if (_descendingBubbleParticleList == null) return 0;
            descendingEmissionRate = Mathf.MoveTowards(descendingEmissionRate, target, Time.deltaTime * _descendingBubbleEmisionMax);

            if (descendingEmissionRate == target) return descendingEmissionRate;
            foreach (ParticleSystem p in _descendingBubbleParticleList)
                SetRate(p, descendingEmissionRate);

            return descendingEmissionRate;
        }

        void RemoveAscendingParticles()
        {
            foreach (ParticleSystem p in _ascendingSpoutParticleList)
                Destroy(p.gameObject, 5);
            _ascendingSpoutParticleList.Clear();

        }

        void RemoveDescendingParticles()
        {
            foreach (ParticleSystem p in _descendingBubbleParticleList)
                Destroy(p.gameObject, 5);
            _descendingBubbleParticleList.Clear();
        }

        void Destroyparticle(GameObject particle, float time)
        {
            if (particle == null) return;
            ParticleSystem targetParticle = particle.GetComponent<ParticleSystem>();
            if (targetParticle == null) Destroy(particle, time);

        }

        #endregion

        /// <summary>
        /// Controls everything related to buoyancy
        /// </summary>
        void BuyoancyControl()
        {
            float ballastThreshhold = .4f;


            //ASCENSION
            if (_ballastPressure > ballastThreshhold)
            {
                if (!ascending)
                {
                    ascending = true;
                    AscendingSpoutFX();
                    if (_myBridge != null)                    
                        SpiderSound.MakeSound("Play_Ballast_Ascending", gameObject);
                }
                SetAscendingEmissionTarget(_ascendingSpoutEmissionMax);
            }
            else if (_ballastPressure <= 0)
            {
                if (SetAscendingEmissionTarget(0) < 1)
                {
                    RemoveAscendingParticles();
                    if (_myBridge != null)
                        SpiderSound.MakeSound("Stop_Ballast_Ascending", gameObject);
                    ascending = false;
                }
            }

            //DIVING
            if (_ballastPressure < -ballastThreshhold)
            {
                if (!descending)
                {
                    descending = true;
                    DescendingBubbleFX();
                    if (_myBridge != null)
                        SpiderSound.MakeSound("Play_Ballast_Diving", gameObject);
                }
                SetDescendingEmissionTarget(_descendingBubbleEmisionMax);
            }
            else if (_ballastPressure >= 0)
            {
                if (SetDescendingEmissionTarget(0) < 1)
                {
                    RemoveDescendingParticles();
                    if (_myBridge != null)
                        SpiderSound.MakeSound("Stop_Ballast_Diving", gameObject);
                    descending = false;
                }
            }
        }
        
        #endregion

        public Vector3 GetLocalVelocity()
        {
            return _localVelocity;
        }

        public void Control(bool c)
        {
            _inControl = c;
        }
        
        void OnDisable()
        {
            StopAmbientSounds();
        }
    }
}