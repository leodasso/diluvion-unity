using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

namespace Diluvion
{

    public class BoltCannon : Cannon
    {

        public int burstSize = 1;
        public float burstCooldown = .1f;
        
        [Button()]
        public override GameObject Fire()
        {
            StartCoroutine(BurstFire());
            return null;
        }


        public override int CannonDanger()
        {
            //Debug.Log("dps: " + DamagePerSecond*2+ " * burst: " + BurstSizeDanger+ " * crit: " + CritDanger + " * range: "+ RangeDanger + " * speed: " + ShotSpeedDanger + " / recoil: " + RecoilDanger+" * burstCD: " + BurstCDDanger);
            return danger =  Mathf.RoundToInt( (DamagePerSecond*2*BurstSizeDanger * CritDanger* RangeDanger * ShotSpeedDanger)/RecoilDanger*BurstCDDanger);
        }

        private float BurstCDDanger => 1 + burstCooldown;
        private float BurstSizeDanger => burstSize*1.0f / 2.0f;


        IEnumerator BurstFire()
        {
            int i = 0;
            while (i < burstSize)
            {
                base.Fire();
                yield return new WaitForSeconds(burstCooldown);
                i++;
            }
            yield break;
        }
    }
}