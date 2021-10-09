using UnityEngine;
using System.Collections;
using System;
using Object = UnityEngine.Object;

namespace Diluvion
{
    /// <summary>
    /// Opens inventory shop
    /// </summary>
    [CreateAssetMenu(fileName = "shop action", menuName = "Diluvion/actions/open shop", order = 0)]
    public class OpenShop : Action
    {
        public override bool DoAction(Object o)
        {
            MonoBehaviour mono = o as MonoBehaviour;
            if (mono == null)
            {
                Debug.Log("Can't open shop on " + o.name);
                return false;
            }
            Inventory inv = mono.GetComponent<Inventory>();
            if (inv == null)
            {
                Debug.Log("No inventory attached to " + o.name);
                return false;
            }

            inv.CreateUI();
            return true;
        }

        protected override void Test()
        {
            Debug.Log(ToString());
            DoAction(testObject);
        }

        public override string ToString()
        {
            return "Open shop.";
        }
    }
}