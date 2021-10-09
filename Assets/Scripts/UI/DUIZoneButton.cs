using UnityEngine;
using UnityEngine.UI;
using Diluvion.SaveLoad;

namespace DUI
{

    public class DUIZoneButton : MonoBehaviour
    {

        public GameZone zone;
        Button button;

        void Start()
        {
            // If this is a discovered zone, do the stuff
            if (DSave.HasDiscoveredZone(zone))
            {
                Animator a = GetComponent<Animator>();
                a.SetBool("new", DUITravel.newZones.Contains(zone));
                a.SetBool("available", true);
            }
            
            // otherwise set the object inactive
            else gameObject.SetActive(false);
        }

        public void SelectZone()
        {
            DUITravel travelWindow = GetComponentInParent<DUITravel>();
            travelWindow.SelectZone(this);
        }
    }
}