using UnityEngine;
using System.Collections;

namespace AP_enum {
	public enum EmptyBehavior { Grow, Fail, ReuseOldest }
	public enum MaxEmptyBehavior { Fail, ReuseOldest }
}

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    protected AutoPool.AP_Manager apManagerScript;
	
    void Awake()
    {
        apManagerScript = gameObject.AddComponent<AutoPool.AP_Manager>();
    }

    // may be be called early and won't create a spawn, but will create a pool reference and return true if the reference was created or already exsists.
    // use if you'd like to link pool references before the first spawn of a particular pool. (probably not necessary except for the most demanding of scenes.)
    // Additionaly, can be used to dynamically create pools at runtime.
    public bool InitializeSpawn(GameObject prefab)
    {
        return InitializeSpawn(prefab, 0f, 0);
    }

    public bool InitializeSpawn(GameObject prefab, int minPool)
    {
        return InitializeSpawn(prefab, 0f, minPool);
    }
    // parameters assigned can be used to create pools at runtime
    // if addPool is < 1, it will be used to increase the exsisting pool by a percentage. Otherwise it will round to the nearest integer and increase by that ammount
    // minPool is the min object that must be in that pool. If the current pool + addPool < minPool, minPool will be used
    public bool InitializeSpawn(GameObject prefab, float addPool, int minPool)
    {
        return InitializeSpawn(prefab, addPool, minPool, AP_enum.EmptyBehavior.Grow, AP_enum.MaxEmptyBehavior.Fail, false);
    }
    public bool InitializeSpawn(GameObject prefab, float addPool, int minPool, AP_enum.EmptyBehavior emptyBehavior, AP_enum.MaxEmptyBehavior maxEmptyBehavior)
    {
        return InitializeSpawn(prefab, addPool, minPool, emptyBehavior, maxEmptyBehavior, true);
    }
    bool InitializeSpawn(GameObject prefab, float addPool, int minPool, AP_enum.EmptyBehavior emptyBehavior, AP_enum.MaxEmptyBehavior maxEmptyBehavior, bool modBehavior)
    {
        if (prefab == null) { return false; } // object wasn't defined

        //		if ( apManagerScript == null ) { // object pool manager script not located yet
        //			apManagerScript = Object.FindObjectOfType<AP_Manager>(); // find it in the scene
        if (apManagerScript == null) { Debug.Log("No Object Pool Manager found in scene."); return false; } // didn't find an object pool manager
                                                                                                            //		}
                                                                                                            // found an object pool manager
        return apManagerScript.InitializeSpawn(prefab, addPool, minPool, emptyBehavior, maxEmptyBehavior, modBehavior);
    }

    // use to create a spawn of the obj prefab. returns the spawned object
    public GameObject Spawn(string name)
    { // spawns at the position and rotation of the pool
        return Spawn(name, null, null, Vector3.zero, Quaternion.identity, false);
    }
    public GameObject Spawn(string name, Transform parent)
    { // spawns at the position and rotation of the pool
        return Spawn(name, parent, null, Vector3.zero, Quaternion.identity, false);
    }
    public GameObject Spawn(string name, Transform parent, int? child)
    { // child allows a single object to hold multiple versions of objects, and only activate a specific child. null = don't use children
        return Spawn(name, parent, child, Vector3.zero, Quaternion.identity, false);
    }
    public GameObject Spawn(string name, Transform parent, Vector3 pos, Quaternion rot)
    { // specify a specific position and rotation
        return Spawn(name, parent, null, pos, rot, true);
    }
    public GameObject Spawn(string name, Transform parent, int? child, Vector3 pos, Quaternion rot)
    {
        return Spawn(name, parent, child, pos, rot, true);
    }
    GameObject Spawn(string name, Transform parent, int? child, Vector3 pos, Quaternion rot, bool usePosRot)
    {
        if (apManagerScript == null)
        { // didn't find an object pool manager
            return null;
        }
        else
        { // found an object pool manager
            return apManagerScript.Spawn(name, parent, child, pos, rot, usePosRot);
        }
    }

    public bool Despawn(GameObject obj)
    {
        if (obj == null) { return false; }
        return Despawn(obj.GetComponent<AutoPool.AP_Reference>(), -1f);
    }
    public bool Despawn(GameObject obj, float time)
    {
        if (obj == null) { return false; }
        return Despawn(obj.GetComponent<AutoPool.AP_Reference>(), time);
    }
    public bool Despawn(AutoPool.AP_Reference script)
    {
        return Despawn(script, -1f);
    }
    public bool Despawn(AutoPool.AP_Reference script, float time)
    {
        if (script == null) { return false; }
        return script.Despawn(time);
    }

    public int GetActiveCount(string name)
    {
        if (apManagerScript == null)
        { // didn't find an object pool manager
            return 0;
        }
        else
        {
            return apManagerScript.GetActiveCount(name);
        }
    }

    public int GetAvailableCount(string name)
    {
        if (apManagerScript == null)
        { // didn't find an object pool manager
            return 0;
        }
        else
        {
            return apManagerScript.GetAvailableCount(name);
        }
    }

    public bool DespawnPool(string name)
    {
        if (apManagerScript == null)
        { // didn't find an object pool manager
            return false;
        }
        else
        {
            return apManagerScript.DespawnPool(name);
        }
    }

    public bool DespawnAll()
    {
        if (apManagerScript == null)
        { // didn't find an object pool manager
            return false;
        }
        else
        {
            return apManagerScript.DespawnAll();
        }
    }

    public bool RemovePool(string name)
    {
        bool result = false;
        if (apManagerScript == null)
        { // didn't find an object pool manager
            return false;
        }
        else
        {
            result = apManagerScript.RemovePool(name);
            if (result == true) { apManagerScript.poolRef.Remove(name); }
            return result;
        }
    }

    public bool RemoveAll()
    {
        if (apManagerScript == null)
        { // didn't find an object pool manager
            return false;
        }
        else
        {
            return apManagerScript.RemoveAll();
        }
    }
}