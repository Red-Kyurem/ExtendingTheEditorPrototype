using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateBrush : MonoBehaviour
{
    public float time = 1;
    public GameObject brush;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("CreateBrush", time);
    }

    void CreateBrush()
    {
        Instantiate(brush);
    }
}
