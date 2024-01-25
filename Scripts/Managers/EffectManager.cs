using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EffectManager : BaseManager
{
    private const string judgeline_prefab_path = "Prefabs/tapeffect";
    public void init(int num, GameObject parent = null)
    {
        initPool(judgeline_prefab_path, num, parent);
    }
    public void show(int type, Vector3 position, Color color, float delay = 0, GameObject parent = null)
    {
        //Debug.Log("showeffect");
        delay = Mathf.Max(0, delay)/1000.0f;
        StartCoroutine(showeffect(type, position, color, delay, parent));
    } 
    private IEnumerator showeffect(int type, Vector3 position, Color color, float delay = 0, GameObject parent = null)
    {
        if(delay != 0)
            yield return new WaitForSeconds(delay);
        GameObject effect = pool.get_item();
        effect.GetComponent<EffectRenderer>().init(color, position);
        if(parent != null)
            effect.transform.parent = parent.transform;
        effect.GetComponent<AudioSource>().volume = Config.keyVolume * Config.defaultKeyVolume;
        effect.GetComponent<AudioSource>().clip = type switch
        {
            0 => Config.tapSound,
            1 => Config.dragSound,
            _ => throw new ArgumentException()
        };
        effect.GetComponent<AudioSource>().time = 0;
        effect.GetComponent<AudioSource>().Play();
        Debug.Log("play tap sound,time =" + Time.time);
        yield return new WaitForSeconds(effect.GetComponent<EffectRenderer>().duration);
        pool.release_item(effect);
    }
}
