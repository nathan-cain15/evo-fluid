using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Bone : MonoBehaviour
{
    public Joint firstJoint;
    public Joint secondJoint;
    public Entity entity;
    public int firstAngle;
    public float length;
    public Vector3 firstEnd;
    public Vector3 secondEnd;
    public Rigidbody2D Rigidbody2D;
    public float viscosityDrag = 0.01f;
    public int num;
    public float surfaceAreaX;
    public float surfaceAreaY;
    
    //Start is called before the first frame update

    public void CalculateViscosityVelocity()
    {
	    // Cache positive axis vectors:
	    Vector3 up = Rigidbody2D.transform.up;
		Vector3 right = Rigidbody2D.transform.right;
		// Find centers of each of box's faces
		Vector3 xpos_face_center = (right * transform.localScale.x / 2f) + transform.position;
		Vector3 ypos_face_center = (up * Rigidbody2D.transform.localScale.y / 2f) + Rigidbody2D.transform.position;
	
		Vector3 xneg_face_center = -(right * transform.localScale.x / 2f) + transform.position;
		Vector3 yneg_face_center = -(up * transform.localScale.y / 2f) + transform.position;
		
		// TOP (posY):
		Vector3 pointVelPosY = Rigidbody2D.GetPointVelocity (ypos_face_center);
		float velPosY = Vector3.Dot (up, pointVelPosY) * pointVelPosY.sqrMagnitude;   // get the proportion of the velocity vector in the direction of face's normal (0 - 1) times magnitude squared
		Vector3 fluidDragVecPosY = -up * velPosY * transform.localScale.y * viscosityDrag;  
		Rigidbody2D.AddForceAtPosition (-fluidDragVecPosY * 2, transform.position);
		//Debug.DrawLine(-fluidDragVecPosY, new Vector3(0, 0, 0));
		
		// Vector3 pointVelNegY = Rigidbody2D.GetPointVelocity (yneg_face_center);
		// float velNegY = Vector3.Dot (up, pointVelNegY) * pointVelNegY.sqrMagnitude;   // get the proportion of the velocity vector in the direction of face's normal (0 - 1) times magnitude squared
		// Vector3 fluidDragVecNegY = -up * velNegY * transform.localScale.y * viscosityDrag;  
		// Rigidbody2D.AddForceAtPosition (fluidDragVecNegY, ypos_face_center);
		
		// RIGHT (posX):
		Vector3 pointVelPosX = Rigidbody2D.GetPointVelocity (xpos_face_center);
		float velPosX = Vector3.Dot (right, pointVelPosX) * pointVelPosX.sqrMagnitude;   // get the proportion of the velocity vector in the direction of face's normal (0 - 1) times magnitude squared
		Vector3 fluidDragVecPosX = -right * velPosX * transform.localScale.x * viscosityDrag;  
		Rigidbody2D.AddForceAtPosition (-fluidDragVecPosX * 2 , transform.position);
		
		// Vector3 pointVelNegX = Rigidbody2D.GetPointVelocity (xneg_face_center);
		// float velNegX = Vector3.Dot (right, pointVelNegX) * pointVelNegX.sqrMagnitude;   // get the proportion of the velocity vector in the direction of face's normal (0 - 1) times magnitude squared
		// Vector3 fluidDragVecNegX = -right * velNegX * transform.localScale.x * viscosityDrag;  
		// Rigidbody2D.AddForceAtPosition (fluidDragVecNegX, xpos_face_center);

       
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CalculateViscosityVelocity();
    }
}
// var right = transform.right;
// var up = transform.up;
// Vector3 xpos_face_center = (right * transform.localScale.x / 2) + transform.position;
// Vector3 ypos_face_center = (up * transform.localScale.y / 2) + transform.position;
// Vector3 xneg_face_center = -(right * transform.localScale.x / 2) + transform.position;
// Vector3 yneg_face_center = -(up * transform.localScale.y / 2) + transform.position;
//         
// Vector3 pointVelPosY = Rigidbody2D.GetPointVelocity (ypos_face_center);
// Vector3 fluidDragVecPosY = -up * Vector3.Dot (up, pointVelPosY) * transform.localScale.y * viscosityDrag;
// Rigidbody2D.AddForceAtPosition(fluidDragVecPosY * 2, ypos_face_center);
//
// Vector3 pointVelPosX = Rigidbody2D.GetPointVelocity (xpos_face_center);
// Vector3 fluidDragVecPosX = -right * Vector3.Dot (right, pointVelPosX) * transform.localScale.x * viscosityDrag;
// Rigidbody2D.AddForceAtPosition (fluidDragVecPosX*2, xpos_face_center);

// Vector2 end =  transform.position + transform.up * (length / 2);
// Vector2 velocityPlusRigidbody = new Vector2(transform.position.x + Rigidbody2D.velocity.x,
//     transform.position.y + Rigidbody2D.velocity.y);
// Vector2 newVector = Quaternion.Euler(transform.eulerAngles) * Rigidbody2D.velocity;
// Vector2 rotatedVector = new Vector2(newVector.x, -newVector.y);
// var velocity = Rigidbody2D.velocity;
//
// Vector2 liftVector = new Vector2(0, 0);
//
// if (rotatedVector.x > 0 && rotatedVector.y > 0)
// {
//     liftVector = velocity.Perpendicular1();
// }
// else if (rotatedVector.x < 0 && rotatedVector.y > 0)
// {
//     liftVector = velocity.Perpendicular2();
// }
// else if (rotatedVector.x > 0 && rotatedVector.y < 0)
// {
//     liftVector = velocity.Perpendicular2();
// }
// else if (rotatedVector.x < 0 && rotatedVector.y < 0)
// {
//     liftVector = velocity.Perpendicular1();
// }
//
// //
// if (num == 0)
// {
//     Debug.DrawLine(new Vector3(0, 0, 0), rotatedVector);
//     //Debug.DrawLine(new Vector2(transform.position.x + Rigidbody2D.velocity.x, transform.position.y + Rigidbody2D.velocity.y), transform.position);
//     Debug.DrawLine(-Rigidbody2D.velocity + (Vector2)transform.position, transform.position);
//     Debug.DrawLine(new Vector2(transform.position.x + liftVector.x, transform.position.y + liftVector.y), transform.position);
//
// }
// Debug.Log(liftVector);
// Rigidbody2D.AddForce(-Rigidbody2D.velocity * 100);
// //Rigidbody2D.AddForceAtPosition(liftVector * 10, transform.position);