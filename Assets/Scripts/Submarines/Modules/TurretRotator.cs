using UnityEngine;
using SpiderWeb;
using Sirenix.OdinInspector;

namespace Diluvion
{
    public enum RotationAxis
    {
        xAxis,
        yAxis
    }

    /// <summary>
    /// this script applies a rotation at a set speed, and clamps.  For turrets with rotators
    /// and elevators, this component should be added to each.  
    /// <para>The object this is applied to MUST be 0, 0, 0 local eulers.  If the turret needs to be at an angle,
    /// make it the child of an empty game object and rotate that object.</para>
    /// </summary>
   // [ExecuteInEditMode]
    public class TurretRotator : WeaponPart
    {
        
        [Tooltip("Target to aim at; used for testing.")]
        public Transform target;

        [Space, Tooltip("The rotator should be a child of this object. This script controls the movement of the rotator.")] 
        [ValidateInput("ValidateRotator", "The rotator must be a child of this transform.")]
        public Transform rotator;
        public RotationAxis myRotationAxis;
        [Tooltip("is this turret clamped or does it have full rotation?")]
        public bool clamped;
        
        [ShowIf("clamped")]
        [Range(-180, 0)] public float limitLeft = -180f;
        [ShowIf("clamped")]
        [Range(0, 180)] public float limitRight = 180f;
        
        [ReadOnly, HorizontalGroup("bearing"), LabelText("Bearing & clamped")]
        public float bearing;
        [ReadOnly, HorizontalGroup("bearing"), HideLabel]
        public float clampedBearing;

        public bool debug = false;

        bool ValidateRotator(Transform newRotator)
        {
            if (!newRotator) return false;
            if (newRotator.transform.parent != transform) return false;
            return true;
        }


        /// <summary>
        /// Maximum allowed error in degrees. 
        /// A turret trying to aim at something beyond its rot limit would result in error.
        /// </summary>
        float maxDegreesError = 1;
        float error;

        [Button()]
        void Reset()
        {
            target = null;
            if (rotator)
                rotator.localEulerAngles = Vector3.zero;
        }

        /// <summary>
        /// Is this the lowest rotator in the hierarchy?
        /// </summary>
        bool FinalRotator()
        {
            return GetComponentsInChildren<TurretRotator>().Length <= 1;
        }

        void OnDrawGizmos()
        {
            if (!debug) return;
            if (!rotator) return;

            Gizmos.matrix = Matrix4x4.identity;

            Color rayColor = Color.white;
            if (!FinalRotator()) rayColor = new Color(1, 1, 1, .1f);
            else if (!FireReady()) rayColor = Color.red;
            
            Gizmos.color = rayColor;
            Gizmos.DrawRay(rotator.transform.position, rotator.transform.forward * 100);
            
            Color color = Color.blue;
            if (!FireReady()) color = Color.red;
		
            Vector3 dir = new Vector3(0, 1, 0);
            if (myRotationAxis == RotationAxis.xAxis) dir = new Vector3(1, 0, 0);
            ShowGizmoLimits( new Vector2(limitLeft, limitRight), dir, color);
        }

        void ShowGizmoLimits(Vector2 clampAngle, Vector3 dir, Color color)
        {
            if (!rotator) return;
            
            Gizmos.matrix = Matrix4x4.TRS(rotator.position, transform.rotation, Vector3.one);
		
            //show gizmo limits for rotator
            Gizmos.color = color;
            Ray oldRay = new Ray();

            Quaternion baseRot = Quaternion.identity;

            Vector3 pos = Vector3.zero;

            //get the total rotation (right side)
            Quaternion totalRightRot = baseRot * Quaternion.Euler(dir * clampAngle.y);

            //get the total range of rotation
            int totalRot = Mathf.Clamp(Mathf.RoundToInt(clampAngle.y - clampAngle.x), 1, 360);

            for (int i = 0; i < totalRot; i += 5)
            {
                //draw a ray at every increment
                Quaternion tempRot = baseRot * Quaternion.Euler(dir * (clampAngle.x + i));
                Ray newRay = new Ray();
                newRay.origin = pos;
                newRay.direction = tempRot * (Vector3.forward * 5);

                //tint color for interior lines
                if (i > 0) Gizmos.color = new Color(1, 1, 1, .02f);

                Gizmos.DrawRay(newRay);
                Gizmos.color = color;
                if (i > 0) Gizmos.DrawLine(oldRay.GetPoint(1), newRay.GetPoint(1));

                oldRay = newRay;
            }
		
            //draw the final ray
            Ray rightRay = new Ray();
            rightRay.origin = pos;
            rightRay.direction = totalRightRot * (Vector3.forward * 5);
            Gizmos.DrawRay(rightRay);
            Gizmos.DrawLine(oldRay.GetPoint(1), rightRay.GetPoint(1));
        }
        
        
        /// <summary>
        /// Determine the signed angle between two vectors around an axis ( axis must be normalized! )
        /// </summary>
        public static float AngleSigned( Vector3 v1, Vector3 v2, Vector3 axis ) {
            return Mathf.Atan2(
                       Vector3.Dot(axis, Vector3.Cross(v1, v2)),
                       Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }

        void Update()
        {
            if (!target) return;
            AimAtPosition(target.position);
        }

        /// <summary>
        /// Aims this turret at the given position.
        /// </summary>
        /// <param name="pos">World space position of target.</param>
        public void AimAt(Vector3 pos)
        {
            AimAtPosition(pos);
        }

        void AimAtPosition(Vector3 aimPos)
        {
            if (!rotator)
            {
                Debug.LogError("No rotator has been defined for " + name, gameObject);
                enabled = false;
                return;
            }
            
            // find target's location in rotation plane
            Vector3 localTarget = rotator.InverseTransformPoint( aimPos ); 
            Vector3 rotatorPlaneTarget = rotator.TransformPoint(localTarget );

            Vector3 bearingDirection = transform.up;
            if (myRotationAxis == RotationAxis.xAxis) bearingDirection = transform.right;

            // Find the bearing - the euler angle on my axis that I should be aiming at
            bearing = AngleSigned(transform.forward, rotatorPlaneTarget - rotator.transform.position, bearingDirection);

            clampedBearing = bearing;

            // Clamp the bearing
            if (clamped)
                clampedBearing = Mathf.Clamp(bearing, limitLeft, limitRight);
		
            if (myRotationAxis == RotationAxis.yAxis)
                rotator.localEulerAngles = new Vector3( 0f, clampedBearing, 0f );
            else 
                rotator.localEulerAngles = new Vector3( clampedBearing, 0, 0f );

            // Find the error due to clamping
            error = Mathf.Abs(bearing - clampedBearing);
        }

        /// <summary>
        /// Turret rotator is fire ready if its not trying to aim at something beyond its rotation limits.
        /// </summary>
        public override bool FireReady()
        {
            return error <= maxDegreesError;
        }
    }
}