using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Loot;
using Diluvion;

namespace DUI
{

    public class DUIZoneMap : MonoBehaviour
    {

        public LandmarkMapIcon lmIconPrefab;
        public GameObject playerIconPrefab;
        public RectTransform playerShipIcon;
        public GameObject mapWaypointPrefab;
        
        [Space]
        public RectTransform mapParent;
        public TextMeshProUGUI mapTitle;

        bool _debug;
        RectTransform _playerIcon;
        List<LandMark> _knownLandmarks;
        //MapObject _currentMap;
        RectTransform _mapTransform;


        // Use this for initialization
        void Start()
        {
            // Get the map from the current zone
            /*
            /_currentMap = GameManager.CurrentZone().map;

            if (_currentMap == null)
            {
                Debug.Log("The current scene has an incorrect keyword or a missing map object.  Map object couldn't be found.");
                return;
            }
            */
            ZoneMapInit();
        }

        
        void ZoneMapInit()
        {
            // Display the map's name
            mapTitle.text = GameManager.CurrentZone().zoneName.LocalizedText();

            // Display the map's Image
            GameObject newMap = Instantiate(GameManager.CurrentZone().mapPrefab);
            newMap.transform.SetParent(mapParent, false);
            _mapTransform = newMap.GetComponent<RectTransform>();

            // add the landmarks the player has discovered
            foreach (LandMark lm in LandMark.DiscoveredLandmarks())
                PlaceLandmarkIcon(lm);

            // If the player has this setting enabled, show their position on map.
            if (PlayerPrefs.GetInt(QualitySettings.prefsMapKey, 1) == 1)
                PlacePlayerIcon();
            
            // Place all the quest waypoints
            foreach (QuestActor wp in QuestManager.GetAllWaypoints())
            {
                GameObject wpInstance = Instantiate(mapWaypointPrefab, _mapTransform);
                wpInstance.GetComponent<MapWaypoint>().InitForWaypoint(this, wp);
            }
        }


        void PlaceAllLandmarks()
        {
            foreach (LandMark lm in LandMark.AllLandmarks()) PlaceLandmarkIcon(lm);
        }

        void PlacePlayerOnMap()
        {
            if (_playerIcon != null) return;
            _playerIcon = Instantiate(playerShipIcon);

            // Set hierarchy & position
            _playerIcon.SetParent(_mapTransform, false);
            _playerIcon.anchoredPosition = PositionOnMap(PlayerManager.PlayerTransform());
        }

        public void SetDebug()
        {
            if (_debug) return;
            _debug = true;
            PlaceAllLandmarks();
            PlacePlayerOnMap();
        }

        void PlaceLandmarkIcon(LandMark lm)
        {
            LandmarkMapIcon newIcon = Instantiate(lmIconPrefab);

            // Set hierarchy & position
            newIcon.transform.SetParent(_mapTransform, false);
            newIcon.GetComponent<RectTransform>().anchoredPosition = PositionOnMap(lm.transform);

            newIcon.Init(lm);
        }

        void PlacePlayerIcon()
        {
            if (PlayerManager.PlayerShip() == null) return;

            GameObject playerIcon = Instantiate(playerIconPrefab);

            playerIcon.transform.SetParent(_mapTransform, false);
            playerIcon.GetComponent<RectTransform>().anchoredPosition = PositionOnMap(PlayerManager.PlayerShip().transform);
            playerIcon.transform.localEulerAngles = new Vector3(0, 0, -PlayerManager.PlayerShip().transform.eulerAngles.y);
        }

        /// <summary>
        /// Returns a position on the map based on the transform's real world positioning.
        /// uses scene control's map origin and end; if those aren't correct, this wont be correct.
        /// </summary>
        public Vector2 PositionOnMap(Transform t)
        {

            Vector2 mapSize = _mapTransform.rect.size;
            Vector2 mapHalf = mapSize * .5f;
            Vector2 normalizedPos = NormalizedPosition(t);

            Vector2 fittedPos = Vector2.Scale(mapSize, normalizedPos);

            return fittedPos - mapHalf;
        }

        /// <summary>
        /// Returns a normalized position between 0 and 1 of the transform in the scene's map.
        /// </summary>
        Vector2 NormalizedPosition(Transform t)
        {
            if (SceneControl.Get() == null) return Vector2.zero;
            
            // Get the size of real world that the map represents
            Vector2 sceneSize = SceneControl.Get().MapSize();
            Vector2 sceneOrigin = TopDownCoords(SceneControl.Get().mapOrigin.position);

            // Get the 'top down' position of the landmark
            Vector2 landmarkPos = TopDownCoords(t.position) - sceneOrigin;

            Vector2 normalizedPos = new Vector2(landmarkPos.x / sceneSize.x, landmarkPos.y / sceneSize.y);
            return normalizedPos;
        }

        Vector2 TopDownCoords(Vector3 realWorldPos)
        {
            return new Vector2(realWorldPos.x, realWorldPos.z);
        }
    }
}