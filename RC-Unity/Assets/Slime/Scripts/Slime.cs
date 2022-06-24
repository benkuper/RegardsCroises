using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour
{
    public ComputeShader shader;

    public int width = 1920, height = 1080;
    public int depth = 1;
    public int numAgents = 1000000;
    [Range(0, 500)]
    public float moveSpeed = 50.0f;
    [Range(0, 100)]
    public float diffuseSpeed = 10.0f;
    [Range(0, 10)]
    public float evaporateSpeed = 0.3f;

    [Range(0, 50)]
    public int senseRange = 3;
    [Range(0, 100)]
    public float sensorLength = 8.0f;
    [Range(0, 180)]
    public float sensorAngleSpacing = 30.0f;
    [Range(0, 180)]
    public float turnSpeed = 50.0f;
    public float marchingError = 0.1f;

    public Vector2 initPos = Vector2.one * .5f;
    public float initRadius = 0.1f;

    public Texture ghostTrailMap;
    private RenderTexture trailMap;
    private RenderTexture trailMapProcessed;
    private ComputeBuffer agentsBuffer;


    //[Range(0, 1)]
    //public float sourceWeight = 1;
    [Range(0, 1)]
    public float ghostWeight;

    [Range(0, 1)]
    public float agentOpacity = .2f;

    [Range(-20, 20)]
    public float ghostSensorLengthFactor = 0;

    [Range(-1,1)]
    public float ghostOpacityFactor = 0;

    [Range(-100,100)]
    public float ghostSpeedFactor = 0;

    private Dictionary<string, int> kernelIndices;

    public Transform[] targets;

    public bool resetPoint;

    public struct Agent
    {
        public Vector2 position;
        public float angle;
        public Vector4 type;
    } // size = 7 * 4 bytes

    private Agent[] agents;

    void Start()
    {
        kernelIndices = new Dictionary<string, int>();
        kernelIndices.Add("Update", shader.FindKernel("Update")); // Thread Shape [16, 1, 1]
        kernelIndices.Add("Postprocess", shader.FindKernel("Postprocess")); // Thread Shape [8, 8, 1]

        createNewTexture(ref trailMap);

        agents = new Agent[numAgents];

        agentsBuffer = new ComputeBuffer(agents.Length, sizeof(float) * 7);
        reset();

    }

    public void reset()
    {
        for (int i = 0; i < agents.Length; i++)
        {
            float angle = Random.Range(0, 2 * Mathf.PI);
            float len = Random.value * height * initRadius / 2.0f;
            float x = Mathf.Cos(angle) * len;
            float y = Mathf.Sin(angle) * len;

            agents[i].position = new Vector2(width * initPos.x + x, height * initPos.y + y);
            agents[i].angle = angle + Mathf.PI;

            Vector4 type = Vector4.zero;
            type[0] = 1;// Random.Range(0, 3)] = 1f;
            agents[i].type = type;
        }
        agentsBuffer.SetData(agents);
    }

    private void Update()
    {
        if(resetPoint)
        {
            resetPoint = false;
            reset();
        }
    }

    void FixedUpdate()
    {

        shader.SetTexture(kernelIndices["Update"], "TrailMap", trailMap);
        if (ghostTrailMap != null)
        {
            shader.SetTexture(kernelIndices["Update"], "GhostTrailMap", ghostTrailMap);
            shader.SetVector("ghostSize", new Vector4(ghostTrailMap.width, ghostTrailMap.height, 0));
            shader.SetFloat("ghostWeight", ghostWeight);
        }
        //shader.SetFloat("sourceWeight", sourceWeight);

        shader.SetFloat("agentOpacity", agentOpacity);

        shader.SetInt("width", width);
        shader.SetInt("height", height);
        shader.SetInt("numAgents", numAgents);
        shader.SetFloat("moveSpeed", moveSpeed);
        shader.SetFloat("deltaTime", Time.fixedDeltaTime);

        shader.SetInt("senseRange", senseRange);
        shader.SetFloat("sensorLength", sensorLength);
        shader.SetFloat("sensorAngleSpacing", sensorAngleSpacing * Mathf.Deg2Rad);
        shader.SetFloat("turnSpeed", turnSpeed);
        shader.SetFloat("marchingError", marchingError);
        shader.SetBuffer(kernelIndices["Update"], "agents", agentsBuffer);


        shader.Dispatch(kernelIndices["Update"], numAgents / 16, 1, 1);

        createNewTexture(ref trailMapProcessed);

        shader.SetFloat("evaporateSpeed", evaporateSpeed);
        shader.SetFloat("diffuseSpeed", diffuseSpeed);
        shader.SetTexture(kernelIndices["Postprocess"], "TrailMap", trailMap);
        shader.SetTexture(kernelIndices["Postprocess"], "TrailMapProcessed", trailMapProcessed);


        //targets
        Vector2[] pos = new Vector2[4];
        float[] posWeights = new float[4];

        for (int i = 0; i < 4; i++)
        {

            if (i >= targets.Length || targets[i] == null)
            {
                posWeights[i] = 0;
                continue;
            }

            Vector3 p = targets[i].transform.position;
            Vector2 norm = new Vector2(p.x / 36 + .5f, p.z / 14 + .5f);
            pos[i] = new Vector2(norm.x, norm.y);
        }


        shader.SetFloat("targetWeight", posWeights[0]);
        shader.SetFloat("ghostSensorLengthFactor", ghostSensorLengthFactor);
        shader.SetFloat("ghostAgentOpacityFactor", ghostOpacityFactor);
        shader.SetFloat("ghostSpeedFactor", ghostSpeedFactor);
        shader.SetVector("targetPos", new Vector4(pos[0].x, pos[0].y));

        shader.Dispatch(kernelIndices["Postprocess"], width / 8, height / 8, 1);

        trailMap.Release();
        trailMap = trailMapProcessed;

        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_BaseTexture", trailMap);
    }

    private void createNewTexture(ref RenderTexture renderTexture)
    {
        renderTexture = new RenderTexture(width, height, depth);
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();
    }

    private void OnDestroy()
    {
        trailMap.Release();
        trailMapProcessed.Release();
        agentsBuffer.Release();
    }
}
