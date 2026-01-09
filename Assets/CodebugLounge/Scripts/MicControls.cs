
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;



public class MicControls : UdonSharpBehaviour
{

    [SerializeField] Transform micTransform;
    float scale = 0.001f;
    void Start()
    {
        
    }

    public void Scale(float amount)
    {
        Debug.Log("Scaling by " + amount);
        micTransform.localScale = micTransform.localScale + Vector3.one * amount;
    }

    public void ScaleUp()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ScaleUpAll));
    }
    public void ScaleDown()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ScaleDownAll));
    }


    public void ScaleUpAll()
    {
        Debug.Log("Scaling Up");
        micTransform.localScale = micTransform.localScale + Vector3.one * scale;
    }
    public void ScaleDownAll()
    {
        Debug.Log("Scaling Down");
        micTransform.localScale = micTransform.localScale - Vector3.one * scale;
    }
}
