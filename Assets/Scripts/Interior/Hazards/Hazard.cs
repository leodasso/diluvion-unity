using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.Roll;
using Loot;
using Sirenix.OdinInspector;
using SpiderWeb;
using DUI;

namespace Diluvion
{
    /// <summary>
    /// Hazards can appear while searching salvages for treasure. This will bring up a battle between the boarding party 
    /// and the hazard!
    /// <see cref="HazardContainer"/><see cref="HazardReaction"/>
    /// </summary>
    [CreateAssetMenu(fileName = "new hazard", menuName = "Diluvion/Hazard")]
    public class Hazard : Entry
    {
        
        #region declare 

        [Range(1, 25)]
        [BoxGroup("testing"), OnValueChanged("TestLevelChanged")]
        public int testLevel = 1;

        void TestLevelChanged()
        {
            testLevel = Mathf.RoundToInt(Mathf.Clamp(testLevel, minMaxLevel.x, minMaxLevel.y));
            previewHp = HpForLevel(testLevel);
            previewDanger = DangerValue(testLevel);
        }

        [BoxGroup("testing"), ReadOnly]
        public int previewHp;

        [BoxGroup("testing"), ReadOnly]
        public float previewDanger;

        [Button, BoxGroup("testing")]
        void TestBattle()
        {
            if (!Application.isPlaying) return;
            HazardContainer testHazard = ApplyToObject(new GameObject("test hazard " + name), testLevel);
            testHazard.Appear();
        }
        
        [TabGroup("haz", "Hazard")]
        public string displayName = "giant crab";

        [TabGroup("haz", "Hazard")]
        [Tooltip("Level determines the strength of the defenses for the different reactions")]
        [MinMaxSlider(1, 25), OnValueChanged("RoundLevels")]
        public Vector2 minMaxLevel = new Vector2(1, 5);

        [TabGroup("haz", "Hazard")]
        public float baseDanger = 20;
        
        [TabGroup("haz", "Hazard")]
        public int firstTurn = 1;
        [TabGroup("haz", "Hazard")]
        public int turnInterval = 3;

        void RoundLevels()
        {
            minMaxLevel = new Vector2(Mathf.RoundToInt(minMaxLevel.x), Mathf.RoundToInt(minMaxLevel.y));
            minMaxHp = new Vector2(Mathf.CeilToInt(minMaxHp.x), Mathf.CeilToInt(minMaxHp.y));
        }
        
        [TabGroup("haz", "Hazard")] [MinMaxSlider(1, 30),  OnValueChanged("RoundLevels")] 
        public Vector2 minMaxHp;

        [TabGroup("haz", "Hazard"), InlineEditor(InlineEditorModes.LargePreview, Expanded = true, PreviewHeight = 100)]
        public Sprite sprite;

        [TabGroup("haz", "Hazard")] 
        [AssetsOnly] public InvGenerator reward;
        
        [TabGroup("haz", "Hazard"), ToggleLeft]
        [Tooltip("Instead of attacking crew members at random, chooses one and focuses on it until the battle ends or the subject dies.")]
        public bool attackSubjectOnly;

        [TabGroup("haz", "Hazard"), ShowIf("attackSubjectOnly"), MultiLineProperty, HideLabel, Title("Subject selection")]
        [Tooltip("Displays a log with this when the hazard chooses a new subject.")]
        public string onNewSubject;

        [TabGroup("haz", "Hazard"), HideLabel, Title("Introduction"), MultiLineProperty]
        public string introduction = "big n scary!";

        [MultiLineProperty, TabGroup("haz", "Hazard"), Title("Attack"), HideLabel]
        public string attack = "Snips the hull!";

        [TabGroup("haz", "Hazard"), AssetsOnly]
        public HazardAction attackAction;

        [MultiLineProperty, TabGroup("haz", "Hazard"), Title("Defeat"), HideLabel]
        public string defeat = "Scurries off!";

        [TabGroup("haz", "Hazard"), AssetsOnly]
        [OnValueChanged("RefreshDescription", true), InlineEditor(Expanded = false)]
        public List<HazardReaction> reactions = new List<HazardReaction>();

        [TabGroup("haz", "Hazard Overview"), ReadOnly, HideLabel, MultiLineProperty(30)]
        public string reactionsDescription = "lol";

        const string prefix = "hazard_";

        [TabGroup("haz", "Audio")]
        public string introSound;
        
        [TabGroup("haz", "Audio")]
        public string hitSound;
        
        [TabGroup("haz", "Audio")]
        public string missSound;
        
        #endregion

        #region localization
        [ButtonGroup("Loc")]
        void AddToLoc ()
        {
            AddLoc();
        }

        [ButtonGroup("Loc")]
        void Refresh ()
        {
            SetLoc();
        }

        void AddLoc ()
        {
            Localization.AddToKeyLib(prefix + name, displayName);
            Localization.AddToKeyLib(prefix + name + "_intro", introduction);
            Localization.AddToKeyLib(prefix + name + "_attack", attack);
            Localization.AddToKeyLib(prefix + name + "_defeat", defeat);
            
            if (attackSubjectOnly)
                Localization.AddToKeyLib(prefix + name + "_newSubject", onNewSubject);
        }

        void SetLoc ()
        {
            displayName = LocName();
            attack = LocAttack();
            defeat = LocDefeat();
            introduction = LocIntro();
            onNewSubject = LocNewSubject();
        }

        public string LocIntro ()
        {
            return Localization.GetFromLocLibrary(prefix + name + "_intro", "[" + introduction + "]");
        }

        public string LocName ()
        {
            return Localization.GetFromLocLibrary(prefix + name, "[" + name + "]");
        }

        public string LocAttack ()
        {
            return Localization.GetFromLocLibrary(prefix + name + "_attack", "[" + attack + "]");
        }

        public string LocDefeat ()
        {
            return Localization.GetFromLocLibrary(prefix + name + "_defeat", "[" + defeat + "]");
        }

        public string LocNewSubject()
        {
            return Localization.GetFromLocLibrary(prefix + name + "_newSubject", "[" + attack + "]");
        }
        #endregion

        /// <summary>
        /// Returns the music for a hazard of the given danger level
        /// </summary>
        public static AdventureDifficulty MusicForDanger(float danger)
        {
            if (danger < GameManager.Get().musicGlobals.tierOneBattle)
                return AdventureDifficulty.Small;
            
            if (danger <  GameManager.Get().musicGlobals.tierTwoBattle)
                return AdventureDifficulty.Medium;
            
            return AdventureDifficulty.Heavy;
        }

        [PropertyOrder(1)]
        [Button("Refresh"), TabGroup("haz", "Hazard Overview")]
        void RefreshDescription ()
        {
            reactionsDescription = "";

            foreach (HazardReaction r in reactions)
            {
                if (!r) continue;
                //if (r.reactionType == HazardReaction.ReactionType.stat && weakness == r.stat)
                //    reactionsDescription += "WEAKNESS \n";

                string defString = "[" + Defense(r.defense, minMaxLevel.x) + " - "
                                   + Defense(r.defense, minMaxLevel.y) + "] \n";
                
                reactionsDescription += string.Format(r.ToString(), defString);
            }
        }

        /// <summary>
        /// Returns the reaction to the given attack
        /// </summary>
        public HazardReaction HitAttemptReaction (CrewStatValue statValue, string attacker)
        {
            return Reaction(statValue.statBase);
        }


        /// <summary>
        /// Returns a value between 0 and 1 to determine the chance of successful attack.
        /// </summary>
        public float ChanceOfSuccess (CrewStatValue attack, int hazardLevel)
        {
            float attackValue = Mathf.Clamp(attack.value, 0, 999);
            float defendValue = .1f;

            foreach (HazardReaction r in reactions)
            {
                if (r.stat == attack.statBase)
                {
                    defendValue += Defense(r.defense, hazardLevel);
                    break;
                }
            }

            //float output = 1 - defendValue / (defendValue + attackValue);
            float output = Calc.LogBase(attackValue, defendValue, 1);
            return output;
        }

        /// <summary>
        /// Is this hazard vulnerable to the given item? If so, returns the specific reaction it has.
        /// </summary>
        public bool VulnerableToItem(DItem item, out HazardReaction reaction)
        {
            reaction = null;

            foreach (var r in reactions)
            {
                if (r.reactionType != HazardReaction.ReactionType.item) continue;
                if (!r.items.Contains(item)) continue;
                reaction = r;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the amount of damage the hazard will take from an attack of the given crew stat value
        /// </summary>
        public int DamageFromStat(CrewStatValue attack)
        {
            foreach (HazardReaction r in reactions)
            {
                if (r.stat == attack.statBase)
                {
                    return r.damage;
                }
            }
            return 0;
        }

        public float Defense(float baseDefense, float level)
        {
            return baseDefense * (level / 2);
        }

        public static int RewardAmount(int level)
        {
            return Mathf.RoundToInt(
                GameManager.Mode().baseHazardReward * Mathf.Pow(level, GameManager.Mode().hazardRewardPower));
        }

        /// <summary>
        /// Returns this hazard's reaction to the given stat action.
        /// </summary>
        public HazardReaction Reaction (CrewStatObject stat)
        {
            foreach (HazardReaction h in reactions)
            {
                if (h.reactionType != HazardReaction.ReactionType.stat) continue;
                if (h.stat == stat) return h;
            }

            return null;
        }

        /// <summary>
        /// The sailor that the hazard has chosen to attack this round. Not used for all attacks.
        /// </summary>
        public static Sailor sailorToAttack;

        /// <summary>
        /// For hazards that latch onto and attack one character, that character is stored here.
        /// </summary>
        public static Sailor subject;

        /// <summary>
        /// The hazard container instance that most recently tried to do its attack action
        /// </summary>
        static HazardContainer _actingInstance;

        /// <summary>
        /// Attempts to do my attack action. Takes success chance of action into account.
        /// </summary>
        public void TryAttack (HazardContainer instance)
        {
            // choose a random character from the player's boarding party - memorized as a static so that actions can
            // access it if they need to.
            sailorToAttack = PlayerManager.RandomBoardingPartyMember();
            
            // Set the acting hazard instance so it can be referenced in the attack outcome
            _actingInstance = instance;
            
            // Log the attempt - and when the attempt is done showing, log the outcome
            BattleLog attemptLog = new BattleLog(attackAction.LocAttempt(), hazard: LocName());
            attemptLog.onEnd += AttackOutcome;
            BattlePanel.Log(attemptLog);
        }

        void AttackOutcome()
        {
            if (_actingInstance == null)
            {
                Debug.LogError(name + " attempted to do an attack, but the acting instance was null!", this);
                return;
            }
            
            if (attackAction.RollSucceeded(_actingInstance))
                Attack();
            else
                Miss();
        }

        void Attack()
        {
            BattlePanel.PlaySound(hitSound);
            attackAction.DoAttack(this);
        }

        void Miss()
        {
            
            BattlePanel.PlaySound(missSound);
            // Log the miss
            BattleLog newLog = new BattleLog(attackAction.LocFail(), hazard: LocName());
            newLog.onEnd += BattlePanel.Iterate;
            BattlePanel.Log(newLog);
        }

        /// <summary>
        /// Chooses a new main subject to attack, and logs that it's attacking this new subject.
        /// </summary>
        public void FocusOnNewSubject()
        {
            ChooseAttackSubject();
            
            // Log the attempt - and when the attempt is done showing, log the outcome
            BattleLog subjectLog = new BattleLog(LocNewSubject(), hazard: LocName());
            subjectLog.onEnd += BattlePanel.Iterate;
            BattlePanel.Log(subjectLog);
        }

        /// <summary>
        /// Chooses a single random member from boarding party. This can be optionally used by certain actions
        /// </summary>
        public static void ChooseAttackSubject()
        {
            subject = PlayerManager.RandomBoardingPartyMember();
        }
        
        /// <summary>
        /// Returns this hazard's reaction to the given stat action.
        /// </summary>
        public string Reaction (DItem item)
        {
            string r = "...";

            foreach (HazardReaction h in reactions)
                if (h.items.Contains(item)) return h.LocAttempt();

            return r;
        }

        public void Die (string attacker)
        {
            BattlePanel.Stop();
            
            BattleLog defeatLog = new BattleLog(LocDefeat(), attacker, LocName());
            defeatLog.onEnd += BattlePanel.Victory;

            BattlePanel.Log(defeatLog);
        }

        /// <summary>
        /// Adds a hazard container to the given object which references to this hazard.
        /// </summary>
        public HazardContainer ApplyToObject (GameObject GO, int level)
        {
            HazardContainer c = GO.AddComponent<HazardContainer>();
            c.hazard = this;
            c.instanceLevel = level;
            c.currentHP = HpForLevel(level);
            
            return c;
        }

        public int HpForLevel(int level)
        {
            return Mathf.RoundToInt(StatForLevel(minMaxHp.x, minMaxHp.y, level));
        }
        
        /// <summary>
        /// Returns a value based on the given min and max that correlates with the given level
        /// </summary>
        /// <param name="minStat">The minimum value of the stat</param>
        /// <param name="maxStat">The max level of the stat</param>
        /// <param name="level">The level of the hazard instance</param>
        public float  StatForLevel(float minStat, float maxStat, int level)
        {
            return StatForRange(minStat, maxStat, minMaxLevel.x, minMaxLevel.y, level);
        }

        public static float StatForRange(float minStat, float maxStat, float minLevel, float maxLevel, int level)
        {
            // get the range of stat
            float statRange = maxStat - minStat; 
            
            // get the range of possible levels
            float levelRange = maxLevel - minLevel; 
            
            // form a ratio based on the two ranges
            float levelToStat =  statRange /  levelRange;

            float levelRemainder = level - minLevel;

            float additionalStat = levelRemainder * levelToStat;
            return minStat + additionalStat;
        }

        /// <summary>
        /// Represents the danger of the hazard at it's minimum level
        /// </summary>
        public override int Value ()
        {
            return DangerValue((int)minMaxLevel.x);
        }

        /// <summary>
        /// Represents the danger of the hazard at the given level
        /// </summary>
        public int DangerValue(int level)
        {
            level = Mathf.Clamp(level, (int)minMaxLevel.x, (int)minMaxLevel.y);
            float adjustedLevel = Mathf.Pow(level, GameManager.Mode().hazardDangerPower);

            float danger = baseDanger * adjustedLevel;
            return Mathf.RoundToInt(danger * GameManager.Mode().hazardDangerCostMultiplier);
        }

        /// <summary>
        /// Returns the maximum level that this hazard can be while still being within the given danger value.
        /// </summary>
        public int MaxLevelForDanger(float dangerValue)
        {
            // start at the highest possible level
            int maxLevel = Mathf.RoundToInt(minMaxLevel.y);

            while (true)
            {
                // Get the danger at that level
                float dangerOutput = DangerValue(maxLevel);

                // If the danger for the selected level is less than the given constraint, return it!
                if (dangerOutput <= dangerValue || maxLevel <= Mathf.RoundToInt(minMaxLevel.x)) 
                    return maxLevel;

                maxLevel--;
            }
        }

        /// <summary>
        /// Returns all the items that can be used against this hazard
        /// </summary>
        public List<DItem> GetItemUsages()
        {
            List<DItem> returnList = new List<DItem>();
            
            foreach (var r in reactions)
            {
                if (r.reactionType == HazardReaction.ReactionType.item)
                {
                    returnList.AddRange(r.items);
                }
            }

            return returnList;
        }

        [Button]
        void LogValue()
        {
            Debug.Log("Danger value: " + Value());
            Debug.Log("Global multiplier: " + GameManager.Mode().hazardDangerCostMultiplier);
        }
    }
}