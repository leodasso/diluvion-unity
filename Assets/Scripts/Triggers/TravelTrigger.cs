using UnityEngine;
using Sirenix.OdinInspector;
using Diluvion.Ships;
using Diluvion;
using Diluvion.SaveLoad;
using DUI;

public class TravelTrigger : Trigger
{
    [InfoBox("The Travel Trigger should have a uniform scale of 1, and be oriented so that the gizmo is facing into the scene, perpendicular to the boundary.")]
    [Tooltip("Add a new zone to the player's world map"), ToggleLeft]
    public bool discoverNewZone;
    [ShowIf("discoverNewZone")]
    public GameZone newZone;

    void OnDrawGizmosSelected()
    {
        Vector3 endPos = transform.position + transform.forward * -100;
        Gizmos.DrawLine(transform.position, endPos);
        Gizmos.DrawWireSphere(endPos, 2);
    }

    protected override void Start()
    {
        base.Start();
        
        // Instantiate the boundary indicator
        Instantiate(GameManager.TravelCompassPrefab(), transform);
    }

    public override void TriggerAction(Bridge otherBridge)
    {
        base.TriggerAction(otherBridge);
        AddNewZone();
        StartTravel();
    }

    //Start Travelling
    public void StartTravel()
    {
        // Pop up the world map
        CaptainsTools.ShowWorldMap();
    }

    //If this trigger discover a new zone
    void AddNewZone()
    {

        if (discoverNewZone)
        {
            // If this is a brand new zone, add it to the new zones list so the special animation can play
            if (!DSave.HasDiscoveredZone(newZone))
                DUITravel.newZones.Add(newZone);

            newZone.onZoneDiscovered.Invoke();
            // Add the new zone to the save file
            DSave.current.AddZone(newZone);
        }
    }
}
