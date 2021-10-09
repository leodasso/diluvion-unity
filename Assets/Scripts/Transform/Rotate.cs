using UnityEngine;
using System.Collections;

namespace Misc
{
    public class Rotate : MonoBehaviour
    {
        public Space rotationSpace = Space.World;
        public float m_speed = 1.0f;

        void Update()
        {

            float deltaTime = Time.deltaTime;
            if (!Application.isPlaying) deltaTime = .0167f;

            transform.Rotate(new Vector3(0, deltaTime * m_speed, 0), rotationSpace);
        }
    }
}
