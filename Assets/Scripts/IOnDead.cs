using UnityEngine;
using System.Collections;

public delegate void OnDead(float afterTime);
public interface IOnDead  {

    event OnDead onDead;
}
