using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audiotest : MonoBehaviour
{
    public float delay;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        test();
    }
    private void test()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            //StartCoroutine(_playAudio(delay));
            playAudio();
        }
    }
    private IEnumerator _playAudio(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        this.GetComponent<AudioSource>().time = 0;
        this.GetComponent<AudioSource>().Play();
    }
    private void playAudio()
    {
        this.GetComponent<AudioSource>().time = 0;
        this.GetComponent<AudioSource>().Play();
    }
}
