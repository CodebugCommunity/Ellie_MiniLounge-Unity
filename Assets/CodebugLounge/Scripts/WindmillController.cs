
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Random = UnityEngine.Random;

public class WindmillController : UdonSharpBehaviour
{
    [SerializeField] Transform[] windmillBlades;
    [SerializeField] float speedScale = 1;

    private void Start()
    {
        foreach (var windmillBlade in windmillBlades)
        {
            windmillBlade.Rotate(Vector3.right* Random.Range(0,360), Space.Self);
        }
    }

    private void Update()
    {
        foreach (var windmillBlade in windmillBlades)
        {
            windmillBlade.Rotate(Vector3.right * Time.deltaTime * 100 * speedScale, Space.Self);
        }
    }
}
