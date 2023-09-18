using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCanvas : MonoBehaviour
{
    new Camera camera;

    void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        transform.forward = camera.transform.forward;
    }
}
