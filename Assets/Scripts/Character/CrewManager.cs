using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Diluvion.Ships
{

    /// <summary>
    /// Class for managing crew on a ship. Keeps track of characters on board, and stations / heart station.
    /// </summary>
    [RequireComponent(typeof(InteriorManager))]
    public class CrewManager : MonoBehaviour
    {

        public List<Station> shipStations = new List<Station>();
        
        [ReadOnly, Tooltip("All characters that are in a place they wouldn't normally be. Like a gunner officer being in" +
                           " the sonar station, for instance. ")]
        public List<Character> guests = new List<Character>();
        public HeartStation heartStation;
        public int maxCrew = 0;

        public delegate void PostInit();
        public PostInit postInit;
        public Bridge bridge;

        bool stationsInit = false;
        InteriorManager interiorManager;

        private void Awake()
        {
            bridge = GetComponentInParent<Bridge>();
        }

        private void Start()
        {
            interiorManager = GetComponent<InteriorManager>();

            //Find heart station & give it crew manager reference
            heartStation = GetComponentInChildren<HeartStation>();
        }

        #region crew finding
        /// <summary>
        /// Returns every character on board the ship, whether guest, officer or sailor.
        /// </summary>
        public List<Character> AllCharactersOnBoard()
        {
            List<Character> allChars = new List<Character>();
            allChars.AddRange(GetComponentsInChildren<Character>());
            return allChars;
        }

        public List<Sailor> AllSailorsOnBoard()
        {
            List<Sailor> allSailors = new List<Sailor>();
            allSailors.AddRange(GetComponentsInChildren<Sailor>());
            return allSailors;
        }

        public List<Officer> AllOfficersOnBoard()
        {
            List<Officer> allOfficers = new List<Officer>();
            allOfficers.AddRange(GetComponentsInChildren<Officer>());
            return allOfficers;
        }

        public int TotalSailors() { return AllSailorsOnBoard().Count; }


        /// <summary>
        /// Returns the crew component of the officer with the given skill.
        /// </summary>
        public Officer GetOfficer(ShipModule skill)
        {
            foreach (Officer officer in AllOfficersOnBoard())
            {
                if (!officer.characterInfo) continue;
                if (officer.characterInfo.skill == skill) return officer;
            }

            return null;
        }

        /// <summary>
        /// Returns all sailors that aren't currently in a 'working' station. I.e., they chillin in the
        /// heart station.
        /// </summary>
        public List<Sailor> AvailableSailors()
        {
            List<Sailor> returnList = new List<Sailor>();

            Sailor[] resting = heartStation.GetComponentsInChildren<Sailor>();
            if (resting.Length < 1) return null;

            foreach (Sailor crew in resting)
            {
                if (crew == null) continue;
                returnList.Add(crew);
            }

            return returnList;
        }

        public Sailor GetRestingSailor()
        {
            List<Sailor> available = AvailableSailors();
            if (available.Count < 1) return null;
            return available[0];
        }


        public bool HasSailor(Sailor s)
        {
            foreach (Sailor sailor in AllSailorsOnBoard())
                if (sailor.generatedName == s.generatedName) return true;

            return false;
        }

        public bool HasCharacter(CharacterInfo ch)
        {
            if (!ch) return false;
            
            foreach (Character c in AllCharactersOnBoard())
                if (c.characterInfo == ch)
                {
                    //Debug.Log("Character " + ch.name + " is on board the player ship.");
                    return true;
                }

            //Debug.Log("Character " + ch.name + " is not on board the player ship.");
            return false;
        }

        /// <summary>
        /// Returns the next available sailor.  Checks for resting sailors first,
        /// then checks all.
        /// </summary>
        public Sailor GetSailor()
        {
            List<Sailor> availableSailors = new List<Sailor>();

            Sailor restingSailor = GetRestingSailor();
            if (restingSailor) return restingSailor;

            //If no sailors were resting, just add all sailors to list of available
            foreach (Sailor s in AllSailorsOnBoard())
                availableSailors.Add(s);

            //Choose a random sailor from the list of available
            int rand = Random.Range(0, availableSailors.Count);
            Sailor selected = availableSailors[rand];

            return selected;
        }

        /// <summary>
        /// Automatically checks if the given character is a sailor, and if so, uses
        /// the sailor comparison. Otherwise compares by character info to check for the character.
        /// </summary>
        public bool HasCrewmember(Character ch)
        {
            Sailor s = ch as Sailor;
            if (s) return HasSailor(s);

            return HasCharacter(ch.characterInfo);
        }

        #endregion

        #region stations
        List<Station> AllStations()
        {
            shipStations.Clear();
            shipStations.AddRange(GetComponentsInChildren<Station>());
            return shipStations;
        }

        public HeartStation MyHeartStation()
        {
            return GetComponentInChildren<HeartStation>();
        }

        /// <summary>
        /// Returns a station linked to the given module
        /// </summary>
        public Station FindStationOfType(ShipModule type)
        {
            foreach (Station st in AllStations())
                if (st.linkedModule == type) return st;

            return null;
        }

        /// <summary>
        /// Returns a station with the given lockey.
        /// </summary>
        public Station FindStationOfType(string locKey)
        {
            foreach (Station st in AllStations())
                if (st.stationLocKey == locKey) return st;
            return null;
        }
        #endregion

        /// <summary>
        /// Moves a crewman of the given name to a station of the given type
        /// </summary>
        public void MoveCrew(CharacterInfo characterInfo, ShipModule moduleStation)
        {

            Debug.Log("Attempting to move " + characterInfo.name + " to " + moduleStation.name);

            Character crewToMove = null;
            Station toStation = FindStationOfType(moduleStation);

            // Make sure we have this kind of station
            if (!toStation)
            {
                Debug.LogError("Can't move " + characterInfo.name + " to " + moduleStation.name + " because " + name +
                    " doesn't have one!", gameObject);
                return;
            }

            // Make sure the character instance is on board
            if (!HasCharacter(characterInfo))
            {
                Debug.LogError("Can't move " + characterInfo.name + " because theyre not on this ship!", gameObject);
                return;
            }

            // Find the character instance
            foreach (var ch in AllCharactersOnBoard())
            {
                if (ch.characterInfo == characterInfo)
                {
                    crewToMove = ch;
                    break;
                }
            }

            crewToMove.JoinStation(toStation);
        }


        /// <summary>
        /// If any crewmembers are guests in other stations, return them to where they belong.
        /// </summary>
        public void RemoveAllGuests()
        {
            foreach (var ch in guests)
            {
                ch.JoinDefaultStation();
            }
        }

        /// <summary>
        /// Adds the given crewman to this crew
        /// </summary>
        public void AddCrewman(Character crewman)
        {
            if (crewman == null) return;
            //Debug.Log("Adding character " + crewman.niceName + " to crew.");
            crewman.transform.parent = transform;
        }

        /// <summary>
        /// Broadcasts a given hashtag to all the dialogue objects within this interior.  The dialogue objects
        /// will then check their dialogue bits for anything that contains the broadcasted hashtag
        /// </summary>
        public void BroadcastHashtag(string hashtag)
        {
            Dialogue[] crewDialogue = GetComponentsInChildren<Dialogue>();
            foreach (Dialogue d in crewDialogue) d.PassHashtag(hashtag);
        }


        /// <summary>
        /// Has the officer of the given skill push an echo of the given hashtag.
        /// </summary>
        public void PushHashtagToOfficer(string hashtag, ShipModule type)
        {
            //Find the officer for the selected skill
            Officer selectedOfficer = GetOfficer(type);

            //If that officer isn't there, end the function
            if (selectedOfficer == null) return;

            //If the officer is there, push the hashtag to him
            selectedOfficer.PushEcho(hashtag);
        }

        /// <summary>
        /// Kills a character with the given character info.
        /// </summary>
        public void KillCrew(CharacterInfo ch, bool leaveGravestone = true)
        {
            foreach (Character character in AllCharactersOnBoard())
            {
                if (character.characterInfo == ch)
                {
                    character.Die(leaveGravestone);
                    return;
                }
            }
        }
        
        

        /// <summary>
        /// Kills a character that is the given instance.
        /// </summary>
        public void KillCrew(Character crew, bool leaveGravestone = true)
        {
            crew.Die(leaveGravestone);
        }
    }
}