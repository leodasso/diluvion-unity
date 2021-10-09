using UnityEngine;
using TMPro;
using Diluvion;
using Diluvion.Ships;
using UnityEngine.UI;

namespace DUI
{

    /// <summary>
    /// Displays the label and the current / max crew for a station.
    /// </summary>
    public class DUIStationInfo : DUIPanel
    {

        public TextMeshProUGUI stationName;
        public TextMeshProUGUI stationCrew;
        public Image officerIcon;
        public CanvasGroup crewGroup;

        Station _station;
        Collider2D _stationCol;

        public void Init(Station st)
        {
            _station = st;
            _stationCol = _station.GetComponent<Collider2D>();
            stationName.text = _station.LocalizedName();
        }

        public void UpdateCrew(int currentCrew, int maxCrew)
        {
            officerIcon.gameObject.SetActive(_station.requireOfficer);

            if (_station.officer != null)
            {
                officerIcon.color = Color.yellow;
                crewGroup.alpha = 1;
            }
            else
            {
                officerIcon.color = Color.gray;
                crewGroup.alpha = 0;
            }
            
            stationCrew.text = currentCrew + " / " + maxCrew;
        }

        protected override void Update()
        {
            base.Update();

            if (!_station) return;

            //full alpha if hovered
            if (DiluvionPointer.HoveredCollider() == _station.gameObject) alpha = 1;
            else alpha = .5f;
        }

        void LateUpdate()
        {

            if (!_station || !_stationCol) return;

            //Position correctly based on the station
            Vector3 point = new Vector3(_station.transform.position.x, _stationCol.bounds.max.y, _station.transform.position.z);
            transform.position = FollowTransform(point, 5, InteriorView.Get().localCam);
        }
    }
}