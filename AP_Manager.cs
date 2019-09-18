using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AutoPool
{
    public class AP_Manager : MonoBehaviour
    {

        public bool allowCreate = true;
        public bool allowModify = true;

        [Tooltip("When the scene is stopped, creates a report showing pool usage:\n\n" +
            "Start Size - Size of pool when beginning the scene.\n\n" +
            "Init Added - Number of objects added by InitializeSpawn() at runtime.\n\n" +
            "Grow Objects - Number of objects added with EmptyBehavior.Grow.\n\n" +
            "End Size - Total objects of this pool, active and inactive, at the time of the log report.\n\n" +
            "Failed Spawns - Number of Spawn() requests that didn't return a spawn.\n\n" +
            "Reused Objects - Number of times an object was reused before despawning normally.\n\n" +
            "Most Objects Active - The most items for this pool active at once.")]
        public bool printAllLogsOnQuit;

        //[HideInInspector] public Dictionary<GameObject, AP_Pool> poolRef;
        [HideInInspector] public Dictionary<string, AP_Pool> poolRef;

        void Awake()
        {
            CheckDict();
        }

        private void OnDestroy()
        {
            RemoveAll();
        }

        void CheckDict()
        {
            if (poolRef == null)
            { // dictionary hasn't been created yet
              //poolRef = new Dictionary<GameObject, AP_Pool>();
                poolRef = new Dictionary<string, AP_Pool>();
            }
        }

        public bool InitializeSpawn(GameObject prefab, float addPool, int minPool, AP_enum.EmptyBehavior emptyBehavior, AP_enum.MaxEmptyBehavior maxEmptyBehavior, bool modBehavior)
        {
            if (prefab == null) { return false; }
            CheckDict();
            bool result = false;
            bool tempModify = false;

            if (poolRef.ContainsKey(prefab.name) == true && poolRef[prefab.name] == null)
            { // check for broken reference
                poolRef.Remove(prefab.name); // remove it
            }
            if (poolRef.ContainsKey(prefab.name) == true)
            {
                result = true; // already have refrence
            }
            else
            {
                if (MakePoolRef(prefab.name) == null)
                { // ref not found
                    if (allowCreate == true)
                    {
                        CreatePool(prefab, 0, 0, emptyBehavior, maxEmptyBehavior);
                        tempModify = true; // may modify a newly created pool
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = true; // ref was created
                }
            }

            if (result == true)
            { // hava a valid pool ref
                if (allowModify == true || tempModify == true)
                { // may modify a newly created pool
                    if (addPool > 0 || minPool > 0)
                    {
                        int size = poolRef[prefab.name].poolBlock.size;
                        int l1 = 0; int l2 = 0;
                        if (addPool >= 0)
                        { // not negative
                            if (addPool < 1)
                            { // is a percentage
                                l2 = Mathf.RoundToInt(size * addPool);
                            }
                            else
                            { // not a percentage
                                l1 = Mathf.RoundToInt(addPool);
                            }
                        }
                        int loop = 0;
                        int a = size == 0 ? 0 : Mathf.Max(l1, l2);
                        if (size < minPool) { loop = minPool - size; }
                        loop += a;
                        for (int i = 0; i < loop; i++)
                        {
                            poolRef[prefab.name].CreateObject(true);
                        }
                        poolRef[prefab.name].poolBlock.maxSize = poolRef[prefab.name].poolBlock.size * 2;
                        if (modBehavior == true)
                        {
                            poolRef[prefab.name].poolBlock.emptyBehavior = emptyBehavior;
                            poolRef[prefab.name].poolBlock.maxEmptyBehavior = maxEmptyBehavior;
                        }
                    }
                }
            }

            return result;
        }

        public GameObject Spawn(string name, Transform parent, int? child, Vector3 pos, Quaternion rot, bool usePosRot)
        {
            CheckDict();

            if (poolRef.ContainsKey(name) == true)
            { // reference already created
                if (poolRef[name] != null)
                { // make sure pool still exsists
                    return poolRef[name].Spawn(parent, child, pos, rot, usePosRot); // create spawn
                }
                else
                { // pool no longer exsists
                    poolRef.Remove(name); // remove reference
                    return null;
                }
            }
            else
            { // ref not yet created
                AP_Pool childScript = MakePoolRef(name); // create ref
                if (childScript == null)
                { // ref not found
                    return null;
                }
                else
                {
                    return childScript.Spawn(parent, child, pos, rot, usePosRot); // create spawn
                }
            }
        }

        AP_Pool MakePoolRef(string name)
        { // attempt to create and return script reference
            for (int i = 0; i < transform.childCount; i++)
            {
                AP_Pool childScript = transform.GetChild(i).GetComponent<AP_Pool>();
                if (childScript && name.Equals(childScript.poolBlock.prefab.name))
                {
                    poolRef.Add(name, childScript);
                    return childScript;
                }
            }
            //		Debug.Log( obj.name + ": Tried to reference object pool, but no matching pool was found." );
            return null;
        }

        public int GetActiveCount(string name)
        {
            AP_Pool childScript = null;
            if (poolRef.ContainsKey(name) == true)
            { // reference already created
                childScript = poolRef[name];
            }
            else
            { // ref not yet created
                childScript = MakePoolRef(name); // create ref
            }
            if (childScript == null)
            { // pool not found
                return 0;
            }
            else
            {
                return childScript.poolBlock.size - childScript.pool.Count;
            }
        }

        public int GetAvailableCount(string name)
        {
            AP_Pool childScript = null;
            if (poolRef.ContainsKey(name) == true)
            { // reference already created
                childScript = poolRef[name];
            }
            else
            { // ref not yet created
                childScript = MakePoolRef(name); // create ref
            }
            if (childScript == null)
            { // pool not found
                return 0;
            }
            else
            {
                return childScript.pool.Count;
            }
        }

        public bool RemoveAll()
        {
            bool result = true;
            string[] temps = new string[poolRef.Count];
            int i = 0;
            foreach (string name in poolRef.Keys)
            {
                if (poolRef[name] != null)
                {
                    temps[i] = name;
                    i++;
                }
            }
            for (int t = 0; t < temps.Length; t++)
            {
                if (temps[t] != null)
                {
                    if (RemovePool(temps[t]) == false) { result = false; }
                }
            }
            return result;
        }

        public bool DespawnAll()
        {
            bool result = true;
            foreach (string name in poolRef.Keys)
            {
                if (DespawnPool(name) == false) { result = false; }
            }
            return result;
        }

        public bool RemovePool(string name)
        {
            bool result = false;
            AP_Pool childScript = null;
            if (poolRef.ContainsKey(name) == true)
            { // reference already created
                childScript = poolRef[name];
            }
            else
            { // ref not yet created
                childScript = MakePoolRef(name); // create ref
            }
            if (childScript == null)
            { // pool not found
                return false;
            }
            else
            {
                result = DespawnPool(name);
                Destroy(childScript.gameObject);
                poolRef.Remove(name);
                return result;
            }
        }

        public bool DespawnPool(string name)
        {
            AP_Pool childScript = null;
            if (poolRef.ContainsKey(name) == true)
            { // reference already created
                childScript = poolRef[name];
            }
            else
            { // ref not yet created
                childScript = MakePoolRef(name); // create ref
            }
            if (childScript == null)
            { // pool not found
                return false;
            }
            else
            {
                for (int i = 0; i < childScript.masterPool.Count; i++)
                {
                    childScript.Despawn(childScript.masterPool[i].obj, childScript.masterPool[i].refScript);
                }
                return true;
            }
        }

        public void CreatePool()
        {
            CreatePool(null, 32, 64, AP_enum.EmptyBehavior.Grow, AP_enum.MaxEmptyBehavior.Fail);
        }
        public void CreatePool(GameObject prefab, int size, int maxSize, AP_enum.EmptyBehavior emptyBehavior, AP_enum.MaxEmptyBehavior maxEmptyBehavior)
        {
            GameObject obj = new GameObject("Object Pool");
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            AP_Pool script = obj.AddComponent<AP_Pool>();
            if (Application.isPlaying == true)
            {
                obj.name = prefab.name;
                script.poolBlock.size = size;
                script.poolBlock.maxSize = maxSize;
                script.poolBlock.emptyBehavior = emptyBehavior;
                script.poolBlock.maxEmptyBehavior = maxEmptyBehavior;
                script.poolBlock.prefab = prefab;
                if (prefab) { MakePoolRef(prefab.name); }
            }
        }

        void OnApplicationQuit()
        {
            if (printAllLogsOnQuit == true)
            {
                PrintAllLogs();
            }
        }

        public void PrintAllLogs()
        {
            foreach (AP_Pool script in poolRef.Values)
            {
                script.PrintLog();
            }
        }
    }
}
