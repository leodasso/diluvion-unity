using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Diluvion
{

    /// <summary>
    /// Places a character in the spot with given pose.  Can have a specific character prefab set,
    /// or choose randomly on start from a list of characters in the scene object.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterPlacer : MonoBehaviour, IRewardable
    {

        #region delcare
        public enum CharacterSpawnType
        {
            any,
            onPlayerCrew,
            offPlayerCrew
        }

        [OnValueChanged("RefreshRandom"), FoldoutGroup("Character"), DisableIf("createRandomSailor")]
        [AssetList(Path = "Prefabs/"), AssetsOnly]
        public CharacterInfo character;

        [Tooltip("If true, this placer can be used to procedurally generate characters as 'rewards' in looting sequences. " +
                 "Takes 'chance of appearance' into account. This should be disabled for explicit placements.")]
        [FoldoutGroup("Character")] public bool allowRewardSpawn = true;
        
        [Space, Title("Spawning"), Range(0, 1), Tooltip("Chance of spawning, from 0 (0%) to 1 (100%)"), ShowIf("NeedsChance")]
        public float chanceOfAppearance = 1;

        public bool NeedsChance()
        {
            return character || allowRewardSpawn;
        }
        
        [ShowIf("CanSpawnOnStart")]
        public bool spawnOnStart = true;

        bool CanSpawnOnStart()
        {
            return character || createRandomSailor;
        }

        [Tooltip("Specify when this character should & shouddn't spawn, regarding their status on the player's crew.")]
        [LabelText("spawn allowance")]
        public CharacterSpawnType spawnWhen = CharacterSpawnType.any;

        [HorizontalGroup("pose"), ToggleLeft]
        public bool applyPose = true;

        [HorizontalGroup("pose")]
        [ShowIf("applyPose"), OnValueChanged("RefreshSprite"), HideLabel]
        public Poses myPose;

        [FoldoutGroup("Character")]
        [OnValueChanged("RefreshPrefab")]
        [DisableIf("allowRewardSpawn")]
        public bool createRandomSailor;

        [FoldoutGroup("Character")]
        [DisableIf("allowRewardSpawn")]
        public bool applyRandomStats;

        [FoldoutGroup("Character")]
        [EnableIf("applyRandomStats")]
        public int minStatPoints = 6;

        [FoldoutGroup("Character")]
        [EnableIf("applyRandomStats")]
        [DisableIf("allowRewardSpawn")]
        public int maxStatPoints = 10;

        [FoldoutGroup("Character")]
        [DisableIf("allowRewardSpawn")]
        public bool applyRandomName;


        [FoldoutGroup("more options", false), AssetsOnly]
        public List<Convo> conversationsToAdd = new List<Convo>();

        [FoldoutGroup("more options")]
        [Tooltip("If the character spawned from here has a conversation that requires other characters" +
            "to be moved near to them, this will have the nearby placers spawn in their characters.")]
        public List<CharacterPlacer> neighbors;
        
        [FoldoutGroup("more options"), ToggleLeft]
        public bool overrideEngineerLevel;

        [FoldoutGroup("more options"), ShowIf("overrideEngineerLevel")]
        public int newEngineerLevel = 1;
        
        [FoldoutGroup("more options")]
        public bool overrideHireCost;
        
        [FoldoutGroup("more options")]
        [ShowIf("overrideHireCost")]
        public int newHireCost = 50;

        [ReadOnly, PropertyOrder(900)]
        public Character characterInstance;
        
        SpriteRenderer _spriteRenderer;
        bool _init;

        void RefreshSprite ()
        {
            if (!applyPose) return;

            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = CharactersGlobal.SpriteForPose(myPose);
        }

        void RefreshRandom ()
        {
            if (character != null)
            {
                createRandomSailor = false;
            }
        }

        void RefreshPrefab ()
        {
            if (createRandomSailor)
            {
                applyRandomStats = true;
                character = null;
            }
        }

        #endregion

        // Use this for initialization
        void Start ()
        {
            //turn off my sprite renderer
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer) _spriteRenderer.enabled = false;
            gameObject.tag = "NoRender";

            Init();
        }
        
        #region iRewardable 
        /// <summary>
        /// Attempts to create a random sailor with the given reward points
        /// </summary>
        public float MakeReward (float rewardPoints)
        {
            //Debug.Log("Placer " + name + " was selected to try and create a reward with " + 
            //          rewardPoints.ToString("0") + " points...", gameObject);

            if (character != null)
            {
                Debug.Log("...it's set to place a pre-made character, and isn't appropriate to create a reward (random sailor)");
                return rewardPoints;
            }

            if (!SuccessfulRoll())
            {
                //Debug.Log("...it didn't roll successfully.");
                return rewardPoints;
            }

            int points = Mathf.FloorToInt(rewardPoints / GameManager.Mode().costPerSailorPoint);
            
            // Check if this is enough points to create a viable sailor. If not, return the gold.
            if (points < GameManager.Mode().minPointsForSailor)
            {
                //Debug.Log("The value of " + rewardPoints.ToString("0") + " creates " + points + " statpoints, which isn't sufficient to " +
                //          "create a sailor. The minimum sailor points can be changed in the game mode.");
                return rewardPoints;
            }
            
            CreateRandomSailor(points);

            float costOfSailor = points * GameManager.Mode().costPerSailorPoint;
            return rewardPoints - costOfSailor;
        }

        public float PopulatePriority()
        {
            return 1;
        }

        public void DisableIfEmpty()
        {
            if (!characterInstance) Disable();
        }

        public void Disable ()
        {
            gameObject.SetActive(false);
        }
        #endregion


        void OnEnable ()
        {
            if (characterInstance != null && characterInstance.gameObject.activeInHierarchy)
                FinalizePlacement(characterInstance);
        }

        public void Init ()
        {
            if (_init) return;
            _init = true;
            
            if (!spawnOnStart) return;

            //Roll to see if character will appear
            if (!SuccessfulRoll()) return;
            
            
            //If I already have a character prefab set, instantiate it
            if (character)
            {
                StartCoroutine(MakeCharacter());
                return;
            }

            // If create random sailor is toggled, do it
            int statPoints = Random.Range(minStatPoints, maxStatPoints + 1);
            if (createRandomSailor) CreateRandomSailor(statPoints);
            
        }

        /// <summary>
        /// Roll to see if the character will appear based on chance of appearance.
        /// <see cref="chanceOfAppearance"/>
        /// </summary>
        bool SuccessfulRoll ()
        {
            float diceRoll = Random.Range(0f, 1f);
            return diceRoll < chanceOfAppearance;
        }

        /// <summary>
        /// Name this object to represent what it will spawn.
        /// </summary>
        [Button("Name Game Object")]
        public void NameGO ()
        {
            if (character != null) gameObject.name = "char placer: " + character.name;
            else if (createRandomSailor) gameObject.name = "char placer: random sailor";
            else gameObject.name = "char placer: empty";
        }

        /// <summary>
        /// Creates a random sailor
        /// </summary>
        /// <param name="totalPoints">amount of points to allocate to stats</param>
        void CreateRandomSailor (int totalPoints)
        {
            // Create a random instance of character info
            Sailor newRandom = Sailor.InstantiateSailor();
            newRandom.Randomize(totalPoints);
            characterInstance = newRandom;
            FinalizePlacement(characterInstance);
        }

        /// <summary>
        /// Called when the character instance I spawned in is talked to by the player.
        /// </summary>
        void MyCharacterTalkedTo ()
        {
            // Make sure all the neighbors required for this conversation are in place
            foreach (CharacterPlacer cp in neighbors)
                cp.MoveCrewHere();
        }

        Character InstantiateCharacter (CharacterInfo charInfo)
        {
            characterInstance = charInfo.InstantiateCharacter();
            characterInstance.transform.position = transform.position;
            characterInstance.transform.rotation = transform.rotation;
            characterInstance.transform.SetParent(transform.parent, true);
            return characterInstance;
        }


        IEnumerator MakeCharacter ()
        {
            while (!PlayerManager.PlayerCrew()) yield return null;

            // only spawn a character when they're not on the player crew
            if (spawnWhen == CharacterSpawnType.offPlayerCrew)
                if (PlayerManager.PlayerCrew().HasCharacter(character)) yield break;


            //Check if the prefab character is on the player crew
            if (spawnWhen == CharacterSpawnType.onPlayerCrew)
            {
                // While they're not on the player's crew, check every second to see if they got added.
                while (!PlayerManager.PlayerCrew().HasCharacter(character))
                    yield return new WaitForSeconds(1);
            }

            //Instantiate the character
            InstantiateCharacter(character);

            characterInstance.onTalkTo += MyCharacterTalkedTo;
            characterInstance.onHired += Hired;

            // Set a random name of this character, even if it's from a prefab
            if (applyRandomName)
            {
                Sailor s = characterInstance as Sailor;
                if (s) s.generatedName = CharactersGlobal.RandomName(s.gender);
            }

            FinalizePlacement(characterInstance);
        }


        /// <summary>
        /// Moves my character prefab to this location if they're already in the same interior.
        /// if not, will create an instance
        /// </summary>
        public void MoveCrewHere ()
        {

            InteriorManager interior = GetComponentInParent<InteriorManager>();
            if (interior == null)
            {
                Debug.Log("Couldn't find the interior manager.", gameObject);
                return;
            }

            // Check if this interior has my character prefab already
            Character otherInstance = interior.GetInhabitant(character);

            // If the character is already in this interior, just move them
            if (otherInstance != null) FinalizePlacement(otherInstance);

            // Otherwise instantiate them here
            else StartCoroutine(MakeCharacter());
        }


        void Hired ()
        {
            characterInstance = null;
        }

        void ApplyRandomStats(Character instance)
        {
            int statPoints = Random.Range(minStatPoints, maxStatPoints + 1);
            Sailor s = instance as Sailor;
            if (s) s.SetRandomCrewStats(statPoints);
        }

        /// <summary>
        /// Places character object in correct place in hierarchy, sets correct
        /// layer / sorting order for the sprite, and sets correct animation pose
        /// </summary>
        public void FinalizePlacement (Character spawnedCrew)
        {
            if (!_init) Init();

            if (spawnedCrew == null)
            {
                Debug.Log("Trying to finalize placement of character, but no crew instance exists.");
                return;
            }

            // Give random stats
            if (applyRandomStats) ApplyRandomStats(spawnedCrew);


            //set character instance (to catch when this function is called from outside)
            characterInstance = spawnedCrew;
            PlaceHere(characterInstance);

            if (conversationsToAdd.Count > 0)
                characterInstance.onDialogCreated += ManualAddConvos;

            // Set new engineer level
            ShipBroker broker = characterInstance.GetComponent<ShipBroker>();
            if (broker != null)
                if (overrideEngineerLevel) broker.engineerLevel = newEngineerLevel;

            if (overrideHireCost && spawnedCrew is Sailor)
            {
                Sailor s = spawnedCrew as Sailor;
                s.costToHire = newHireCost;
            }
        }

        /// <summary>
        /// Places the given character in this spot, but without doing any extra actions.
        /// </summary>
        public void PlaceHere (Character ch)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            //set character instance (to catch when this function is called from outside)
            ch.transform.SetParent(transform, true);
            ch.transform.localPosition = ch.transform.localEulerAngles = Vector3.zero;
            ch.transform.localScale = Vector3.one;

            //Pass info from the placer sprite to the character sprite (i.e. layer, order)
            SpriteRenderer characterSprite = ch.GetComponent<SpriteRenderer>();

            if (characterSprite)
            {
                characterSprite.sortingLayerID = _spriteRenderer.sortingLayerID;
                characterSprite.sortingOrder = _spriteRenderer.sortingOrder;
            }

            // animation info pass
            if (applyPose)
            {
                //Debug.Log("Applying pose " + myPose.ToString() + " to character ", ch.gameObject);
                string animationName = myPose.ToString();
                ch.SetAnimation(animationName, 0, true);
            }
        }

        void ManualAddConvos (Dialogue d)
        {
            d.manualAdded.AddRange(conversationsToAdd);
        }

        public bool Used ()
        {
            if (characterInstance == null) return false;
            return true;
        }
    }


    public enum Poses
    {
        stand,
        sleep,
        sitGround,
        sitEdge,
        sitDesk,
        navSailor,
        gunSailor,
        sonarSailor,
        invisible,
        injured,
        officer
    }
}