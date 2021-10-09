using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;
using PathologicalGames;

namespace Diluvion
{
    /// <summary>
    /// Bolt is a munition that fires in a straight path, and after a certain range will cool off and begin to drag heavily.
    /// </summary>
    public class Bolt : Munition
    {
        /// <summary>
        /// Once it's reached the max distance, how many seconds will it take to despawn?
        /// </summary>
        [Tooltip("Once it's reached the max distance, how many seconds will it take to despawn?")]
        public float despawnTime = 2;

        [Space]
        public Color hotColor;
        public Color cooledColor;
        public float colorLerpTime = 1;
        public List<Renderer> glowyBits;

        Color currentColor;
        string colorPropertyName = "_EmissionColor";

        public override void OnSpawned()
        {
            base.OnSpawned();
            RB.useGravity = false;
            RB.drag = 0;
            currentColor = hotColor;
            StartCoroutine(LerpColor());
        }

        /// <summary>
        /// When a bolt is at max range, turn on gravity and add drag, so it kind of sinks before despawning.
        /// </summary>
        protected override void Dudify()
        {
            if (!gameObject.activeInHierarchy) return;
            RB.useGravity = true;
            RB.drag = 3;
          //  Debug.Log("bolt dudify on ", gameObject );
            StartCoroutine(WaitAndEnd());
        }

        IEnumerator WaitAndEnd()
        {
            yield return new WaitForSeconds(despawnTime);
            base.Dudify();
        }

        /// <summary>
        /// Interpolate the emission color from hot to cool.
        /// </summary>
        IEnumerator LerpColor()
        {
            if (glowyBits.Count < 1) yield break;

            float progress = 0;
            while (progress < 1)
            {
                currentColor = Color.Lerp(hotColor, cooledColor, progress);

                foreach (Renderer r in glowyBits)
                {
                    if (r == null) continue;
                    r.material.SetColor(colorPropertyName, currentColor);
                }

                progress += Time.deltaTime / colorLerpTime;
                yield return null;
            }
        }

        protected override void OnDespawned(SpawnPool pool)
        {
            base.OnDespawned(pool);
            StopAllCoroutines();
        }
    }
}