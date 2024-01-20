using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class positionCeil : MonoBehaviour
{
    public int id;
    public GameObject posList;
    public InputField posX,posY;
    public Vector3 pos;
    public void init(int id,float x = 0,float y = 0)
    {
        this.id = id;
        pos = new Vector3(x, y, 0);
        posX.text = x.ToString();
        posY.text = y.ToString();
    }
    // Start is called before the first frame update
    public void setId()
    {
        posList.GetComponent<PosList>().setId(id);
    }
    public void setPosX(string s)
    {
        if (s == "")
            return;
        pos.x = float.Parse(s);
    }
    public void setPosY(string s)
    {
        if (s == "")
            return;
        pos.y = float.Parse(s);
    }
}
