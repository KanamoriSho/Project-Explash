using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laser_looks : MonoBehaviour
{
    public Texture[] BeamFrames; // Animation frame sequence
    public bool AnimateUV; // UV Animation

    LineRenderer lineRenderer; // Line rendered component

    [SerializeField]
    private float beamLength; // Current beam length
    float initialBeamOffset; // Initial UV offset 
    void Awake()
    {
        // Get line renderer component
        lineRenderer = GetComponent<LineRenderer>();

        // Assign first frame texture
        if (!AnimateUV && BeamFrames.Length > 0)
            lineRenderer.material.mainTexture = BeamFrames[0];

        // Randomize uv offset
        initialBeamOffset = Random.Range(0f, 5f);
    }

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer.SetPosition(1, new Vector3(0f, 0f, beamLength));
    }
}
