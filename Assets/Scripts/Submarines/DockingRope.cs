using UnityEngine;
using System.Collections;
using SpiderWeb;

namespace Diluvion
{

    public class DockingRope : MonoBehaviour
    {
        public float extendTime = 0.3f;
        public Transform ropeEnd;
        Vector3 startPos;

        Transform targetHook;

        Quaternion randomRot;

        public void FireDockRope (Transform target)
        {
            targetHook = target;

            randomRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

            StartCoroutine(TranslateDockRope(target));
        }

        void Update ()
        {
            if (!targetHook) return;

            ropeEnd.transform.position = targetHook.transform.position;
            transform.rotation = Quaternion.LookRotation(targetHook.transform.position - transform.position, randomRot * transform.forward);
        }

        IEnumerator TranslateDockRope (Transform target)
        {
            float lerpValue = 0;
            startPos = transform.position;

            while (!Calc.WithinDistance(0.1f, ropeEnd, target))
            {
                ropeEnd.position = Vector3.Slerp(startPos, target.transform.position, lerpValue / extendTime);
                yield return new WaitForEndOfFrame();
                lerpValue += Time.deltaTime;
            }

            OrbitCam.ShakeCam(.2f, target.transform.position);
        }


        public void StartPull ()
        {
            GetComponent<AKTriggerCallback>().Callback();
        }


        public void Killme ()
        {
            Destroy(ropeEnd.gameObject);
           Debug.Log("DESPAWNING FROM:" + this.name , this); Destroy(gameObject);
        }
    }
}