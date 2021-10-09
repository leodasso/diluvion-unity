using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.Roll;
using System;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName ="new room entry", menuName ="Diluvion/RollTables/room entry")]
public class RoomEntry : SpawnableEntry {

    public override GameObject Create(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        return Instantiate(prefab, position, rotation, parent);
    }
}
