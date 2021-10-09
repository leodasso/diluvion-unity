using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using HeavyDutyInspector;
using System.Linq;
using System.Resources;
using Diluvion;
using Diluvion.AI;
using NodeCanvas.BehaviourTrees;
using Debug = UnityEngine.Debug;


namespace Diluvion.Roll
{
    /// <summary>
    /// Main Input interface for getting a filtered list of rolllable objects, represents the object asking the table
    /// </summary>
    public interface IRoller
    {
        /// <summary>
        /// The Conditions for valid objects, 
        /// </summary> 
        bool RollQuery(Entry checkedObject);
        /// <summary>
        /// Shortcut to the list of tags
        /// </summary>
        List<Tag> RollingTags { get; set; }
        void CombineTagList(List<Tag> tags);
    }

    /// <summary>
    /// Main Input interface for getting a filtered list of rolllable objects, represents the object asking the table
    /// </summary>
    public interface ITransformRoller:IRoller
    {
        /// <summary>
        /// shortcut to the object transform
        /// </summary>
        Transform Roller();
    }

    [System.Serializable]
    public class TypeToggle
    {
        public bool toggled;
        public SerializableSystemType type;

        public TypeToggle(SerializableSystemType t)
        {
            toggled = false;
            type = t;
        }

        public override bool Equals(object obj)
        {
            TypeToggle otherToggle = obj as TypeToggle;
            SerializableSystemType t = null;
            if (otherToggle == null)
                t = obj as SerializableSystemType;
            else
            {
                t = otherToggle.type;
            }
            return t == type;
        }
    }

    [System.Serializable]
    public class TableEntry
    {
        public Entry entry = null;
        public int tableBonus = 0;
        public int guaranteeBonus = 0;
        public int guaranteeDrop = 0;
        public int dropTimes = 0;
        public int totalResourcesSpent = 0;

        public TableEntry() { }
        public TableEntry(Entry e)
        {
            entry = e;
            tableBonus = 0;
            guaranteeBonus = 0;
            guaranteeDrop = 0;
            dropTimes = 0;
            totalResourcesSpent = 0;
        }


        int EntryWeight()
        {
            if (entry == null) return 0;
            return entry.weight;
        }

        public int TotalWeight
        {
            get { return EntryWeight() + tableBonus + guaranteeBonus; }           
        }
      
        public bool HasEntry(Entry e)
        {
            return entry == e;
        }

        public System.Type EntryType()
        {
            if (entry == null) return null;
            return entry.GetType();
        }
    }

    /// <summary>
    /// A generic Roll table class, contains a collection of Roll Objects and functions that return the weighted objects randomly
    /// </summary>
    [CreateAssetMenu(fileName = "new rollTable", menuName = "Diluvion/RollTables/Roll Table")]
    public class Table : Entry
    {
        [Space()]
        [Comment("Table Variables")]
        [Button("Roll For Item", "TestRoll", true)]
        public bool rollBool;

        public List<SerializableSystemType> containedTypes = new List<SerializableSystemType>();

        [Tooltip("Debug for the last rolled object ")]
        public Entry lastRolledObject;

        [SerializeField]
        public Entry entryToAdd = null;
        [SerializeField]  
        public List<Entry> tableEntries = new List<Entry>();

        [SerializeField]
        public List<TableEntry> rollTable = new List<TableEntry>();

        [SerializeField]
        int _lastRoll;
        [SerializeField]
        int _totalWeight;
        [SerializeField]
        float _currentWeight;
        
        [SerializeField]
        int minDanger;
        [SerializeField]
        int minValue;


        public List<TypeToggle> typeList = new List<TypeToggle>();

        /// <summary>
        /// Sums the whole list of object's weight and stores it in the totalWeight;
        /// </summary>
        public void TotWeight()
        {
            List<TableEntry> filtered = TypeFilteredList(rollTable);
            _totalWeight = TotalWeight(filtered);
            //Debug.Log("Setting total weight = " + totalWeight + "from " + filtered.Count + " objects");
        }

        /// <summary>
        /// Returns TotalWeight
        /// </summary>
        public int TotalWeight(List<TableEntry> inputTable)
        {
            int currentWeight = 0;
            for (int i = 0; i < inputTable.Count; i++)
            {
                if (inputTable[i] == null) continue;
                currentWeight += inputTable[i].TotalWeight;
            }

            return _totalWeight = currentWeight;
        }
        #region paired list handlers
        public void AddRoll(Entry entry)
        {
            if (entry == null) return;
            if (ContainsRoll(entry)) return;
            //Debug.Log("adding" + entry.name);
            TableEntry te =new TableEntry(entry);        
            rollTable.Add(te);            
        }



        

        #region danger and value calc
        public void RecalcDanger()
        {
            int mDanger = 9999;
          
            foreach (Entry e in tableEntries)
            {
                if (e == null) continue;
                if (e.Danger() < mDanger)
                    mDanger = e.Danger();
            }
           minDanger =  mDanger;
        }
        
        public void RecalcValue()
        {
            int mValue = 9999;
          
            foreach (Entry e in tableEntries)
            {
                if (e == null) continue;
                if (e.Value() < mValue)
                    mValue = e.Value();
            }
            minValue =  mValue;
        }
        
        /// <summary>
        /// Returns the minimum danger on this table
        /// </summary>
        /// <returns></returns>
        public override int Danger()
        {
            if (minDanger != 0) return minDanger;
            RecalcDanger();
            return minDanger;
        }

        /// <summary>
        /// Minimum value in this table
        /// </summary>
        /// <returns></returns>
        public override int Value()
        {
            if (minValue != 0) return minValue;
            RecalcValue();
            return minValue;
        }
        #endregion
        
        
        public void AddRoll<T>(List<T> inputEntries) where T: Entry
        {
            foreach(T et in inputEntries)           
                AddRoll(et as Entry);
        }

        /*
        List<int> _indexRemoval = new List<int>();
        void StoreRemovalIndex(int i)
        {
            _indexRemoval.Clear();
            _indexRemoval.Add(i);
        }

        void RemoveStoredIndex()
        {
            foreach(int i in _indexRemoval)
            {
                RemoveRoll(i);
            }
        }
        */
       
        
        public void RemoveRoll(int index)
        {
            if(tableEntries.Count>index)
                tableEntries.RemoveAt(index);
            if (rollTable.Count > index)
                rollTable.RemoveAt(index);
          //  GetUniqueTypes();
        }
        
        /// <summary>
        /// safely removes the entry and rollentry
        /// </summary>   
        List<TableEntry> _removeList = new List<TableEntry>();
        public void StoreForRemoval(Entry e)
        {
            foreach (TableEntry te in rollTable)
            {
                if (te.HasEntry(e))
                    _removeList.Add(te);
            }           
        }

        public void RemoveStored()
        {
            foreach (TableEntry te in _removeList)
                rollTable.Remove(te);
        }

        public void RemoveRoll(Entry e)
        {
            if (e == null) return;
            Debug.Log("Removing E " + e.name);
            StoreForRemoval(e);
            tableEntries.Remove(e);
            RemoveStored();
        }

        public void RemoveRoll(TableEntry tee)
        {
            if (lastRolledObject == tee.entry)
                lastRolledObject = null;           
            rollTable.Remove(tee);
            tableEntries.Remove(tee.entry);          
        }

        public void ClearTables()
        {
            tableEntries.Clear();
            rollTable.Clear();
        }
        #endregion

        #region table search and manipulation
        /// <summary>
        /// public Get for the RollTable
        /// </summary>
        /// <returns></returns>
        public void ResizeTable()
        {
            //Debug.Log("Table Entry Count1 : " + tableEntries.Count);
            tableEntries = tableEntries.Distinct().ToList();//Removes duplicates
           // Debug.Log("Table Entry Count2 : " + tableEntries.Count);
            foreach (Entry e in tableEntries)
            {
                AddRoll(e);
            }

            GetUniqueTypes();  
        }
        
        
        


        //TODO YAGNI add an ContainsAssignable function similar to containstype
        
        
        /// <summary>
        /// Checks the containedTypes list for duplicate types
        /// </summary>
        /// <param name="t">type to compare to conaintedtypes list</param>
        /// <returns> true if there is a duplicate type</returns>
        public bool ContainsAssignable(System.Type t)
        {
            System.Type st = null;
            for (int i=0; i<containedTypes.Count; i++)
            {
                st = containedTypes[i].SystemType;
                if (st == null) continue;
                //Debug.Log("Comparing: " + st + " and " + t);
                if (t.IsAssignableFrom(st))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Checks the containedTypes list for duplicate types
        /// </summary>
        /// <param name="t">type to compare to conaintedtypes list</param>
        /// <returns> true if there is a duplicate type</returns>
        public bool ContainsType(System.Type t)
        {
            System.Type st = null;
            for (int i=0; i<containedTypes.Count; i++)
            {
                st = containedTypes[i].SystemType;
                if (st == null) continue;
                //Debug.Log("Comparing: " + st + " and " + t);
                if (st == t)
                    return true;
            }
            return false;
        }
        
        public bool ContainsType(SerializableSystemType t)
        {
            SerializableSystemType st = null;
            for (int i=0; i<containedTypes.Count; i++)
            {
                st = containedTypes[i];
                if (st == null) continue;
                //Debug.Log("Checking if " + st.SystemType + " is the same as " + t.SystemType + " on " + this.name, this);
                if (st.SystemType == t.SystemType)                
                {
                    //Debug.Log("IT IS!!!!", this);
                    return true;
                }
                   
            }
            //Debug.Log("ITS NOT!!!!", this);
            return false;
        }

        /// <summary>
        /// populates the type list with only unique types from the table entries
        /// </summary>
        public List<SerializableSystemType> GetUniqueTypes()
        {        
            
           // Stopwatch sw = new Stopwatch();
          //  sw.Start();
            containedTypes.Clear();
            foreach (Entry e in tableEntries)
            {
                if (e == null) {  continue; }
                if(e==this)continue;
                
                Table table = e as Table;
                if (table != null)
                {
                    Debug.Log("Found a table: " + table.name, table);
                    foreach (SerializableSystemType sst in table.GetUniqueTypes())
                    {
                        if (ContainsType(sst)) continue;
                        Debug.Log("Found original type " + sst.SystemType);
                        containedTypes.Add(sst);
                    }
                }
                else
                {
                    if (ContainsType(e.GetType())) continue;
                    containedTypes.Add(new SerializableSystemType(e.GetType()));
                }
            }
            return containedTypes;
            //sw.Stop();
            // Debug.Log("Get types took " + sw.ElapsedTicks + " processor ticks.");
        }

        /// <summary>
        /// Checks if this table has the input entry
        /// </summary>
        /// <param name="e">the roll we are looking for</param>
        /// <returns>true if it has the entry</returns>
        public bool ContainsRoll(Entry e)
        {
            if (e == null) return false;
            foreach(TableEntry te in rollTable)
            {
                if (te.HasEntry(e))
                    return true;
            }
            return false;                
        }


        /// <summary>
        /// Type check for the roll table
        /// </summary>
        /// <param name="type">The type we are looking for </param>
        /// <returns>True if the roll table contains an entry of type</returns>
        public bool ContainsRollType(System.Type type)
        {
            foreach (TableEntry e in rollTable)
            {
                if (e.GetType() == typeof(NullEntry))
                    return true;
                if (type.IsAssignableFrom(e.EntryType()))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Resets all the droptimes
        /// </summary>
        public void ResetRollCounter()
        {
            foreach (TableEntry te in rollTable)
            {
                te.dropTimes = 0;
                te.totalResourcesSpent = 0;
            }
        }
        #endregion
        #region rolls
        /// <summary>
        /// Test Roll for the button
        /// </summary>
        public void TestRoll()
        {        
            lastRolledObject = Roll<Entry>();
        }   

       
    
        /// <summary>
        /// Default Roll function, returns an object with the correct type from the weighted list;
        /// </summary>
        public Entry Roll<T>() where T: Entry
        {
           Entry e =  Roll<T>(TypeList<T>(rollTable));
            if (e == null) return null;
            Table tableCatch = e as Table;
            if (tableCatch != null)
            {
                Debug.Log(e.name + "It was a table!", e);
               // return tableCatch.Roll<T>(rollTable);
            }
//            Debug.Log("returning entry: " + e, e);
            return e as T;
        }


        /// <summary>
        /// Returns an object from a filtered list of the rollTable based on the input tags
        /// </summary>    
         public Entry Roll<T>(System.Func<Entry,bool> query) where T : Entry
        {
            Entry e =  Roll<T>(QueryFilteredList<T>(query));
            if (e == null) return null;
            Table tableCatch = e as Table;
            if (tableCatch != null)
            {
               //Debug.Log(e.name + "It was a table!", e);
               return tableCatch.Roll<T>(query);
            }
            //Debug.Log("returning entry: " + e, e);
            return e as T;
        }
        
        

        /// <summary>
        /// Main Roll Function, checks target list of RollObjects for weight, and rolls through to find an object
        /// </summary>   
        Entry Roll<T>(List<TableEntry> drollObjects)where T : Entry
        {
//            Debug.Log(drollObjects.Count + " objects to roll against");
            if (drollObjects.Count < 1) return null;
            if (drollObjects.Count == 1)
            {
                Entry firstObject = drollObjects.First().entry;
               // Debug.Log(firstObject + "is the entry",firstObject);
                return firstObject;
            } 
      
            int allWeight = TotalWeight(drollObjects);
            //Debug.Log("Allweight is: " + allWeight + " out of " + drollObjects.Count);
            _lastRoll = Random.Range(1, allWeight + 1);
            _currentWeight = 0;
            for (int i = 0; i < drollObjects.Count; i++)
            {
                if (drollObjects[i] == null || drollObjects[i].entry == null) // if its an empty entry, ignore it
                {
                    //GameManager.DebugLog("found an empty drollObject" + i, this);
                    continue;
                }

                TableEntry currentObject = drollObjects[i];
                _currentWeight += currentObject.TotalWeight;
                if (_lastRoll <= _currentWeight)
                {
                   // GameManager.DebugLog("Rolled " + currentObject.entry.name + " out of a list of " + drollObjects.Count + 
                  //  "(Roll:" + _lastRoll +", CurrentWeight:" + _currentWeight + " / " + allWeight, currentObject.entry);

                    if (currentObject.guaranteeDrop != 0)
                        ResetGuarantee(drollObjects);
                    else
                        IncreaseGuarantee(drollObjects);
                  

                    currentObject.dropTimes++;
                    currentObject.totalResourcesSpent += currentObject.entry.Value();
                    return currentObject.entry;
                }
            }
            Debug.LogError("Roll Returned nothing, something went wrong (Roll:" + _lastRoll + ", CurrentWeight:" + 
                           _currentWeight + " / " + allWeight);
            return null;
        }
        #endregion

        #region guaranteedDrops

        /// <summary>
        /// Checks the input list for any Guaranteed drop items
        /// </summary>
        /// <param name="drollObjects"></param>
        /// <returns></returns>
        public bool HasGuaranteedRolls(List<TableEntry> drollObjects)
        {
            for(int i=0; i<drollObjects.Count; i++)
            {
                TableEntry te = drollObjects[i];
                if(te!=null&& te.guaranteeDrop != 0) return true;               
            }
            return false;
        }

        /// <summary>
        /// Increases the odds for all guaranteed drops in list to drop
        /// </summary>
        /// <param name="drollObjects"></param>
        void IncreaseGuarantee(List<TableEntry> drollObjects)
        {
            for (int i = 0; i < drollObjects.Count; i++)
            {
                TableEntry te = drollObjects[i];

                if (te == null) continue;
                if (te.guaranteeDrop != 0)
                {
                    int addWeight = (_totalWeight) / te.guaranteeDrop;
                    //Debug.Log("Adding " + addWeight);
                    te.guaranteeBonus += addWeight;
                }
                else
                    te.guaranteeBonus = 0;
            }
        }
       
        /// <summary>
        /// Resets the guarantees of the drops, used when a guaranteed drop has dropped
        /// </summary>
        /// <param name="drollObjects"></param>
        public void ResetGuarantee(List<TableEntry> drollObjects)
        {
            for (int i = 0; i < drollObjects.Count; i++)
            {
                TableEntry te = drollObjects[i];
                te.guaranteeBonus = 0;
            }
        }

        #endregion;





        #region filters
       
        
        /// <summary>
        /// Checks to see if we are filtering at all
        /// </summary>
        /// <returns></returns>
        public bool Filtering()
        {
            foreach (TypeToggle tt in typeList)
                if (tt.toggled) return true; 
            
            return false;
        }

        /// <summary>
        /// Sets the typelist to be true for each input type in the list
        /// </summary>
        /// <param name="tList"></param>
        public void SetTypeList(List<SerializableSystemType> tList)
        {            
            for(int i = 0; i<typeList.Count; i++)
            {
                TypeToggle tt = typeList[i];
                if(tList.Contains(tt.type))
                    tt.toggled = true;
            }
        }

        //resets the Type List
        void ResetTypeList()
        {
            for (int i = 0; i < typeList.Count; i++)
                typeList[i].toggled = false;
        }


        /// <summary>
        /// Filters the input table by the existing types in typeList
        /// </summary>
        /// <param name="rollList"></param>
        public List<TableEntry> TypeFilteredList(List<TableEntry> rollList)
        {          
            if (!Filtering()) return rollList;
            //Debug.Log("We are filtering ");
            List<TableEntry> returnList = new List<TableEntry>();

            foreach (TableEntry te in rollList)
            {
                if(te==null)continue;
                foreach (TypeToggle tt in typeList)
                {
                    if (tt == null) continue;
                    if (tt.toggled&&tt.type.SystemType.IsAssignableFrom(te.EntryType()))
                        returnList.Add(te);
                }
                    
            }
           // Debug.Log("returned " + returnList.Count + " from type filtering ");

            return returnList;

        }
        
        /// <summary>
        /// Filters the input table by the existing types in typeList
        /// </summary>
        /// <param name="rollList"></param>
         List<TableEntry> TypeList<T>(List<TableEntry> rollList) // TODO ADD OTHER LISTS OF TYPE ROLLING
        {          
          //  Debug.Log("We are filtering by type: " + typeof(T).ToString() + " on " + this.name,this);
            List<TableEntry> returnList = new List<TableEntry>();

            foreach (TableEntry te in rollList)
            {
                if(te==null)continue;
                Table t = te.entry as Table;
                if (t != null)
                {
                    if(t.ContainsAssignable(typeof(T)))
                        returnList.Add(te);
                }
                else
                {
                   if(typeof(T).IsAssignableFrom(te.EntryType()))
                        returnList.Add(te);
                }
            }
//            Debug.Log("returned " + returnList.Count +" / " + rollList.Count +  " when filtering for types: "+ typeof(T).ToString(),this);

            return returnList;

        }


        /// <summary>
        /// Returns a list filtered by all tags and types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        List<TableEntry> TypeAndQueryFilteredList<T>(System.Func<Entry, bool> query, List<SerializableSystemType> types) where T : Entry
        {
            List<TableEntry> filteredEntries = QueryFilteredList<T>(query, TypeList<T>(rollTable));
            return filteredEntries;

        }
        
        /// <summary>
        /// Filters the list by query
        /// </summary>
        /// <param name="query"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<TableEntry> QueryFilteredList<T>(System.Func<Entry, bool> query) where T : Entry
        {
            return QueryFilteredList<T>(query, rollTable);
        }

        List<TableEntry> QueryFilteredList<T>(System.Func<Entry, bool> query, List<TableEntry> rollList) where T : Entry
        {
            List<TableEntry> filteredList = new List<TableEntry>();
            for (int i = 0; i < rollList.Count; i++)
            {
                TableEntry rollObj = rollList[i];
                if (rollObj.entry == null) continue;               
                       
                //Input query from the roller
                if (rollObj.EntryType() == typeof(NullEntry) || query(rollObj.entry))
                {
                    //Debug.Log("Valid object: " + rollObj.entry.name + "  with " + rollObj.entry.tags.Count + " tags.");
                    
                    filteredList.Add(rollObj);
                }
            }
            //if (filteredList.Count < 1)
                //GameManager.DebugLog("Found no legal spawns according to the <color=green>RollQuery</color> in:<b> " + query.Target.GetType().ToString()+ "</b>", group: LogGroup.Tables);
            return filteredList;
        }

    }
    #endregion

}
