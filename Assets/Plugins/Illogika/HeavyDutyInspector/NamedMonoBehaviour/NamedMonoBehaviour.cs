//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2015  Illogika
//----------------------------------------------

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
namespace HeavyDutyInspector
{
    [System.Serializable]
    public class NamedMonoBehaviour : MonoBehaviour {

        public NamedMonoBehaviour() : base()
        {
            typeName = GetType().ToString();
        }

    #pragma warning disable 414
        [SerializeField]
        [HideInInspector]
        private string typeName;
    #pragma warning restore 414

        [NMBName]
        public string scriptName;

        [NMBColor]
        public Color scriptNameColor = Color.white;
    }
}