using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Class with helpful static functions for selecting the correct spawnpoint. 
/// <para>Instances can be for travel from a particular zone, or just default spawn point.</para>
/// <para><see cref="SpawnPoint(GameZone)"/></para>
/// <para><see cref="SpawnPoint(string)"/></para>
/// <para><see cref="DefaultSpawn"/></para>
/// </summary>
public class PlayerSpawnPoint : MonoBehaviour {

    [Tooltip("Only for testing. Forces the ship to spawn from this point.")]
    public bool forceSpawn;
    public bool defaultSpawn;

    [Tooltip("If this is where the player should spawn when travelling from a certain zone, put that zone here.")]
    public GameZone travelZone;

    public UnityEvent onPlayerSpawned;

    /// <summary>
    /// If there's any points that force spawning returns the first.
    /// </summary>
    public static PlayerSpawnPoint OverrideSpawn()
    {
        foreach (PlayerSpawnPoint sp in SpawnPoints())
            if (sp.forceSpawn) return sp;

        return null;
    }

    /// <summary>
    /// Returns the first player spawn point marked 'default'
    /// </summary>
    public static PlayerSpawnPoint DefaultSpawn()
    {
        foreach (PlayerSpawnPoint sp in SpawnPoints())
            if (sp.defaultSpawn) return sp;

        return null;
    }

    /// <summary>
    /// Returns the first player spawn point meant for travel from the given zone.
    /// </summary>
    public static PlayerSpawnPoint SpawnPoint(GameZone fromZone)
    {
        foreach (PlayerSpawnPoint sp in SpawnPoints())
            if (sp.travelZone == fromZone) return sp;

        return null;
    }


    /// <summary>
    /// Returns a spawnpoint with the given name.
    /// </summary>
    public static PlayerSpawnPoint SpawnPoint(string name)
    {
        foreach (PlayerSpawnPoint sp in SpawnPoints())
            if (sp.name == name) return sp;

        return null;
    }

    static List<PlayerSpawnPoint> SpawnPoints()
    {
        List<PlayerSpawnPoint> spawnPoints = new List<PlayerSpawnPoint>();
        spawnPoints.AddRange(FindObjectsOfType<PlayerSpawnPoint>());
        return spawnPoints;
    }
}
