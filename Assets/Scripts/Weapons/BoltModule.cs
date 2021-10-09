using UnityEngine;
using System.Collections;
using SpiderWeb;

namespace Diluvion.Ships
{

    [CreateAssetMenu(fileName = "bolt module", menuName = "Diluvion/subs/modules/bolt")]
    public class BoltModule : WeaponModule
    {

        public override void UpdateSystem(WeaponSystem ws)
        {
            base.UpdateSystem(ws);

            // Aim the turrets!
            foreach (WeaponPart p in ws.weaponParts)
            {
                TurretRotator t = p as TurretRotator;
                if (t) t.AimAt(AimPos(ws, t));
            }

            // If not firing, no further action needed!
            if (ws.NotFiring()) return;

            // Fire weapons!
            ws.FireNextMount();
        }

        public override bool ValidMount(Mount m)
        {
            return m.FireReady();
        }

        public override GameObject FireWeapon(Mount m, WeaponSystem ws)
        {
            if (!TrySpendAmmo(ws)) return null;
            return base.FireWeapon(m, ws);
        }

        /// <summary>
        /// Returns the position that the given turrety rotator should aim at to lead the weapon system's target.
        /// </summary>
        /// <param name="ws">The weapon system the turret belongs to</param>
        /// <param name="t">The turret rotator aiming</param>
        Vector3 AimPos(WeaponSystem ws, TurretRotator t)
        {
            if (!ws.autoLead) return ws.TotalAimPosition();

            float s = ws.BulletSpeed();
            Vector3 v1 = ws.Velocity();
            Vector3 tPos = ws.TotalAimPosition();
            Vector3 v2 = ws.GetTarget().Velocity();

            return Calc.FirstOrderIntercept(t.transform.position, v1, s, tPos, v2);
        }
    }
}