using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Diluvion.Ships
{

    /// <summary>
    /// Station slot is the housing for any station. It's charged with spawning the station and positioning it correctly.
    /// </summary>
    public class StationSlot : MonoBehaviour
    {

        [ToggleGroup("equipOnAwake")]
        public bool equipOnAwake;

        [Tooltip("If cosmetic, all the acting components on the spawned station will be disabled.")]
        public bool cosmetic;

        [ToggleGroup("equipOnAwake"), DisableIf("randomStation")]
        [AssetList(Path = "Prefabs/Interiors/Stations/")]
        public Station stationToEquip;

        [ToggleGroup("equipOnAwake")]
        public bool randomStation;
        
        [ShowIf("randomStation")]
        [ToggleGroup("equipOnAwake")]
        [AssetList(Path = "Prefabs/Interiors/Stations/")]
        public List<Station> possibleStations = new List<Station>();

        [ReadOnly]
        public Station equippedStation;

        [BoxGroup("testing", order: 199)]
        [AssetList(AutoPopulate = false, Path = "Prefabs/Interiors/Stations/")]
        public Station testStation;

        bool HasTestStation()
        {
            return (testStation != null);
        }

        [BoxGroup("testing")]
        [ShowIf("HasTestStation")]
        [OnValueChanged("PreviewStationToggle")]
        public bool previewStation;

        void PreviewStationToggle()
        {
            if (previewStation) EquipStation(testStation);
            else
            {
                RemoveStation();
                Show();
            }
        }

        private void Awake()
        {
            Hide();
            
            if (equipOnAwake)
            {
                // equip a random station from the list
                if (randomStation && possibleStations.Count > 0)
                {
                    int r = Random.Range(0, possibleStations.Count);
                    Station s = possibleStations[r];
                    EquipStation(s);
                }
                
                
                else if (stationToEquip)
                    // equip station
                    EquipStation(stationToEquip);
            }
        }

        void EquipStation(Station s)
        {
            Hide();
            equippedStation = Instantiate(s);

            if (cosmetic)
            {
                equippedStation.enabled = false;
                //Animator animator = equippedStation.GetComponent<Animator>();
                //if (animator) animator.enabled = false;
                Rigidbody2D rb = equippedStation.GetComponent<Rigidbody2D>();
                if (rb) Destroy(rb);

                Collider2D col = equippedStation.GetComponent<Collider2D>();
                if (col) col.enabled = false;
            }
            
            equippedStation.transform.parent = transform;
            equippedStation.transform.localEulerAngles = equippedStation.transform.localPosition = Vector3.zero;
            gameObject.layer = equippedStation.gameObject.layer;
        }

        public void EquipStation(ShipModule module)
        {
            if (!module.installsStation) return;
            
            if (GetComponent<MeshRenderer>()) GetComponent<MeshRenderer>().enabled = false;

            //spawn the station
            Station s = module.stationPrefab;
            EquipStation(s);
            equippedStation.linkedModule = module;
        }

        public void RemoveStation()
        {
            if (!equippedStation) return;

#if UNITY_EDITOR
            DestroyImmediate(equippedStation.gameObject);
#else
            equippedStation.DisableStation();
            Destroy(equippedStation.gameObject);
#endif
        }

        void Hide()
        {
            Renderer r = GetComponent<Renderer>();
            if (r) r.enabled = false;
        }

        void Show()
        {
            Renderer r = GetComponent<Renderer>();
            if (r) r.enabled = true;
        }
    }
}