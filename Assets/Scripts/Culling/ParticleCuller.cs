using UnityEngine;
using Sirenix.OdinInspector;


/// <summary>
/// When placed on an object with a particle system, culls in / out that particle system. Extends from culler base.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ParticleCuller : CullerBase
{

    ParticleSystem _ps;
    
    void Start()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    protected override bool SetState(bool enabled)
    {
        if (!_ps) return base.SetState(enabled);
        
        if (enabled && !_ps.isPlaying) _ps.Play();
        
        if (!enabled && _ps.isPlaying) _ps.Stop();
        
        return base.SetState(enabled);
    }

    [Button]
    void Play()
    {
        SetState(true);
    }

    [Button]
    void Stop()
    {
        SetState(false);
    }
}
