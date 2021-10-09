using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DUI;
using Loot;
using SpiderWeb;

namespace Diluvion
{
    /// <summary>
    /// Contains a reference to the hazard and an instance 'HP'
    /// <see cref="Hazard"/>
    /// </summary>
    public class HazardContainer : MonoBehaviour
    {
        public Hazard hazard;
        public int currentHP;
        public int instanceLevel = 1;

        /// <summary>
        /// memorize the most recent attack
        /// </summary>
        CrewStatValue _cachedAttack;

        /// <summary>
        /// memorize the name of the most recent attacker
        /// </summary>
        string _cachedAttacker;

        /// <summary>
        /// memorize the cached reaction to most recent attack
        /// </summary>
        HazardReaction _cachedReaction;

        public bool firstTurnConsumed;

        public int lastTurn;

        /// <summary>
        /// Takes the given hit, attempts to dodge it. If not dodged, takes damage.
        /// </summary>
        /// <param name="attack">The crew stat used for attacking</param>
        /// <param name="attacker">The localized name of the attacker</param>
        public void TakeHit(CrewStatValue attack, string attacker)
        {
            // Memorize the stuff of the attack
            _cachedAttack = attack;
            _cachedAttacker = attacker;

            // Get the hazard's reaction to the hit
            _cachedReaction = hazard.HitAttemptReaction(attack, attacker);

            // Log that the attempt happened.
            BattleLog hitAttempt = new BattleLog(_cachedReaction.LocAttempt(), attacker, hazard.LocName(), 3);

            BattlePanel.Shake(1, .5f);

            // When the hit attempt is done, show the result
            hitAttempt.onEnd += HitResult;

            BattlePanel.Log(hitAttempt);
        }

        public void TakeItemHit(DItem item, string attacker)
        {
            HazardReaction reaction;
            if (hazard.VulnerableToItem(item, out reaction))
            {
                _cachedAttacker = attacker;
                BattlePanel.Shake(1, .5f);
                
                // subtract the damage of the attack from HP
                currentHP -= reaction.damage;

                // Log that the attack was successful
                BattleLog newLog = new BattleLog(reaction.LocPass(), _cachedAttacker, hazard.LocName());
                newLog.onEnd += BattlePanel.Iterate;
                BattlePanel.Log(newLog);
            }
        }

        /// <summary>
        /// Is the turn at the given index my turn?
        /// </summary>
        public bool IsItMyTurn(int turnIndex)
        {
            int turnsSinceLast = turnIndex - lastTurn;

            if (!firstTurnConsumed)
                return turnIndex == hazard.firstTurn;

            return turnsSinceLast == hazard.turnInterval;
        }

        /// <summary>
        /// The result of the cached crew attack on this hazard
        /// </summary>
        void HitResult()
        {
            // Rool the dice!
            float roll = Random.Range(0f, 1.0f);

            // Get the crew attack's chance of success from the hazard
            float chance = hazard.ChanceOfSuccess(_cachedAttack, instanceLevel);

            // If the roll is outside of the chance of success, the attack failed.
            if (roll > chance)
            {
                BattleLog failLog = new BattleLog(_cachedReaction.LocFail(), _cachedAttacker, hazard.LocName());
                failLog.onEnd += BattlePanel.Iterate;
                BattlePanel.Log(failLog);
                return;
            }

            // subtract the damage of the attack from HP
            currentHP -= _cachedReaction.damage;

            // Log that the attack was successful
            BattleLog newLog = new BattleLog(_cachedReaction.LocPass(), _cachedAttacker, hazard.LocName());
            newLog.onEnd += BattlePanel.Iterate;
            BattlePanel.Log(newLog);
            
            SpiderSound.MakeSound("Play_Attack_Success", gameObject);

            BattlePanel.ShakeHazard();

            // If HP runs out, die!
            if (currentHP <= 0) hazard.Die(_cachedAttacker);
        }


        /// <summary>
        /// Creates the battle panel with this hazard!
        /// </summary>
        public void Appear()
        {
            // reset turn info
            lastTurn = 0;
            firstTurnConsumed = false;
            
            // create the battle panel, set it up to do battle with this hazard
            BattlePanel.Create();
            BattlePanel.SetHazard(this);
            
            SpiderSound.MakeSound(hazard.introSound, gameObject);
            
            // Create the battle log that introduces the hazard.
            BattleLog appearanceLog = new BattleLog(hazard.LocIntro(), hazard: hazard.LocName(), t:5);
            BattlePanel.Log(appearanceLog);
            
            // Log the danger amount of the hazard
            Debug.Log(hazard.displayName + " appears. Danger amount: " + hazard.DangerValue(instanceLevel));
            
            // choose the static attack subject
            Hazard.ChooseAttackSubject();
            
            // Have the attack action prep for battle (happens ONCE at the beginning of every battle)
            hazard.attackAction.BattleBeginPrep();
            
            // Get the correct music for my difficulty, and set the music
            AdventureDifficulty diff = Hazard.MusicForDanger(hazard.DangerValue(instanceLevel));
            AKMusic.Get().SetAdventure(diff);
        }

        public int MaxHP()
        {
            if (!hazard) return 0;
            return hazard.HpForLevel(instanceLevel);
        }

        public float NormalizedHP()
        {
            if (!hazard) return 0;
            return currentHP / (float)hazard.HpForLevel(instanceLevel);
        }
    }
}