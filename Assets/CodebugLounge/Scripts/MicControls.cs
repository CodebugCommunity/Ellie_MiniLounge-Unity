
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


public class MicControls : UdonSharpBehaviour
{

    [SerializeField] Transform micTransform;
    float scale = 0.001f;
    void Start()
    {
        
    }

    public void ScaleUp()
    {
        Debug.Log("Scaling Up");
        micTransform.localScale = micTransform.localScale + Vector3.one * scale;
    }
    public void ScaleDown()
    {
        Debug.Log("Scaling Down");
        micTransform.localScale = micTransform.localScale - Vector3.one * scale;
    }
}
