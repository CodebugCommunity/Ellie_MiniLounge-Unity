
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class BeansController : UdonSharpBehaviour
{
    [SerializeField] private GameObject ObjectToToggle;
    void Start()
    {

    }


    public override void Interact()
    {
        if (ObjectToToggle.activeSelf)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(DisableLid));
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(EnableLid));
        }

        
    }

    void DisableLid()
    {
        ObjectToToggle.SetActive(false);
    }

    void EnableLid()
    {
        ObjectToToggle.SetActive(true);
    }


}
