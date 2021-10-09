using UnityEngine;

/// <summary>
/// Adds the default cubemap to reflection probes.
/// </summary>
[RequireComponent(typeof(ReflectionProbe))]
public class AtmoReflectionProbe : MonoBehaviour {

    static Cubemap reflectionMap;
    ReflectionProbe probe;

	// Use this for initialization
	void Awake () {

        probe = GetComponent<ReflectionProbe>();
        if ( probe == null ) Destroy(this);

        probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Custom;
        probe.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces;
        probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;

        probe.customBakedTexture = Reflection();
    }

    static Cubemap Reflection()
    {
        if (reflectionMap) return reflectionMap;
        reflectionMap = Resources.Load("default reflection") as Cubemap;
        return reflectionMap;
    }
}