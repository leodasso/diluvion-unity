using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace Diluvion.Roll
{


    /// <summary>
    /// Class for holding tables, its children can search for a table of the desired type, if it cant find it in here, it will check its own parent
    /// </summary>
    public class TableHolder : MonoBehaviour
    {
        [OnValueChanged("GetAllTypes"), AssetList(AutoPopulate = false)]
        public List<Table> tables = new List<Table>();

        [ReadOnly, MultiLineProperty, HideLabel]
        public string allTypeNames;

        [ReadOnly]
        public List<SerializableSystemType> containedTypes = new List<SerializableSystemType>();

        void Awake ()
        {
            //GetAllTypes();
        }

        /// <summary>
        /// Finds and returns a table with entries of the given type in the ancestry of t. Assumes t is an interior object.
        /// </summary>
        /// <typeparam name="T">Type of entries to search for</typeparam>
        /// <param name="searcher">The interior object looking for a table</param>
        public static Table FindTableForInterior<T> (Transform searcher) where T : Entry
        {
            
            // Check for tables in searcher's ancestry
            Table table = FindTable<T>(searcher);
            if (table != null) return table;
            
            // Check for an interior manager in parent of querier
            InteriorManager i = searcher.gameObject.GetComponentInParent<InteriorManager>();
            
            // If an interior manager was found...
            if (i)
            {
                // Get the 3D 'parent'
                Transform root = i.GetWorldParent();
                return FindTable<T>(root);
            }
            
            //Debug.Log(searcher.name + " can't find a parent interior manager; checking transform of " + searcher.name, searcher.gameObject);
            return FindTable<T>(searcher);
        }

        /// <summary>
        /// Finds & returns a table with entries of the given type in the nearest table holder in the ancestry of t.
        /// For interior objects, <see cref="FindTableForInterior{T}(Transform)"/>
        /// </summary>
        /// <typeparam name="T">Type of entry to search for in the tables</typeparam>
        /// <param name="t">The transform looking for a table.</param>
        public static Table FindTable<T> (Transform t) where T : Entry
        {
            if (t == null)
            {
                Debug.Log("Given transform is null");
                return null;
            }

            // Search up the hierarchy looking for an appropriate table.
            TableHolder holder = t.GetComponentInParent<TableHolder>();

            if (holder == null)
            {
                //Debug.Log(t.name + " can't find a table holder in hierarchy!", t.gameObject);
                return null;
            }

            return holder.Table(typeof(T));
        }


        /// <summary>
        /// public get for the Table, if it cant find a correct type here, it will check its own parent
        /// </summary>
        /// <param name="type"></param>
        /// <returns>If Null, then we have hit the top level object</returns>
        public Table Table (Type type)
        {
            if (containedTypes.Count < 1)
                GetAllTypes();

            foreach (SerializableSystemType t in containedTypes)
            {
                //Debug.Log(t.Name + " is assignable from: " + type.Name+ "? "+ t.SystemType.IsAssignableFrom(type) );
                //Debug.Log(type.Name + " is assignable from " + t.Name +"?  "+ type.IsAssignableFrom(t.SystemType) );
                if (type.IsAssignableFrom(t.SystemType))
                {
                    //Debug.Log(gameObject.name + " has the type: " + type.ToString(), gameObject);
                    return GetTableOfType(type);
                }
                 
            }

            if (!transform.parent) return null;

            TableHolder parentTable =  transform.parent.GetComponentInParent<TableHolder>();

            if (!parentTable) return null;

            return parentTable.Table(type);
        }


        public void AddUniqueTypes (Table tableWithTypes)
        {
            if (!tableWithTypes)
            {
                Debug.LogWarning("No table was provided.");
                return;
            }
            
            //tableWithTypes.GetUniqueTypes();

            foreach (SerializableSystemType st in tableWithTypes.containedTypes)
            {
                if(containedTypes.Contains(st))continue;
               // Debug.Log("Adding type: " +st.Name);
                containedTypes.Add(st);
                allTypeNames += st.Name + ", ";
            }
        }

        /// <summary>
        /// Re-aquires the types in all the tables
        /// </summary>
        [Button]
        public void GetAllTypes ()
        {
            allTypeNames = "Contains:";

            //Debug.Log("Clearing list");
            containedTypes.Clear();

            foreach (Table t in tables)
            {
                AddUniqueTypes(t);
            }
        }

        /// <summary>
        /// Gets the first table from the tables that contain the correct types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Table GetTableOfType (Type type)
        {
            foreach (Table t in TablesWithType(type))
                return t;//TODO YAGNI create temp table with all correct types of items?
            Debug.Log(" I have no Tables with type: " + type.ToString(),this);
            return null;
        }

        /// <summary>
        /// Filters the local tables for ones that contain the correct type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        List<Table> TablesWithType (Type type)
        {
            List<Table> returnList = new List<Table>();

            foreach (Table ta in tables)
            {
                if (ta==null) continue;
                if (ta.ContainsAssignable(type))
                {
                    returnList.Add(ta);
                   // Debug.Log(ta.name + " has the type: " + type.ToString() + " adding to returnList count: " + returnList.Count, this);
                }
                   
            }
            return returnList;
        }
    }
}

