using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using System;

public class PleaidesClient : MonoBehaviour
{
    public string ipAdress = "127.0.0.1";
    public int port = 6060;

    WebSocket websocket;

    Dictionary<int, PObject> objects;
    public GameObject[] objectPrefabs;

    [Range(0,1)]
    public float[] probas;

    async void Start()
    {
        objects = new Dictionary<int, PObject>();

        websocket = new WebSocket("ws://"+ ipAdress + ":" + port);
        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            //Debug.Log("OnMessage!");

            processData(bytes);
        };


        await websocket.Connect();
    }

    // Update is called once per frame
    /*async*/ void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif

        List<int> objectsToRemove = new List<int>();
        foreach (var o in objects.Values) if (o.timeSinceGhost > 1) objectsToRemove.Add(o.objectID);
        foreach (var oid in objectsToRemove)
        {
            Destroy(objects[oid].gameObject);
            objects.Remove(oid);
        }
    }


    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    void processData(byte[] data)
    {
        var type = data[0];

        if (type == -1) //dataType CLEAR
        {
            foreach (var op in objects) Destroy(op.Value);
            return;
        }

        int objectID = BitConverter.ToInt32(data, 1);

        PObject o = null;
        if(objects.ContainsKey(objectID)) o = objects[objectID];
        if (o == null)
        {
            GameObject prefab = objectPrefabs[0];
            float totalProba = 0;
            foreach (var p in probas) totalProba += p;
            float r = UnityEngine.Random.value * totalProba;

            for(int i=0;i<probas.Length;i++)
            {
                if(r <= probas[i])
                {
                    prefab = objectPrefabs[i];
                    break;
                }
            }
           

            o = Instantiate(prefab).GetComponent<PObject>();
            o.objectID = objectID;
            o.type = type;
            objects.Add(objectID, o);
            o.transform.parent = transform;
        }

        if (type == 0) //cloud
        {
            o.updateData(data);
        }
        else if (type == 1) //cluster
        {
            var state = BitConverter.ToInt32(data, 5);
            if (state == 2) //Will leave
            {
                objects[objectID].kill();
                objects.Remove(objectID);
            }
            else
            {
                o.updateData(data);
            }
        }
    }
}