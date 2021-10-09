using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using DUI;

namespace Diluvion.Sonar
{
    /// <summary>
    /// The basic listening ability of ships. This doesn't give the ability to ping, but does give
    /// the basic listening radius.
    /// <para>To keep performance reasonable, only one listener does a listen check per frame. So if there's 50 listeners,
    /// each one will check ever 50 frames.</para>
    /// </summary>
    public class Listener : MonoBehaviour, IHUDable
    {
        static int _activeIndex;
        static List<Listener> _allListeners = new List<Listener>();

        /// <summary>
        /// Signatures 1 units away will gain power at a rate of sonarPower / second. 10 units away will gain power at rate of sonarPower / 10 seconds
        /// </summary>
        static float _passivePower = 5;

        /// <summary>
        /// Signature power will decay this many units per second.
        /// </summary>
        static float _signatureDecay = .2f;

        [ReadOnly]
        public SonarModule sonarModule;

        public float minRange ;  // this can be set by sonar ping.

        /// <summary>
        /// Ping can modify this value. Gets added to passive power to define total power.
        /// </summary>
        public float pingPower;

        /// <summary>
        /// Multiplies the default listening range.
        /// </summary>
        public float listenRangeMultiplier = 1;

        /// <summary>
        /// Multiplies the listening power, gaining more power per ping / listen
        /// </summary>
        public float powerMultiplier = 1;

        public delegate void ContactUpdated(SonarStats contact);
        public ContactUpdated contactGained;
        public ContactUpdated contactLost;

        int _index;
        
        float _defaultRange = 50;

        bool _actedInUpdate;


        private List<int> _myChildStats = new List<int>();
        public List<int> MyChildStats
        {
            get
            {
                if (_myChildStats != null && _myChildStats.Count > 0) return _myChildStats;
                foreach (SonarStats ss in GetComponentsInChildren<SonarStats>())
                {
                    if (ss == _mySignature) continue;
                    _myChildStats.Add(ss.GetInstanceID());
                }
                return _myChildStats;
            }
        }
        
            /// <summary>
        /// Dictionary with instance ID and sonar stats instance for all signatures I currently know about
        /// </summary>
        Dictionary<int, SonarStats> _contacts = new Dictionary<int, SonarStats>();

        /// <summary>
        /// Dictionary with the instanceID of all sonar stats in scene, and their distance to this listener.
        /// </summary>
        Dictionary<int, float> _sDistance = new Dictionary<int, float>();

        /// <summary>
        /// Dictionary with instanceID of all sonar stats, and the 'power' of the signature. The higher the power, the more is known about 
        /// the signature.
        /// </summary>
        Dictionary<int, float> _sPower = new Dictionary<int, float>();

        /// <summary>
        /// Dictionary for hud of all the sonar signatures
        /// </summary>
        Dictionary<int, SignatureHUD> _sHud = new Dictionary<int, SignatureHUD>();

        /// <summary>
        /// Shorthand for the global list of all sonarstats
        /// </summary>
        List<SonarStats> _allStats;

        int _targetedId;
        List<Ping> _activePings = new List<Ping>();
        LayerMask _shipMask;
        LayerMask _terrainMask;
        public List<SonarStats> inRangeStats = new List<SonarStats>();
      

        SonarStats _mySignature;
        const float buffer = 30;
        bool _showingUi;
        float _lastUpdate;

        static void IterateIndex()
        {
            _activeIndex--;
            if (_activeIndex < 0)
                _activeIndex = _allListeners.Count - 1;
        }

        /// <summary>
        /// Resets the indexes of all the listeners. This is called after a listener is destroyed.
        /// </summary>
        static void ResetListenerIndexes()
        {
            for (int i = 0; i < _allListeners.Count; i++)
            {
                _allListeners[i]._index = i;
            }
            _activeIndex = 0;
        }

        void Awake()
        {
            _index = _allListeners.Count;
            _allListeners.Add(this);
            _shipMask = LayerMask.GetMask("Default");
            _terrainMask = LayerMask.GetMask("Terrain");
        }

        // Use this for initialization
        void Start()
        {
            _mySignature = GetComponent<SonarStats>();
            _allStats = SonarStats.allSonarStats;
        }

        public void CreateUI()
        {
            _showingUi = true;
        }

        public void AddPing(Ping p)
        {
            if (_activePings.Contains(p)) return;
            _activePings.Add(p);
        }

        public void RemovePing(Ping p)
        {
            if (!_activePings.Contains(p)) return;
            _activePings.Remove(p);
        }
    

        public void RemoveUI()
        {
            _showingUi = false;
            foreach (SignatureHUD h in _sHud.Values)
            {
                if (h == null) continue;
                h.End();
            }
            _sHud.Clear();
        }

        
        //if we hit a terrain before hitting the target, return false
        public bool InLOS(Transform target, float range)
        {
            return !Physics.Raycast(transform.position, target.position - transform.position, range, _terrainMask);
        }

        float _tempRange = 50;
        // Update is called once per frame
        void Update()
        {
           //Debug.Log("Update is: " + activeIndex + " / " + index + " on " + gameObject.name, gameObject);
            // This index check makes sure that only one listener can update per frame.
            if (_activeIndex == _index)
            {
                _actedInUpdate = true;

                float deltaTime = Time.time - _lastUpdate;
                
                _tempRange = Range();
                _tempRange = Mathf.Clamp(_tempRange, minRange, 9999);
                
                float totalPower = _passivePower * powerMultiplier + pingPower;
              
                for (int i = 0; i < _allStats.Count; i++)
                {
                    if (_allStats[i] == null) continue;
                    if (_allStats[i] == _mySignature) continue;

                    Transform statTrans = _allStats[i].transform;
                    int sigID = _allStats[i].GetInstanceID();
                    
      
                    if (MyChildStats.Contains(sigID)) continue;//if this is one of my children, ignore it
                    //If the signature isn't in the list, add it
                    if (!_sDistance.ContainsKey(sigID)) _sDistance.Add(sigID, 1000);
                    if (!_sPower.ContainsKey(sigID)) _sPower.Add(sigID, 0);
                    
              
                    // set the actual distance to the sonar stat
                    _sDistance[sigID] = Vector3.Distance(transform.position, statTrans.position);

                    bool inLos = InLOS(statTrans, _sDistance[sigID]);
                  
     
                    foreach (Ping p in _activePings)
                    {
                        if (p == null) continue;
                        var result = p.Pinged(sigID, _sDistance[sigID],inLos);
                        if (result == PingResult.Invalid) continue;
                        
                        // Check the thing's alignment

                        sonarModule.CreatePingedEffect(_allStats[i].gameObject);
                        /*
                        // instantiate effects object for stuff that gets pinged
                        GameObject effects = sonarModule.EffectObject(result);
                        if (effects)
                            Destroy(Instantiate(
                                effects, statTrans.position, statTrans.rotation), 3);
                                */
                        
                        // Tell the sonar stats that it was pinged
                        _allStats[i].Pinged(this, result);
                    }

                    // set the power
                    float powerDelta = (totalPower / _sDistance[sigID]) - _signatureDecay;
                    _sPower[sigID] += deltaTime * powerDelta;
                    _sPower[sigID] = Mathf.Clamp01(_sPower[sigID]);

                    CheckContact(_allStats[i], _tempRange,inLos);

                    // UI for sonar signatures
                    if (_showingUi) ManageHUD(_allStats[i]);
                }

             
                _lastUpdate = Time.time;
            }
            else _actedInUpdate = false;
        }


        private SignatureHUD _hud;
        void ManageHUD(SonarStats ss)
        {
            _hud = null;
            
            // Check the dictionary for a hud element related to the given sonar stats' instance ID
            int ID = ss.GetInstanceID();
            _sHud.TryGetValue(ID, out _hud);
            
            // If it exists, take no further action.
            if (_hud != null) return;

            // otherwise create the hud element
            // Remove any previous signature hud from the dictionary
            if(_sHud.ContainsKey(ID)) _sHud.Remove(ID);
            // Create the new signature hud, and add it tot he dictionary
            SignatureHUD newHUD = SignatureHUD.CreateHUD(ss, this);
            _sHud.Add(ID, newHUD);
        }


        void LateUpdate()
        {
            if (_activeIndex == _index && _actedInUpdate)
            {
             //   Debug.Log("Late Update iterating on: " + gameObject.name, gameObject);
                IterateIndex();
                _actedInUpdate = false;
                return;
            }

            if (_activeIndex == 0) return;
            if (_allStats == null) return;
            if (_activeIndex < 0 || _activeIndex >= _allStats.Count)
                _activeIndex = 0;
        }

        private void OnDisable()
        {
            _allListeners.Remove(this);
            ResetListenerIndexes();
            ResetContacts();
            RemoveUI();
            //RemoveAllHudElements();
        }

        void ResetContacts()
        {
            inRangeStats.Clear();
            _contacts.Clear();
        }

        /*
        /// <summary>
        /// Clears out all the hud elements related to this listener
        /// </summary>
        void RemoveAllHudElements()
        {
            foreach (SignatureHUD HUD in _sHud.Values)
            {
                if (HUD == null) continue;
                Destroy(HUD.gameObject);
            }
        }
        */

        /// <summary>
        /// If a sonarstats has at least one property exposed here, then it's valid for leading.
        /// </summary>
        public bool ValidForLeading(SonarStats signature, float maxRange)
        {
            if (!ContactExposed(signature)) return false;
            if (!ContactInRange(signature, maxRange)) return false;

            foreach (Signature s in signature.signatures)
            {
                if (s == null) continue;
                if (s.revealStrengh <= PowerOfSignature(signature)) return true;
            }

            return false;
        }

        List<SonarStats> _returnList = new List<SonarStats>();
        public List<SonarStats> ValidForLeading(float maxRange)
        {
            _returnList.Clear();

            foreach (var sig in ContactsInRange(2, maxRange))
            {
                if (ValidForLeading(sig, maxRange)) _returnList.Add(sig);
            }

            return _returnList;
        }
        
        /// <summary>
        /// Checks if contact c is in the given range. If so, adds it to the list of exposed contacts.
        /// If contact is out of range, removes it from the list of contacts. Both call a delegate; contactGained and contactLost.
        /// </summary>
        void CheckContact(SonarStats c, float newRange, bool inLos)
        {
            if (c == _mySignature) { return;}
            //if (!InBuffer(c, newRange)) { return;} TODO what is the goal of this line?

            int ID = c.GetInstanceID();

            SonarStats newSig = null;

            bool visible = ContactInRangeAndLos(c, newRange, inLos);

            //TODO while pinging, Ping sonarstats with information
            //Debug.Log("Found Contact in Range: "+ c.name );

            // If this contact is in range but hasn't yet been added, then add it.
            if (visible && !_contacts.ContainsKey(ID)&&c.gameObject.activeInHierarchy)
            {
                _contacts.Add(ID, c);
                if (contactGained != null) contactGained(c);
                inRangeStats.Add(c);
//                Debug.Log("added new contact " + c.name);
            }

            // If I've just lost this contact...
            else if ((!visible||!c.gameObject.activeInHierarchy) && _contacts.ContainsKey(ID))
            {
                _contacts.Remove(ID);
                if (contactLost != null) contactLost(c);
                inRangeStats.Remove(c);
            //    Debug.Log("removed old contact " + c.name);
            }
        }

        /// <summary>
        /// Checks if this contact is near enough to the border to check if it should be added/removed from
        /// visible contacts.
        /// </summary>
        bool InBuffer(SonarStats c, float maxRange)
        {
            float dist = Mathf.Abs(DistanceToSignature(c) - maxRange);
           // Debug.Log(dist + " = bufferRange " + maxRange + " = maxrange" + DistanceToSignature(c) + " = realdistance");
            if (dist < buffer) return true;
            return false;
        }

        public bool ContactInRangeAndLos(SonarStats contact, float range, bool inLos)
        {
            bool inRange = ContactInRange(contact, range);
            //Debug.Log(contact.name  + " is in LoS: " + inLos + " and in range: " + range);
            return inLos && inRange;
        }

        public bool ContactInRange(SonarStats contact, float range)
        {
            return DistanceToSignature(contact) <= range;
        }

        /// <summary>
        /// Is the given sonar stats contained in the list of exposed contacts?
        /// </summary>
        public bool ContactExposed(SonarStats contact)
        {
            if (contact == null || !contact.gameObject.activeInHierarchy) return false;
            return _contacts.ContainsKey(contact.GetInstanceID());
        }

        /// <summary>
        /// Returns all contacts within the specified range.
        /// </summary>  
        public List<SonarStats> ContactsInRange(float minRange, float maxRange)
        {
            List<SonarStats> c = new List<SonarStats>();
            for (int i = 0; i < _allStats.Count; i++)
            {
                if (_allStats[i] == null) continue;
                float dist = DistanceToSignature(_allStats[i]);
                if (dist > maxRange) continue;
                if (dist < minRange) continue;
                c.Add(_allStats[i]);
            }

            return c;
        }

        /// <summary>
        /// Returns the distance from this listener to the given signature.
        /// </summary>
        float DistanceToSignature(SonarStats signature)
        {
            float dist = 1000;
            _sDistance.TryGetValue(signature.GetInstanceID(), out dist);
            return dist;
        }

        public float PowerOfSignature(SonarStats signature)
        {
            float power = 0;
            _sPower.TryGetValue(signature.GetInstanceID(), out power);
            return power;
        }


        List<SonarStats> _omniInRangeSignatures = new List<SonarStats>();
        List<SonarStats> _omniFilteredSigs = new List<SonarStats>();
        /// <summary>
        /// Finds all signatures with tags ignoring raycasts and signal strength
        /// </summary>
        public List<SonarStats> OmniSigsWithTags(List<Signature> includeTags, float maxRange=99999)
        {
            _omniInRangeSignatures = ContactsInRange(1, maxRange); //CHECK CONTACTS IN RANGE AND ALLSTATS
            _omniFilteredSigs.Clear();

            for (int i = 0; i < _omniInRangeSignatures.Count; i++)
            {          
                if (includeTags != null && includeTags.Count > 0)
                {
                    foreach (Signature s in includeTags)
                        if (_omniInRangeSignatures[i].HasSignature(s))
                        {
                            _omniFilteredSigs.Add(_omniInRangeSignatures[i]);
                            break;
                        }
                }
                else
                    _omniFilteredSigs.Add(_omniInRangeSignatures[i]);
            }
            return _omniFilteredSigs;
        }



        /// <summary>
        /// Returns all the Signatures with tags, orderedbydistance, if the includetags are empty, it will default to returning everything
        /// </summary>
        /// <param name="includeTags"></param>
        /// <param name="maxRange"></param>
        /// <returns></returns>   
        List<SonarStats> _inRangesignatures = new List<SonarStats>();
        List<SonarStats> _filteredSigs = new List<SonarStats>();
        public List<SonarStats> SigsWithTags(List<Signature> includeTags, float maxRange)
        {
            _inRangesignatures = ContactsInRange(1, maxRange); // TODO YAGNI 
            _filteredSigs.Clear();

            for (int i = 0; i < _inRangesignatures.Count; i++)
            {
                // Check if the signature has the include tags and doesn't have exclude tags
                if (includeTags != null && includeTags.Count > 0)
                {
                    foreach (Signature s in includeTags)
                        if (SignatureExposed(s, _inRangesignatures[i]))
                        {
                            _filteredSigs.Add(_inRangesignatures[i]);
                            break;
                        }
                }
                else
                {                    
                    _filteredSigs.Add(_inRangesignatures[i]);
                }
            }

            return _filteredSigs;
        }

        List<SonarStats> _distanceFilteredSigs = new List<SonarStats>();
        public List<SonarStats> DistanceSortedAllSigWithTags(List<Signature> includeTags, float maxRange)
        {
            _distanceFilteredSigs = SigsWithTags(includeTags, maxRange);
            if (_distanceFilteredSigs.Count < 1) return null;
            _distanceFilteredSigs.OrderBy(DistanceToSignature);
            return _distanceFilteredSigs;
        }


        /// <summary>
        /// Returns the closest sonar signature with the given tags
        /// </summary>
        public SonarStats ClosestSigWithTags(List<Signature> includeTags, float maxRange = 300)
        {
            List<SonarStats> distanceSigs = DistanceSortedAllSigWithTags(includeTags, maxRange);
            if (distanceSigs == null || distanceSigs.Count < 1) return null;
            return distanceSigs[0];
        }

        /// <summary>
        /// Is the given signature exposed on the given sonar stats?
        /// </summary>
        bool SignatureExposed(Signature sig, SonarStats stats)
        {
            if (!stats.HasSignature(sig)) return false;
            float p = PowerOfSignature(stats);
            return (p >= sig.revealStrengh);
        }

        /// <summary>
        /// Returns the listening range of this instance.
        /// </summary>
        float Range() { return _defaultRange * listenRangeMultiplier; }
    }
}