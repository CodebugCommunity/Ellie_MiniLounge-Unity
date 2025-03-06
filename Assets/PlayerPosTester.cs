using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class PlayerPosTester : MonoBehaviour
{
    [SerializeField] private Renderer grassRenderer;
    Vector4[] positions = new Vector4[20];
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            positions[i] = transform.GetChild(i).position;
        }

        grassRenderer.material.SetVectorArray("_PlayerPositions", positions);

    }
}
