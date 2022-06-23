using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class PObject : MonoBehaviour
{
    public int objectID;
    public int type;

    public Vector3[] points;

    //cluster
    public Vector3 centroid;
    public Vector3 velocity;
    public Vector3 minBounds;
    public Vector3 maxBounds;
    public int state;

    float lastUpdateTime;

    public float killDelayTime = 0;

    public float timeSinceGhost;
    public bool drawDebug;

    public enum PositionUpdateMode { None, Centroid, BoxCenter }
    public PositionUpdateMode posUpdateMode = PositionUpdateMode.Centroid;

    public enum CoordMode { Absolute, Relative }
    public CoordMode pointMode = CoordMode.Relative;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastUpdateTime > .5f) timeSinceGhost = Time.time;
        else timeSinceGhost = -1;
    }

    public void updateData(byte[] data)
    {
        var deltaTime = (Time.time - this.lastUpdateTime) / 1000.0;

        var verticesIndex = 5;

        if (this.type == 1) //cluster
        {
            state = BitConverter.ToInt32(data, 5);
            Vector3[] clusterData = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                int si = 9 + i * 12;

                Vector3 p = new Vector3(BitConverter.ToSingle(data, si), BitConverter.ToSingle(data, si + 4), BitConverter.ToSingle(data, si + 8));
                if (pointMode == CoordMode.Absolute) clusterData[i] = transform.parent.InverseTransformPoint(p);
                else clusterData[i] = p;
            }

            centroid = clusterData[0];
            velocity = clusterData[1];
            minBounds = clusterData[2];
            maxBounds = clusterData[3];


            verticesIndex += 4 + 12 * 4; //state = 4 bytes, boxMinMax = 6 * 4 bytes
        }

        switch (posUpdateMode)
        {
            case PositionUpdateMode.None:
                break;
            case PositionUpdateMode.Centroid:
                transform.localPosition = centroid;
                break;
            case PositionUpdateMode.BoxCenter:
                transform.localPosition = (minBounds + maxBounds) / 2;
                break;
        }

        int numPoints = (data.Length - verticesIndex) / 12;
        points = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            int si = verticesIndex + (i * 12);
            Vector3 p = new Vector3(BitConverter.ToSingle(data, si), BitConverter.ToSingle(data, si + 4), BitConverter.ToSingle(data, si + 8));
            if (pointMode == CoordMode.Absolute) points[i] = p;
            else points[i] = transform.parent.TransformPoint(p);
        }


       

        lastUpdateTime = Time.time;
    }

    public void kill()
    {
        if(killDelayTime == 0)
        {
            Destroy(gameObject);
            return;
        }

        points = new Vector3[0];
        StartCoroutine(killForReal(killDelayTime));
    }

    IEnumerator killForReal(float timeBeforeKill)
    {
        yield return new WaitForSeconds(timeBeforeKill);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if(drawDebug)
        {
            Color c = Color.HSVToRGB((objectID * .1f) % 1, 1, 1); //Color.red;// getColor();
            if (state == 3) c = Color.gray / 2;

            Gizmos.color = c;
            foreach (var p in points) Gizmos.DrawLine(p, p + Vector3.forward * .01f);

            Gizmos.color = c + Color.white * .3f;
            Gizmos.DrawWireSphere(centroid, .03f);
            Gizmos.DrawWireCube((minBounds + maxBounds) / 2, maxBounds - minBounds);
        }

    }
}
