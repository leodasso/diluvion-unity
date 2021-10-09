using UnityEngine;
using System.Collections;
using System.Collections.Generic;





public class TweakChildMaterials : MonoBehaviour {

    public List<Material> materialBases = new List<Material>();
    public string editProperty = "_EmissionColor";
    public float targetValue = 1;
    public float changeRate = 5;
    float currentValue = 0;
    List<Material> materialInstances;
    Color initColor;
    // Use this for initialization
    Material tempMat;
    bool doneMaterialInit = false;

	void Awake ()
    {
        initColor = Color.black;
        InitSharedMaterialInstances();
    }
	
    //INitializeAll the shared materials to new instances
    void InitSharedMaterialInstances()
    {
        if (doneMaterialInit) return;
        materialInstances = new List<Material>();
        foreach (Material materialBase in materialBases)
        {
            tempMat = new Material(materialBase);
            materialInstances.Add(tempMat);
        }

        List<Material> addMaterials = new List<Material>();
        foreach (Renderer render in GetComponentsInChildren<Renderer>())
        {
            if (render == null) continue;
            if (render.sharedMaterials== null) continue;
            if (render.sharedMaterials.Length<1) continue;

            addMaterials.Clear();
            foreach (Material renderMat in render.sharedMaterials)
            {
                foreach (Material materialInstance in materialInstances)
                {
                    if (renderMat.name == materialInstance.name) addMaterials.Add(materialInstance);
                    else addMaterials.Add(null);
                }
            }

            for (int i = 0; i < addMaterials.Count; i++)
            {

                if (addMaterials[i] == null) continue;
                render.sharedMaterials[i] = addMaterials[i];
            }
        }
        doneMaterialInit = true;
    }


    void LerpToTargetValue()
    {
        if (targetValue != currentValue)
        {
            currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * changeRate);

            foreach (Material localMat in materialInstances)
                if (localMat != null)
                    localMat.SetColor(editProperty, Color.white * currentValue);
        }
    }
	// Update is called once per frame
	void Update ()
    {
        if (!doneMaterialInit) return;
        LerpToTargetValue();
     



    }
}
