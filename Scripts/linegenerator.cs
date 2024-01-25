using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class linegenerator : MonoBehaviour
{
    List<Vector3> positions = new List<Vector3>();
    LineRenderer line;
    public int num = 100;
    private float radius = 1;
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        for(int i=0;i<=num;i++)
        {
            positions.Add(new Vector3(radius * Mathf.Sin(2 * Mathf.PI / num * i), radius * Mathf.Cos(2 * Mathf.PI / num * i), 0));
        }
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.positionCount = num+1;
        line.SetPositions(positions.ToArray());
    }

    // Update is called once per frame
    void Update()
    {
        radius = Mathf.Abs(Mathf.Sin(Time.time)*10);
        positions.Clear();
        for (int i = 0; i <= num; i++)
        {
            positions.Add(new Vector3(radius * Mathf.Sin(2 * Mathf.PI / num * i), radius * Mathf.Cos(2 * Mathf.PI / num * i), 0));
        }
        line.positionCount = num+1;
        line.SetPositions(positions.ToArray());
    }
}
