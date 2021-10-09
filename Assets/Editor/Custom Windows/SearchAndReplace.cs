using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public enum ComponentTypes
{
    MeshRenderer
}

//TODO YAGNI, Add generlized support for any component from a dropDown
public class SearchAndReplace : EditorWindow
{
    public Object objectToSearchFor;
    public Object objectToReplaceWith;

    public ComponentTypes componentTolookIn;
    
    public bool searchInProject;

    [MenuItem("Diluvion/Window/Search and replace")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SearchAndReplace));
    }


    void OnGUI()
    {

        objectToSearchFor = EditorGUILayout.ObjectField(objectToSearchFor, typeof(Object),false);
        objectToReplaceWith = EditorGUILayout.ObjectField(objectToReplaceWith, typeof(Object), false);

        if (objectToSearchFor == null) return;
        if (objectToReplaceWith == null) return;


        if (GUILayout.Button("Replace!"))
            ReplaceAllObjectsOfType(componentTolookIn);



    }


    void ReplaceAllObjectsOfType(ComponentTypes types)
    {
      /*  System.Type type;
        switch (types)
        {
            default:
                {
                    type = typeof(MeshRenderer);
                    break;
                }
        }
        */
        List<MeshRenderer> componentsToCheck = new List<MeshRenderer>();

        MeshRenderer[] checkComponents = FindObjectsOfType<MeshRenderer>();
        componentsToCheck.AddRange(checkComponents);
        foreach(MeshRenderer mr in componentsToCheck)
        {

			List<Material> replacematerials = new List<Material>();
			foreach (Material m in mr.sharedMaterials) {
	            if(m == objectToSearchFor)
	            {
					replacematerials.Add((Material)objectToReplaceWith);
	            }
				else
					replacematerials.Add(m);
			}
			mr.sharedMaterials = replacematerials.ToArray();
        }


    }



}
