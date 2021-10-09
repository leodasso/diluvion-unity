using UnityEngine;

namespace Diluvion
{
    /// <summary>
    /// Rotates to keep this instance even while the ship is manouevering all around
    /// </summary>
    public class CapsuleRotator : MonoBehaviour
    {

        float moveSpeed = 5;

        void Awake()
        {
            if (GetComponent<MeshRenderer>()) GetComponent<MeshRenderer>().enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            Quaternion gotoRot = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, InteriorView.mainShipZ));
            Quaternion inputRot = Quaternion.Slerp(transform.localRotation, gotoRot, Time.deltaTime * moveSpeed);
            transform.localRotation = inputRot;
        }
    }
}