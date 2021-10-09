using UnityEngine;
using System.Collections;

namespace Diluvion
{

    public class FollowPlayer : MonoBehaviour
    {

        // Update is called once per frame
        void Update()
        {

            if (PlayerManager.PlayerTransform()) return;

            Transform playerShip = PlayerManager.PlayerTransform();

            transform.position = playerShip.position;
        }
    }
}