using UnityEngine;
using System.Collections.Generic;
using SpiderWeb;
using Diluvion;
using Diluvion.Ships;
using NodeCanvas.BehaviourTrees;
using Sirenix.OdinInspector;
using UnityEngine.Assertions.Comparers;

namespace Loot
{
    [CreateAssetMenu(fileName = "new weapon item", menuName = "Diluvion/items/weapon item")]
    public class DItemWeapon : DItem
    {
        [BoxGroup("Weapon", order: -800)]
        public Sprite weaponIcon;
        [BoxGroup("Weapon", order: -800)]
        [OnValueChanged("Danger"), InlineEditor()]
        public Cannon weapon;

        [SerializeField]
        [BoxGroup("Weapon", order: -800)]
        private int danger = 0;
        
        [BoxGroup("Weapon", order: -800)]
        [Button]
        void EquipToShip()
        {
            if (!Application.isPlaying) return;

            Bridge playerBridge = PlayerManager.pBridge;

            if (!playerBridge) return;
            
            AddToShip();
    
            List<WeaponSystem> playerWeaps = new List<WeaponSystem>();
            playerWeaps.AddRange(playerBridge.GetComponents<WeaponSystem>());

            foreach (var weapon in playerWeaps)
            {
                if (weapon.module.allowedWeapons.Contains(this))
                {
                    weapon.EquipWeapon(this);
                    return;
                }
            }

        }
        
        public override int Danger()
        {
            if (weapon)
                return danger = weapon.CannonDanger();
            else
                return danger;

        }
        
    }
}