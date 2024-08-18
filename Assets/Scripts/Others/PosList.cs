using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosList : MonoBehaviour
{
    private const int segnum = 100;
    public int selectedId;
    public Vector3 selectedPos;
    public Vector3 startPos
    {
        get { return startPosCeil.GetComponent<positionCeil>().pos; }
    }
    public Vector3 endPos
    {
        get { return endPosCeil.GetComponent<positionCeil>().pos; }
    }
    public float deltaPosX,deltaPosY;
    public GameObject ceil;
    public GameObject content;
    public List<GameObject> ceilList;
    public GameObject startPosCeil;
    public GameObject endPosCeil;
    public LineRenderer line;
    private bool _preview;
    private List<Vector3> posList;
    private Config.PathType pathType = Config.PathType.Straight;
    public void preview()
    {
        _preview = !_preview;
        if(_preview)
        {
            line.enabled = true;
            line.positionCount = segnum + 1;
            MoveableObject moveableObject = new MoveableObject();
            for(int i=0;i<=segnum;i++)
            {
                Vector3 worldpos;
                switch (pathType)
                {
                    case Config.PathType.Straight:
                        worldpos = moveableObject.myposition2world(Utilities.getStraightPosition((float)i / (segnum + 1), posList));
                        line.SetPosition(i, worldpos);
                        break;
                    case Config.PathType.Bessel:
                        worldpos = moveableObject.myposition2world(Utilities.getBesselPosition((float)i / (segnum + 1), posList));
                        line.SetPosition(i, worldpos);
                        break;
                }
                
            }
        }
        else
        {
            line.enabled = false;
        }
    }
    public void init()
    {
        selectedId = 0;
        selectedPos = Vector3.zero;
        ceilList = new List<GameObject>();
        posList = new List<Vector3>();
        _preview = false;
    }
    public void setPosList(List<Vector3> poslist,Config.PathType type = Config.PathType.Straight)
    {
        this.posList = poslist;
        this.pathType = type;
        foreach(GameObject obj in ceilList)
        {
            Destroy(obj);
        }
        ceilList.Clear();
        for(int i=0;i<poslist.Count;i++)
        {
            Vector3 pos = poslist[i];
            GameObject obj = GameObject.Instantiate(ceil, content.transform);
            obj.SetActive(true);
            obj.GetComponent<positionCeil>().init(i,pos.x, pos.y);
            obj.transform.localScale = ceil.transform.localScale;
            ceilList.Add(obj);
        }
    }
    public void setId(int id)
    {
        selectedId = id;
        selectedPos = ceilList[id].GetComponent<positionCeil>().pos;
    }
}
