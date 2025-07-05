using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    public string assetLabelString;

    internal void SetSelection(bool v)
    {
        GetComponent<Collider>().enabled = !v;
        GetComponent<MeshRenderer>().material.color = v ? Color.green : Color.white;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public FurnitureDescriptor GetFurnitureDescriptor()
    {
        return new FurnitureDescriptor() { assetLabelString = this.assetLabelString, position = transform.position };
    }
}

