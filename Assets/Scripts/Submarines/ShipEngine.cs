using UnityEngine;
using System.Collections;

namespace Diluvion.Ships
{

    [CreateAssetMenu(fileName = "ship engine", menuName = "Diluvion/subs/new engine")]
    public class ShipEngine : ScriptableObject
    {

        public ShipMake engineMake;
        public EngineAudio engineSound;
        public OverdriveStart overdriveStart;
        public OverdriveStop overdriveStop;
        [Space]
        public float maxEnginePower;

        [Space]
        public float rotationSpeed = 1;
        public float rotationDamping;
        public float minVelForTurn = 1;
        public float strafeDrag = 100;
        public float stability = 1;

        [Space]
        public float rotationProp;
        public float rotationDer;
        public float correctionProp;
        public float correctionDer;
    }

    public enum EngineAudio
    {
        Icer_Engine_Loop,
        Royal_Engine_Loop,
        Dweller_Engine_Loop,
        Huge_Royal_Engine_Loop
    }

    public enum OverdriveStart
    {
        Play_Dweller_Overdrive,
        Play_Royal_Overdrive,
        Play_Icer_Overdrive
    }

    public enum OverdriveStop
    {
        Stop_Dweller_Overdrive,
        Stop_Royal_Overdrive,
        Stop_Icer_Overdrive
    }
}
