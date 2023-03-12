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
    public Bone parentBone;
    public int boneId;
    
    //Start is called before the first frame update

    public void CalculateViscosityVelocity()
    {
	    // Cache positive axis vectors:
	    Vector3 up = Rigidbody2D.transform.up;
		Vector3 right = Rigidbody2D.transform.right;
		// Find centers of each of box's faces
		Vector3 xpos_face_center = (right * transform.localScale.x / 2f) + transform.position;
		Vector3 ypos_face_center = (up * transform.localScale.y / 2f) + transform.position;
	
		Vector3 xneg_face_center = -(right * transform.localScale.x / 2f) + transform.position;
		Vector3 yneg_face_center = -(up * transform.localScale.y / 2f) + transform.position;

		Vector3 halfWayPointUp = (up * transform.localScale.y / 4f) + transform.position;
		Vector3 halfWayPointDown = -(up * transform.localScale.y / 4f) + transform.position;

		Vector3 origin = new Vector3(0, 0, 0);
		
		//Debug.Log(Rigidbody2D.GetPointVelocity(ypos_face_center));

		float dragConst = 0.5f;
		float angularDragConst = 0.01f;
		
		
		// TOP (posY):
		Vector3 velocityPosYFace = Rigidbody2D.GetPointVelocity(xpos_face_center);
		float dotYPos = Vector3.Dot(up, velocityPosYFace.normalized);
		float velocityExposedToDragYPos = (1 - Math.Abs(dotYPos)) * Math.Sign(dotYPos) * velocityPosYFace.sqrMagnitude;
		//float velPosY = Vector3.Dot (up, pointVelPosY) * pointVelPosY.sqrMagnitude;   // get the proportion of the velocity vector in the direction of face's normal (0 - 1) times magnitude squared
		float yDrag = 1f / ( 100f + velocityPosYFace.sqrMagnitude * 0.2f);
		Vector3 fluidDragVecPosY = velocityPosYFace.normalized * velocityExposedToDragYPos * transform.localScale.y * dragConst;
		Rigidbody2D.AddForceAtPosition (-fluidDragVecPosY, xpos_face_center);

		// BOTTOM (negY)
		Vector3 velocityNegYFace = Rigidbody2D.GetPointVelocity (xneg_face_center);
		 float dotYNeg = Vector3.Dot(up, velocityNegYFace.normalized);
		 float velocityExposedToDragYNeg = (1 - Math.Abs(dotYNeg)) * Math.Sign(dotYNeg) * velocityPosYFace.sqrMagnitude;
		 Vector3 fluidDragVecNegY = velocityNegYFace.normalized * velocityExposedToDragYNeg * transform.localScale.y * dragConst;
		Rigidbody2D.AddForceAtPosition (-fluidDragVecNegY, xneg_face_center);
		
		//RIGHT (posX):
		Vector3 velocityPosXFace = Rigidbody2D.GetPointVelocity(ypos_face_center);
		float dotPosX = Vector3.Dot(right, velocityPosXFace.normalized);
		float velocityExposedToDragXPos = (1 - Math.Abs(dotPosX)) * velocityPosXFace.sqrMagnitude;
		//float velPosX = Vector3.Dot (right, pointVelPosX) * pointVelPosX.magnitude;   // get the proportion of the velocity vector in the direction of face's normal (0 - 1) times magnitude squared
		float xDrag = 1f / (100f + velocityPosXFace.magnitude * 0.2f);
		Vector3 fluidDragVecPosX = velocityPosXFace.normalized * velocityExposedToDragXPos * transform.localScale.x * dragConst;  
		Rigidbody2D.AddForceAtPosition (-fluidDragVecPosX , ypos_face_center);
		
		Vector3 velocityNegXFace = Rigidbody2D.GetPointVelocity (yneg_face_center);
		float dotNegX = Vector3.Dot(right, velocityNegXFace.normalized);
		float velocityExposedToDragXNeg = (1 - Math.Abs(dotNegX)) * velocityNegXFace.sqrMagnitude;
		Vector3 fluidDragVecNegX = velocityNegXFace.normalized * velocityExposedToDragXNeg * transform.localScale.x * dragConst;
	    Rigidbody2D.AddForceAtPosition (-fluidDragVecNegX, yneg_face_center);

	    Vector3 angularDragVectorPos = Rigidbody2D.angularVelocity * angularDragConst * right ;
	    Vector3 angularDragVectorNeg = Rigidbody2D.angularVelocity  * angularDragConst * right;
	    //Rigidbody2D.AddTorque(angularDragConst);
	    //Rigidbody2D.AddForceAtPosition(angularDragVectorNeg, halfWayPointDown);
	    
		
		if (boneId == 0)
		{
			// Debug.DrawLine(angularDragVectorPos + halfWayPointUp, halfWayPointUp);
			// Debug.DrawLine(angularDragVectorNeg + halfWayPointDown, halfWayPointDown);
			// Debug.Log(Rigidbody2D.angularVelocity);
			// Debug.Log(angularDragVectorNeg);
			Debug.DrawLine(-fluidDragVecNegX + yneg_face_center, yneg_face_center);
			Debug.DrawLine(-fluidDragVecNegY + xneg_face_center, xneg_face_center);
			Debug.DrawLine(-fluidDragVecPosX + ypos_face_center, ypos_face_center);
			Debug.DrawLine(-fluidDragVecPosY + xpos_face_center, xpos_face_center);
			
			// Debug.DrawLine(halfWayPointDown, origin);
			// Debug.DrawLine(halfWayPointUp, origin);
			
			
			//Debug.DrawLine(yneg_face_center,  new Vector3(0, 0, 0));
			//Debug.DrawLine(pointVelPosY, new Vector3(0, 0, 0));
			//Debug.Log(Vector3.Dot(up, Rigidbody2D.velocity.normalized));
			//Debug.Log(velocityExposedToDrag);
			//Debug.DrawLine(fluidDragVecPosY + transform.position, transform.position);

		}
    }

    public void Viscosity2()
    {

	    //have the drag applied to the middle
	    Vector3 velocty = Rigidbody2D.velocity;
	    float dotYPos = Vector3.Dot(transform.up, velocty.normalized);
	    float velocityExposedToDragYPos = (1 - Math.Abs(dotYPos)) * Math.Sign(dotYPos) * velocty.sqrMagnitude;
	    //float velPosY = Vector3.Dot (up, pointVelPosY) * pointVelPosY.sqrMagnitude;   // get the proportion of the velocity vector in the direction of face's normal (0 - 1) times magnitude squared
	    float yDrag = 1f / ( 100f + velocty.sqrMagnitude * 0.2f);
	    Vector3 fluidDragVecPosY = transform.up * velocityExposedToDragYPos * transform.localScale.y * 0.5f;
	    Vector3 otherForce = velocty.normalized * velocityExposedToDragYPos * transform.localScale.y * 0.5f;
	    Rigidbody2D.AddForceAtPosition (-otherForce, transform.position);
	    //Rigidbody2D.AddForceAtPosition(-otherForce, transform.position);
	    
	    Debug.Log(-fluidDragVecPosY);
	    
	    Debug.DrawLine(-otherForce + transform.position, transform.position);
    }

    public void WaterPush()
    {
	    Vector3 velocity = Rigidbody2D.velocity;
	    float magnitudeInPerp = Vector3.Dot(transform.right, velocity.normalized) * velocity.sqrMagnitude;
	    Vector3 velInPerp = transform.right * magnitudeInPerp * transform.localScale.y;
	    Rigidbody2D.AddForceAtPosition(-velInPerp, transform.position);

	    if (boneId == 0)
	    {
		    //Debug.DrawLine(velInPerp + transform.position, transform.position);
		    //Debug.DrawLine(velocity + transform.position, transform.position);
	    }
    }
    
    

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
	    WaterPush();
    }
}
// first version
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

//second version kinda
// Vector3 velocity = Rigidbody2D.velocity;
// 		//Debug.Log(pointVelPosY);
// 		float dotY = Vector3.Dot(up, velocity.normalized);
// 		float velocityExposedToDragY = (1 - Math.Abs(dotY)) * Math.Sign(dotY) * velocity.sqrMagnitude;
// 		//float velPosY = Vector3.Dot (up, pointVelPosY) * pointVelPosY.sqrMagnitude;   // get the proportion of the velocity vector in the direction of face's normal (0 - 1) times magnitude squared
// 		float yDrag = 1f / ( 100f + velocity.sqrMagnitude * 0.2f);
// 		//Debug.Log(yDrag);
// 		//Debug.Log(velPosY);
// 		Vector3 fluidDragVecPosY = velocity.normalized * velocityExposedToDragY * transform.localScale.y * 0.95f;
// 		
// 		//Debug.Log(pointVelPosY.sqrMagnitude);
// 		//Debug.Log(fluidDragVecPosY * 2);
// 		// if (fluidDragVecPosY == new Vector3(0, 0, 0))
// 		// {
// 		// 	Debug.Log(pointVelPosY.sqrMagnitude);
// 		// 	Debug.Log(up);
// 		// 	Debug.Log(velPosY);
// 		// 	Debug.Log(yDrag);
// 		// }
// 		Rigidbody2D.AddForceAtPosition (-fluidDragVecPosY, transform.position);
// 		//Debug.DrawLine(-fluidDragVecPosY, new Vector3(0, 0, 0));
// 		
// 		// Vector3 pointVelNegY = Rigidbody2D.GetPointVelocity (yneg_face_center);
// 		// float velNegY = Vector3.Dot (up, pointVelNegY) * pointVelNegY.sqrMagnitude;   // get the proportion of the velocity vector in the direction of face's normal (0 - 1) times magnitude squared
// 		// Vector3 fluidDragVecNegY = -up * velNegY * transform.localScale.y * viscosityDrag;  
// 		// Rigidbody2D.AddForceAtPosition (fluidDragVecNegY, ypos_face_center);
// 		
// 		//RIGHT (posX):
// 		Vector3 pointVelPosX = Rigidbody2D.velocity;
// 		float dotX = Vector3.Dot(right, velocity.normalized);
// 		float velocityExposedToDragX = (1 - Math.Abs(dotX)) * velocity.sqrMagnitude;
// 		//float velPosX = Vector3.Dot (right, pointVelPosX) * pointVelPosX.magnitude;   // get the proportion of the velocity vector in the direction of face's normal (0 - 1) times magnitude squared
// 		float xDrag = 1f / (100f + pointVelPosX.magnitude * 0.2f);
// 		Vector3 fluidDragVecPosX = velocity.normalized * velocityExposedToDragX * transform.localScale.x * 0.95f;  
// 		Rigidbody2D.AddForceAtPosition (-fluidDragVecPosX , transform.position);
// 		
// 		if (boneId == 0)
// 		{
// 			Debug.DrawLine(fluidDragVecPosY,  new Vector3(0, 0, 0));
// 			//Debug.DrawLine(pointVelPosY, new Vector3(0, 0, 0));
// 			//Debug.Log(Vector3.Dot(up, Rigidbody2D.velocity.normalized));
// 			//Debug.Log(velocityExposedToDrag);
// 			//Debug.DrawLine(fluidDragVecPosY + transform.position, transform.position);
//
// 		}
// 		
// 		// Vector3 pointVelNegX = Rigidbody2D.GetPointVelocity (xneg_face_center);
// 		// float velNegX = Vector3.Dot (right, pointVelNegX) * pointVelNegX.sqrMagnitude;   // get the proportion of the velocity vector in the direction of face's normal (0 - 1) times magnitude squared
// 		// Vector3 fluidDragVecNegX = -right * velNegX * transform.localScale.x * viscosityDrag;  
// 		// Rigidbody2D.AddForceAtPosition (fluidDragVecNegX, xpos_face_center);