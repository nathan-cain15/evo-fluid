using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

public class Joint : MonoBehaviour
{
    public Entity entity;
    public Joint parentJoint;
    public int jointId;
    public Vector3 originalPosition;
    public int energy = 100000;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
