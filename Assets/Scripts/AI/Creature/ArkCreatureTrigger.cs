using UnityEngine;
using System.Collections;
using Diluvion.Ships;

namespace Diluvion
{

    public class ArkCreatureTrigger : MonoBehaviour
    {

        public Transform newTarget;
        public float expandTime = 2;
        public float expandSize;

        // On start lerps this to expand, and the color to fade out
        IEnumerator Start()
        {

            transform.localScale = Vector3.one;
            Vector3 finalScale = Vector3.one * expandSize;

            Material myMat;
            myMat = GetComponent<Renderer>().material;
            Color startColor = myMat.GetColor("_HighlightColor");
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

            Color startMainColor = myMat.GetColor("_RegularColor");
            Color endMainColor = new Color(startMainColor.r, startMainColor.g, startMainColor.b, 0);

            Color currentColor;
            Color currentMainColor;

            // Create a loop to animate size, color
            float progress = 0;
            while (progress < 1)
            {

                //float easedLerp = SpiderWeb.Calc.EaseOutLerp(0, 1, progress);

                transform.localScale = Vector3.Lerp(Vector3.one, finalScale, progress);
                currentColor = Color.Lerp(startColor, endColor, progress);
                currentMainColor = Color.Lerp(startMainColor, endMainColor, progress);

                myMat.SetColor("_HighlightColor", currentColor);
                myMat.SetColor("_RegularColor", currentMainColor);

                progress += Time.deltaTime / expandTime;
                yield return null;
            }

            Destroy(gameObject);
            yield break;
        }


        void OnTriggerEnter(Collider other)
        {

            ArkCreature otherArk = other.GetComponent<ArkCreature>();
            if (otherArk == null) return;

            Debug.Log("Setting target on " + otherArk.name);
            otherArk.SetTarget(newTarget);
        }
    }
}