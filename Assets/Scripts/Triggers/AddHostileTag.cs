using UnityEngine;
using Diluvion.AI;
namespace Diluvion.Sonar
{

    public class AddHostileTag : MonoBehaviour
    {

        public Signature sigToAdd;

        // Use this for initialization
        void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Tools");
        }

        void OnTriggerEnter(Collider other)
        {

            Captain captain = other.GetComponentInChildren<Captain>();
           // if (captain != null) captain.AddHostileTag(sigToAdd);
        }
    }
}