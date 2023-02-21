using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muscle : MonoBehaviour
{
    public Bone firstBone;
    public Bone secondBone;
    public LineRenderer LineRenderer;
    public float timeScale;
    public float force;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Push(float scaleForce)
    {
        var centerPosition = (firstBone.transform.position + secondBone.transform.position) / 2;

        var firstBoneForce = (firstBone.transform.position - centerPosition).normalized;
        var secondBoneForce = (secondBone.transform.position - centerPosition).normalized;

        Vector3 scale = new Vector3(scaleForce, scaleForce, scaleForce);
        
        firstBoneForce.Scale(scale);
        secondBoneForce.Scale(scale);

        firstBone.Rigidbody2D.AddForceAtPosition(firstBoneForce, firstBone.transform.position);
        secondBone.Rigidbody2D.AddForceAtPosition(secondBoneForce, secondBone.transform.position);
    }

    public void Pull(float scaleForce)
    {
        var centerPosition = (firstBone.transform.position + secondBone.transform.position) / 2;

        var firstBoneForce = (centerPosition - firstBone.transform.position).normalized;
        var secondBoneForce = (centerPosition - secondBone.transform.position).normalized;

        Vector3 scale = new Vector3(scaleForce, scaleForce, scaleForce);
        firstBoneForce.Scale(scale);
        secondBoneForce.Scale(scale);
        
        firstBone.Rigidbody2D.AddForceAtPosition(firstBoneForce, firstBone.transform.position);
        secondBone.Rigidbody2D.AddForceAtPosition(secondBoneForce, secondBone.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
