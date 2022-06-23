using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CubeVFXManager : MonoBehaviour
{
    public Transform sizeEffector;
    public Vector3 sizeEffectorOffset;
    private VisualEffect _vfx;

    PObject tracker;

    // Start is called before the first frame update
    void Start()
    {
        tracker = GetComponent<PObject>();
        _vfx = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        sizeEffector.localPosition = transform.localPosition + sizeEffectorOffset;

        if(_vfx.HasInt("Points"))
            _vfx.SetInt("Points", tracker.points.Length);
    }
}
