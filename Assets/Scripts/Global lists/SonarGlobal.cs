using UnityEngine;
using System.Collections.Generic;
using Diluvion.Sonar;

namespace Diluvion
{

    [CreateAssetMenu(fileName = "sonar global", menuName = "Diluvion/global lists/sonar")]
    public class SonarGlobal : GlobalList
    {

        static SonarGlobal sonarGlobal;

        public List<Signature> allSignatures = new List<Signature>();
        public Signature playerSignature;

        const string resourceName = "sonar global";

        public static SonarGlobal Get()
        {
            if (sonarGlobal) return sonarGlobal;
            sonarGlobal = Resources.Load(resourcesPrefix + resourceName) as SonarGlobal;
            return sonarGlobal;
        }

        public static Signature GetSignature(string name)
        {
            return GetObject<Signature>(name, Get().allSignatures);
        }
        


        protected override void TestAll()
        {
            TestAllObjects(allSignatures, new GetObjectDelegate(GetSignature));
        }


        public override void FindAll()
        {
#if UNITY_EDITOR
            ConfirmObjectExistence(Get(), resourcesPrefix + resourceName);
            allSignatures = LoadObjects<Signature>("Assets/Prefabs/Sonar");
            Debug.Log("Finding all sonar signatures.");    
            SetDirty(this);
#endif
        }
    


        /// <summary>
        /// Returns the prefab of the signature used to indicate 'this is player'
        /// </summary>
        public static Signature PlayerSignature()
        {
            return Get().playerSignature;
        }
    }
}