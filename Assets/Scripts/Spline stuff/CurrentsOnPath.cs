using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.Curvy.Generator.Modules;
using Sirenix.OdinInspector;

namespace Diluvion
{

    public class CurrentsOnPath : ObjectsOnPath
    {

        [FoldoutGroup("current"), InlineEditor, OnValueChanged("RefreshStats"), AssetsOnly] 
        public OceanCurrentParams currentParams;
       
        [FoldoutGroup("current")]
        public float exteriorOffsetScale = 1;
        
        [FoldoutGroup("current"), ReadOnly]
        public CurvyGenerator myGenerator;

        [ReadOnly, FoldoutGroup("current")]
        public GameObject exteriorMesh;
        
        [ReadOnly, FoldoutGroup("current")]
        public InputSplinePath inputPath;

        [ReadOnly]
        public List<OceanCurrent> childCurrents = new List<OceanCurrent>();
     
       
        GameObject GeneratorPrefab()
        {
            return Resources.Load("oceanCurrentTubeGenerator") as GameObject;
        }

        CurvyGenerator MyGenerator()
        {
            if (myGenerator != null) return myGenerator;
            myGenerator = GetComponentInChildren<CurvyGenerator>();
            if (myGenerator == null)
            {
                GameObject gen = Instantiate(GeneratorPrefab(), spline.transform.position, spline.transform.rotation) as GameObject;
                gen.transform.SetParent(transform, true);
                myGenerator = gen.GetComponent<CurvyGenerator>();
            }
            return myGenerator;
        }


        float Radius()
        {
            return objectsToSpawn.First().GetComponent<CapsuleCollider>().radius;
        }


        [Button("generate path mesh"), FoldoutGroup("current")]
        public void SetupGenerator()
        {
            if (MyGenerator() == null) return;
            inputPath = MyGenerator().GetComponentInChildren<InputSplinePath>();
            inputPath.Spline = (CurvySpline)spline;
            BuildShapeExtrusion bse = MyGenerator().GetComponentInChildren<BuildShapeExtrusion>();
            bse.ScaleMode = BuildShapeExtrusion.ScaleModeEnum.Advanced;
            bse.ScaleX = exteriorOffsetScale; //* currentDirection;

            if (useCurveForScale)
                bse.m_ScaleCurveX = sizeOnSpline;
            
            MyGenerator().Refresh();
        }

        [Button("apply path mesh"), FoldoutGroup("current")]
        void SaveToScene()
        {
            #if UNITY_EDITOR
            if (exteriorMesh != null) DestroyImmediate(exteriorMesh);
            exteriorMesh = MyGenerator().GetComponentInChildren<CreateMesh>().SaveToScene(transform);
            exteriorMesh.transform.position = spline.transform.position;
            exteriorMesh.name = name + "_exteriorMesh";
            
            DestroyImmediate(exteriorMesh.GetComponent<CGMeshResource>());
            DestroyImmediate(MyGenerator().gameObject);
            
            UnityEditor.EditorUtility.SetDirty(exteriorMesh);
            UnityEditor.EditorUtility.SetDirty(gameObject);
            
            #endif
        }

        void RefreshStats()
        {
            // remove null entries
            childCurrents = childCurrents.Where(i => i != null).ToList();
            foreach (var c in childCurrents) c.parameters = currentParams;
        }


        protected override void CleanList()
        {
            base.CleanList();
            childCurrents = childCurrents.Where(i => i != null).ToList();
        }

        public override GameObject SpawnLength(GameObject reference, Vector3 orientation, float tf, float angle, out float length, bool asPrefab = false)
        {
            GameObject newReturn = base.SpawnLength(reference, orientation, tf, angle, out length, asPrefab);

            OceanCurrent current = newReturn.GetComponent<OceanCurrent>();
            
            if (current)
            {
                childCurrents.Add(current);
                current.parameters = currentParams;
                current.RandomizeVisibleStreams();
            }

            return newReturn;
        }
    }
}