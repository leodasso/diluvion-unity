using UnityEngine;


//TODO more with this bullshit
public class FilteredObjectAttribute : PropertyAttribute
{
    public string BasePath
    {
        get;
        private set;
    }


    public FilteredObjectAttribute(string basePath)
    {
        BasePath = basePath;
    }


}

