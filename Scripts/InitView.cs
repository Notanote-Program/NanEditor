using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitView : MonoBehaviour
{
    // Start is called before the first frame update
    Animator animator;
    void Start()
    {
        init();
        StartCoroutine(enter_scene(1));// go to select view
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void init()
    {
        //init globals
        Config.GraphicQuality = Config.GraphicQuality;
        Config.antiAliasing = Config.antiAliasing;
        Config.dspBufferSize = Config.dspBufferSize;
        Application.targetFrameRate = -1;

        animator = GetComponent<Animator>();
    }
    IEnumerator enter_scene(int id,float delay = 3.0f)
    {
        animator.SetTrigger("enter");       
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(id);
    }
}
