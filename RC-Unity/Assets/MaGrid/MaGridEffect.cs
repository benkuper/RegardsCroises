using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class MaGridEffect : MonoBehaviour
{

    Dictionary<Transform, float> timesAtEnter;

    public enum Mode { Sphere, Axis };

    [Header("Effector")]

    public Mode mode;

    [ConditionalField("mode", false, Mode.Sphere)]
    public float radius = 1;

    [ConditionalField("mode", false, Mode.Axis)]
    public Vector3 axis;

    public AnimationCurve decayCurve;

    [Range(0, 1)]
    public float weight = 1;

    public bool absolute;
    public bool invert;

    [Header("Transform Effect")]

    public Vector3 offset = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public float radialPush = 0;

    [Header("Animation Effect")]
    public Vector3 rotationSpeed;


    [Header("Debug")]
    public bool alwaysDrawDebug = true;
    public Color debugColor = Color.yellow;


    public void process(Transform t, Vector3 initialPosition, ref Vector3 _position, ref Vector3 _rotation, ref Vector3 _scale)
    {
        float w = this.weight;

        if(timesAtEnter == null) timesAtEnter  = new Dictionary<Transform, float>();


        if (mode == Mode.Sphere)
        {
            float relDist = Vector3.Distance(initialPosition, transform.position) / radius;
            w *= decayCurve.Evaluate(relDist);
            w = Mathf.Clamp01(w);
        }

        if (invert) w = 1 - w;

        if(w == 0)
        {
            timesAtEnter.Remove(t);
            return;
        }

        if (!timesAtEnter.ContainsKey(t)) timesAtEnter[t] = Time.time;

        float relTime = Time.time - timesAtEnter[t];

        Vector3 targetPosition = offset + Vector3.Normalize(initialPosition - transform.position) * radialPush;
        _position = Vector3.Lerp(_position, (absolute ? Vector3.zero : _position) + targetPosition, w);
        _rotation = Vector3.Lerp(_rotation, (absolute ? Vector3.zero : _rotation) + rotation + rotationSpeed * relTime, w);
        _scale = Vector3.Lerp(_scale, Vector3.Scale(absolute ? Vector3.one : _scale, scale), w);
    }

    private void OnDrawGizmos()
    {
        if (alwaysDrawDebug) OnDrawGizmosSelected();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = debugColor;

        if (mode == Mode.Sphere)
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
        else
        {
        }
    }
}
