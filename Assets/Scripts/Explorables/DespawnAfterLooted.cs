using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using Diluvion;

/// <summary>
/// Class for handling despawning of objects with inventories, as to make sure we dont end up with endless amounts of them
/// </summary>
public class DespawnAfterLooted : MonoBehaviour
{
    public float despawnTime = 60;

    float _timer;
    DockControl _dock;
    bool _looted;

    void RegisterDockControl()
    {
        if (_dock == null) _dock = GetComponent<DockControl>();
        
        if (_dock != null)
            _dock.unDocked += OnUndock;
    }


    /// <summary>
    /// resume counting after undocking
    /// </summary>
    public void OnUndock(DockControl who)
    {        
        _looted = true;
        _timer = 0;
    }


   /// <summary>
   /// Checks the players position relative to this so we dont respawn in sight or right next to player
   /// </summary>
    bool WithinPlayerRange()
   {
       return Calc.WithinDistance(75, transform, PlayerManager.PlayerTransform());
   }

    void Update()
    {
        if (!_looted) return;

        _timer += Time.deltaTime;

        if (_timer >= despawnTime)
        {
            if (WithinPlayerRange()) return;
            Destroy(gameObject);
        }
    }
	
}
