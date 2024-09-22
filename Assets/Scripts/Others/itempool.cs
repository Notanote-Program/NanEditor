using System.Collections.Generic;
using UnityEngine;

public class itempool
{
    public GameObject prefab;
    private GameObject pool_object;
    private List<GameObject> item_pool = new List<GameObject>();
    private int max_pool_size = 1 << 31; 
    private int enlarge_size = 256;
    private bool needRelease = false;
    public int set_item(string path, bool needRelease, GameObject parent = null)
    {
        this.needRelease = needRelease;
        if (parent == null)
            parent = new GameObject();
        pool_object = parent;
        prefab = Object.Instantiate(Resources.Load<GameObject>(path), parent.transform);
        //Debug.Log(prefab);
        if (prefab == null)
            return -1;
        prefab.SetActive(false);
        return 0;
    }

    public void init(int init_num = 100)
    {
        for(int i= 0; i < init_num; i++)
        {
            GameObject item = Object.Instantiate(prefab, pool_object.transform);
            item.SetActive(false);
            item_pool.Add(item);
        }
    }

    public GameObject get_item()
    {
        bool not_enough_item = true;
        foreach(GameObject item in item_pool)
        {
            if(!item.activeSelf)
            {
                not_enough_item = false;
                item.SetActive(true);
                return item;
            }
        }
        if(not_enough_item)
        {
            for (int i=0;i<enlarge_size;i++)
            {
                GameObject item = Object.Instantiate(prefab, prefab.transform.parent);
                item.SetActive(false);
                item_pool.Add(item);
            }                        
            GameObject selected_item = item_pool[item_pool.Count-1];
            selected_item.SetActive(true);
            return selected_item;
        }
        return null;
    }

    public void release_item(GameObject item)
    {
        item.SetActive(false);
        item.transform.SetParent(pool_object.transform, false);
        if (!needRelease) return;
        IReleasablePoolItem[] releasablePoolItems = item.GetComponents<IReleasablePoolItem>();
        foreach (IReleasablePoolItem releasablePoolItem in releasablePoolItems)
        {
            releasablePoolItem.OnRelease();
        }
    }
}
