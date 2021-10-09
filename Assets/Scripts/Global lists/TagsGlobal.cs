using UnityEngine;
using System.Collections.Generic;
using Diluvion.Roll;

namespace Diluvion
{
    [CreateAssetMenu(fileName = "tags global", menuName = "Diluvion/global lists/tags")]
    public class TagsGlobal : GlobalList
    {
        public List<Tag> allItems = new List<Tag>();
        public static TagsGlobal tagsGlobalStatic;
        const string resourceName = "tags global";

        public static TagsGlobal Get()
        {
            if (tagsGlobalStatic != null) return tagsGlobalStatic;
            tagsGlobalStatic = Resources.Load(resourcesPrefix + resourceName) as TagsGlobal;
            return tagsGlobalStatic;
        }

        public static Tag GetItem(string nameKey)
        {
            return GetObject(nameKey, Get().allItems) as Tag;
        }

        public static List<Tag> GetItems(List<string> itemNames)
        {
            return GetObjects(itemNames, Get().allItems);
        }
        

        protected override void TestAll()
        {
            TestAllObjects(allItems, new GetObjectDelegate(GetItem));

            Debug.Log("<color=green>Testing complete.</color>");
        }


        public override void FindAll()
        {
#if UNITY_EDITOR
            ConfirmObjectExistence(Get(), (resourcesPrefix + resourceName));
            allItems = LoadObjects<Tag>("Assets/Prefabs/RollTableObjects/Tags");
            SetDirty(this);
            Debug.Log("Loading all tags");
#endif
        }

    }
}