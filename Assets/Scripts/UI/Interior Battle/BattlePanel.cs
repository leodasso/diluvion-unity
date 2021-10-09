using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion;
using Sirenix.OdinInspector;
using Rewired;
using SpiderWeb;

namespace DUI
{
    /// <summary>
    /// The panel that shows that 'battle' between the player's boarding party and the
    /// hazards they come across.
    /// <see cref="Hazard"/>
    /// </summary>
    public class BattlePanel : DUIPanel
    {

        #region declare
        [TabGroup( "battle", "Setup")]
        public PopupObject forcedRetreatPopup;
        
        [TabGroup( "battle", "Setup")]
        public float shakeAmount;

        [TabGroup( "battle", "Setup")]
        public Animator hazardAnimator;

        [TabGroup("battle", "Setup")] 
        public CanvasGroup retreatButton;

        [TabGroup("battle", "Setup")] 
        public HazardVictoryPanel victoryPanelPrefab;

        [TabGroup( "battle", "Setup")]
        public List<RectTransform> turnSlots = new List<RectTransform>();

        [TabGroup("battle", "Setup")] 
        public RectTransform standbySlot;

        [TabGroup( "battle", "Setup")]
        public CharacterBox crewBoxPrefab;

        [TabGroup("battle", "Setup")] 
        public HazardSlotPanel hazardSlotPrefab;

        [TabGroup( "battle", "Setup")]
        public HazardPanel hazardPanel;

        [TabGroup( "battle", "Setup")]
        public TalkyText battleLog;

        [TabGroup("battle", "Setup")] 
        public LocTerm characterIntro;

        [TabGroup("battle", "Setup")] 
        public LocTerm injuredCharacter;

        [TabGroup("battle", "Runtime"), Tooltip("The number of turns that have elapsed in the current battle")] 
        public int turns;

        [TabGroup( "battle", "Runtime")]
        public List<BattleLog> battleLogs = new List<BattleLog>();

        /// <summary>
        /// The boxes that appear in the 'turn order' section
        /// </summary>
        [TabGroup( "battle", "Runtime")]
        public List<DUIPanel> turnBoxInstances;

        [TabGroup( "battle", "Runtime")]
        public HazardContainer hazardInstance;

        [TabGroup( "battle", "Runtime")]
        public Vector2 shakeVector;


        static bool _playing = true;
        static BattlePanel _battlePanel;

        float _logTimer;

        BattleLog _visibleLog;
        int _partyIndex;

        #endregion

        protected override void Start ()
        {
            base.Start();

            if (NeedToRetreat())
            {
                ForcedRetreat();
                return;
            }
            
            // spawn the first turn boxes
            for (int i = 0; i < 5; i++)
            {
                SpawnNextTurnBox();
            }
            
            SetAllCharacterBoxes();
            
            LogActiveCharacter();
        }

        protected override void Update ()
        {
            base.Update();

            if (_logTimer > 0) _logTimer -= Time.unscaledDeltaTime;

            // If there's logs available to show
            if (battleLogs.Count > 0 && _logTimer <= 0)
            {
                // If there's already a log showing
                if (_visibleLog != null) RemoveCurrentLog();

                // show the next log
                if (battleLogs.Count > 0) ShowLog(battleLogs [0]);
            }

            // Skip
            if (GameManager.Player().GetButtonDown("select"))
                _logTimer = 0;
        }

        static bool NeedToRetreat()
        {
            return PlayerManager.BoardingParty().Count < 1;
        }

        void RemoveCurrentLog()
        {
            if (battleLogs.Count < 1)
            {
                Debug.Log("Tried to remove log, but there's no logs remaining.");
                return;
            }

            // do the ending action of the visible log
            battleLogs [0].End();
            _visibleLog = null;

            // remove the visible log log
            battleLogs.RemoveAt(0);
        }

        #region static functions
        public static BattlePanel Create ()
        {
            _playing = true;
            return UIManager.Create(UIManager.Get().battlePanel as BattlePanel);
        }

        public static bool Exists()
        {
            return _battlePanel != null;
        }

        public static BattlePanel Get ()
        {
            if (_battlePanel) return _battlePanel;
            _battlePanel = UIManager.GetPanel<BattlePanel>();
            return _battlePanel;
        }

        /// <summary>
        /// Returns the hazard we're currently battling (if any)
        /// </summary>
        public static Hazard GetHazard()
        {
            if (!Exists()) return null;
            return Get().hazardInstance.hazard;
        }

        /// <summary>
        /// Returns the instance holding the hazard we're currently battling
        /// </summary>
        public static HazardContainer GetHazardContainer()
        {
            if (!Exists()) return null;
            return Get().hazardInstance;
        }

        
        /// <summary>
        /// Stops the battle panel from taking further action
        /// </summary>
        public static void Stop()
        {
            Debug.Log("Battle log is stopping.");
            _playing = false;
        }

        public static void Play()
        {
            _playing = true;
        }

        /// <summary>
        /// Sets up the hazard of this battle to be the one contained in the given hazard container.
        /// </summary>
        public static void SetHazard (HazardContainer h)
        {
            Get().hazardInstance = h;
            Get().hazardPanel.Init(h);
        }

        /// <summary>
        /// Iterates the battle by one step. Next character's turn, enemy's turn, etc
        /// </summary>
        public static void Iterate ()
        {
            if (!Get()) return;
            Get().IterateLocal();
        }

        public static void PlaySound(string audioName)
        {
            if (!Exists()) return;
            Debug.Log("Playing sound " + audioName);
            SpiderSound.MakeSound(audioName, Get().gameObject);
        }


        /// <summary>
        /// Perform an attack on the hazard using the given stat
        /// </summary>
        public static void NewAttack (CrewStatValue attack, Character attacker)
        {
            Get();
            if (!_battlePanel) return;
            if (_battlePanel.hazardInstance == null) return;

            string attackerName = attacker.GetLocalizedName();
            
            SpiderSound.MakeSound("Play_Select_Attack", _battlePanel.gameObject);

            _battlePanel.hazardInstance.TakeHit(attack, attackerName);
            _battlePanel.RemoveActiveCrew();
        }
        
        public static void NewItemUse (Loot.DItem item, Character attacker)
        {
            Get();
            if (!_battlePanel) return;
            if (_battlePanel.hazardInstance == null) return;

            string attackerName = attacker.GetLocalizedName();

            _battlePanel.hazardInstance.TakeItemHit(item, attacker.GetLocalizedName());
            _battlePanel.RemoveActiveCrew();
        }

        public static void ShakeHazard()
        {
            _battlePanel.hazardAnimator.SetTrigger("shake");
        }

        /// <summary>
        /// Log a certain action to the battle panel.
        /// </summary>
        public static void Log (BattleLog log)
        {
            Get();
            if (!_battlePanel) return;

            //Debug.Log("Logging " + log.FullLog());
            _battlePanel.battleLogs.Add(log);
        }

        public static void Shake (float shakeTime = 1, float intensity = 1)
        {
            Get().StartCoroutine(Get().ShakeRoutine(shakeTime, intensity));
        }

        public static void Victory()
        {
            if (Get())
                Get().Success();
        }

        public static void ForcedRetreat()
        {
            if (!Get()) return;

            _playing = false;
            
            // show popup that you were forced to retreat
            Get().forcedRetreatPopup.CreateUI(Get().Flee, Get().Flee);
        }


        #endregion
        
        public void Flee()
        {
            AKMusic.Get().SetNeutralExporable();
            End();
        }
        
        
        /// <summary>
        /// Displays the text for the given log immediately
        /// </summary>
        void ShowLog (BattleLog log)
        {
            _visibleLog = log;
            battleLog.inputText = log.FullLog();
            _logTimer = log.time;

            log.Show();
        }

        void ShowRetreatButton()
        {
            retreatButton.alpha = 1;
            retreatButton.interactable = true;
        }


        void Success ()
        {
            AKMusic.Get().SetNeutralExporable();
            
            // play the 'victory' animation
            HazardVictoryPanel victoryInstance = UIManager.Create(victoryPanelPrefab);
            victoryInstance.Init(hazardInstance.hazard, hazardInstance.instanceLevel);
            
            Room r = hazardInstance.GetComponent<Room>();
            if (r) r.EnterRoom();
            End();
        }
        
        #region turn boxes


        void SpawnNextTurnBox()
        {
            // If there's no crew left, force a retreat
            if (NeedToRetreat())
            {
                ForcedRetreat();
                return;
            }
            
            int index = turnBoxInstances.Count + turns;
            
            // Spawn hazard box
            if (hazardInstance.IsItMyTurn(index))
                SpawnHazardBox(index);
            
            // Spawn a character box
            else SpawnCrewBox();
            
            RefreshPositions();
            
            ShowRetreatButton();
        }
        

        void SpawnCrewBox()
        {
            if (PlayerManager.BoardingParty().Count < 1) return;

            // Get the remainder of turn index / party index. This way the party index is never out of range.
            int i = _partyIndex % PlayerManager.BoardingParty().Count;

            _partyIndex++;
            
            Character c = PlayerManager.BoardingParty()[i];
            Debug.Log("Party index: " + i + " is crewmember " + c.GetLocalizedName());
            
            var newBox = Instantiate(crewBoxPrefab);
            newBox.Init(c);

            turnBoxInstances.Add(newBox);
            
            // Position the new panel
            SetInitPosition(newBox.GetComponent<PanelLerper>());
        }

        void SpawnHazardBox(int turnIndex)
        {
            hazardInstance.firstTurnConsumed = true;
            hazardInstance.lastTurn = turnIndex;

            HazardSlotPanel newHazardBox = Instantiate(hazardSlotPrefab);
            newHazardBox.Init(hazardInstance.hazard);

            turnBoxInstances.Add(newHazardBox);
            
            SetInitPosition(newHazardBox.GetComponent<PanelLerper>());
        }

        void SetInitPosition(PanelLerper panel)
        {
            panel.TeleportTo(standbySlot);
        }

        /// <summary>
        /// Re-positions all the character boxes into their slots.
        /// </summary>
        [ButtonGroup]
        void RefreshPositions ()
        {
            if (!_playing) return;
            
            // For each turn box...
            for (int i = 0; i < turnBoxInstances.Count; i++)
            {
                
                int index = Mathf.Clamp(i, 0, turnSlots.Count - 1);
                
                PanelLerper p = turnBoxInstances[i].GetComponent<PanelLerper>();
                p.nextPanel = turnSlots [index];
            }
        }

        void SetCharacterBox(int index)
        {
            CharacterBox box = turnBoxInstances[index].GetComponent<CharacterBox>();
            if (!box) return;
            
            // The character currently taking action is at index 0
            box.showStats = index == 0;
        }

        void SetAllCharacterBoxes()
        {
            //Debug.Log("Setting all character boxes...");
            
            for (int i = 0; i < turnBoxInstances.Count; i++)
                SetCharacterBox(i);
        }

        public static void LogActiveCharacter()
        {
            if (Get()) 
                Get().LogActiveCharacterLocal();
        }

        void LogActiveCharacterLocal()
        {
            CharacterBox c = turnBoxInstances[0].GetComponent<CharacterBox>();
            if (!c) return;
            
            SpiderSound.MakeSound("Play_Crew_Turn", gameObject);
            
            // Check for an injured character
            Sailor s = c.myCrew as Sailor;
            if (s)
            {
                if (s.IsInjured())
                {
                    BattleLog injuredLog = new BattleLog(injuredCharacter.LocalizedText(), c.myCrew.GetLocalizedName());
                    injuredLog.onEnd += Iterate;
                    Log(injuredLog);
                    return;
                }
            }

            // Log the character's turn
            BattleLog newLog = new BattleLog(characterIntro.LocalizedText(), c.myCrew.GetLocalizedName());
            Log(newLog);
        }
        
        #endregion

        /// <summary>
        /// Shakes the hazard panel
        /// </summary>
        IEnumerator ShakeRoutine (float time, float intensity)
        {
            if (!_playing) yield break;
            
            float t = time;
            RectTransform rect = GetComponent<RectTransform>();

            Vector2 initPos = rect.anchoredPosition;
            Vector2 noise;
                

            while (t > 0)
            {
                noise = Vector2.one * Mathf.PerlinNoise(t * intensity, t * intensity);
                rect.anchoredPosition = initPos + noise; // noise RandomVector(t * intensity);
                t -= Time.unscaledDeltaTime;
                yield return null;
            }

            rect.anchoredPosition = Vector2.zero;
        }

        void IterateLocal ()
        {

            if (NeedToRetreat())
            {
                ForcedRetreat();
                return;
            }
            
            
            if (turnBoxInstances.Count < 1) return;
            if (!_playing) return;

            // Remove the most recent character from the list
            var boxToRemove = turnBoxInstances[0];
            turnBoxInstances.RemoveAt(0);
            boxToRemove.End();
            
            // add a new slot to the end of the list
            SpawnNextTurnBox();

            // Refresh the positions of the crew
            RefreshPositions();
            
            SetAllCharacterBoxes();
            
            // check if the hazard is the active slot. If so, do hazard turn
            if (turnBoxInstances[0].GetComponent<HazardSlotPanel>())
                HazardTurn();
            
            else LogActiveCharacter();

            turns++;
        }

        /// <summary>
        /// Actions that the hazard takes on its turn
        /// </summary>```
        void HazardTurn()
        {    
            SpiderSound.MakeSound("Play_Hazard_Turn", gameObject);
            
            // If the hazard only attacks one subject, check that the subject is still alive.
            if (hazardInstance.hazard.attackSubjectOnly)
            {
                // If the subject is dead, select a new subject. This consumes the turn.
                if (!Hazard.subject || !Hazard.subject.gameObject.activeInHierarchy)
                {
                    Debug.Log("Hazard selecting new subject!");
                    hazardInstance.hazard.FocusOnNewSubject();
                    return;
                }
            }
            hazardInstance.hazard.TryAttack(hazardInstance);
        }

        void RemoveActiveCrew ()
        {
            if (turnBoxInstances.Count > 0) turnBoxInstances [0].End();
        }
    }
}