using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class biasDisplay : MonoBehaviour
{
    public GameObject pointer;
    public GameObject PerfectLine;
    public GameObject GoodLine;
    public GameObject BadLine;
    public int max_num = 50;
    public float length = 200;
    public float height = 2;
    public float fadeDuration = 1;
    private int num = 0;
    // Start is called before the first frame update
    void Start()
    {
        init();
    }
    void init()
    {
        BadLine.GetComponent<RectTransform>().sizeDelta = new Vector2(length,height);
        GoodLine.GetComponent<RectTransform>().sizeDelta = new Vector2(length * Config.range_normal.good_duration / Config.range_normal.bad_duration, height);
        PerfectLine.GetComponent<RectTransform>().sizeDelta = new Vector2(length * Config.range_normal.perfect_duration / Config.range_normal.bad_duration, height);
        Debug.Log(PerfectLine.GetComponent<Image>());
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void addPointer(float time)
    {
        if (num >= max_num)
            return;
        num++;
        Debug.Log(time);
        StartCoroutine(setPointer(time));
    }
    private IEnumerator setPointer(float time)
    {
        GameObject newpointer = Instantiate(pointer);        
        newpointer.GetComponent<RectTransform>().SetParent(this.gameObject.GetComponent<RectTransform>());
        Color color = getColor(time);
        newpointer.GetComponent<Image>().color = color;
        newpointer.GetComponent<Transform>().localScale = pointer.GetComponent<Transform>().localScale;
        newpointer.GetComponent<RectTransform>().localPosition = new Vector2(getPosition(time),0);
        for(int i=0;i<10;i++)
        {
            newpointer.GetComponent<Image>().color = new Color(color.r,color.g,color.b,color.a * (10-i) / 10);
            yield return new WaitForSeconds(fadeDuration / 10);
        }
        Destroy(newpointer);
        num--;
    }
    private Color getColor(float time)
    {
        time = Mathf.Abs(time);
        if (time > Config.range_normal.bad_duration)
            return BadLine.GetComponent<Image>().color;
        else if (time > Config.range_normal.good_duration)
            return BadLine.GetComponent<Image>().color;
        else if (time > Config.range_normal.perfect_duration)
            return GoodLine.GetComponent<Image>().color;
        else
            return PerfectLine.GetComponent<Image>().color;
    }
    private float getPosition(float time)
    {
        return Mathf.Clamp(time / Config.range_normal.bad_duration * length/2,-length/2,length/2);
    }
}
