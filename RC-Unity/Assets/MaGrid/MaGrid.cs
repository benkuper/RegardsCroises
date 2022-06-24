using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class MaGrid : MonoBehaviour
{
    [Serializable]
    class InitialData
    {
        public InitialData(Vector3 position, Vector3 rotation, Vector3 scale) { this.position = position; this.rotation = rotation; this.scale = scale; }
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    Dictionary<Transform, InitialData> initialData;

    public float smoothing;

    void Start()
    {
        initialData = new Dictionary<Transform, InitialData>();

    }

    void Update()
    {
        List<Transform> objects = new List<Transform>();
        GetComponentsInChildren<Transform>(false, objects);
        objects.Remove(transform);

        if (initialData == null) initialData = new Dictionary<Transform, InitialData>();

        foreach (Transform t in objects)
        {
            if (!initialData.ContainsKey(t))
            {
                initialData[t] = new InitialData(t.position, t.rotation.eulerAngles, t.localScale);
            }

            MaGridEffect[] effects = FindObjectsOfType<MaGridEffect>();

            Vector3 position = initialData[t].position;
            Vector3 rotation = initialData[t].rotation;
            Vector3 scale = initialData[t].scale;

            foreach (MaGridEffect effect in effects)
            {
                if (!effect.enabled || !effect.gameObject.activeInHierarchy) continue;
                effect.process(t, initialData[t].position, ref position, ref rotation, ref scale);
            }

            if (smoothing > 0)
            {
                float step = Mathf.Min(Time.deltaTime / smoothing, 1); ;
                t.position = Vector3.Lerp(t.position, position, step);
                Quaternion rot = Quaternion.Euler(rotation);
                t.rotation = Quaternion.Lerp(t.rotation, rot, step);
                t.localScale = Vector3.Lerp(t.localScale, scale, step);
            }
            else
            {
                t.position = position;
                t.rotation = Quaternion.Euler(rotation);
                t.localScale = scale;
            }
        }
    }
}
