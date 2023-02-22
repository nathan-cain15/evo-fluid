using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Entity : MonoBehaviour
{
    public GameObject bonePrefab;
    public GameObject jointPrefab;
    public GameObject checkForOverlapPrefab;
    
    public List<Muscle> muscles;
    public List<Bone> bones;
    public List<Joint> joints;
    public Joint firstJoint;

    public Entity()
    {
        joints = new List<Joint>();
        bones = new List<Bone>();
        muscles = new List<Muscle>();
    }
    
    public void AddBone(Joint joint, int angle, float length)
    {
        if (IsBoneOverlapping(joint, angle, length))
        {
            return;
        }
        
        var boneObject = Instantiate(bonePrefab).AddComponent<Bone>();
        var boneComponent = boneObject.GetComponent<Bone>();
        boneComponent.length = length;
        boneObject.transform.localScale = new Vector3(0.25f, length, 0);
        
        boneComponent.firstJoint = joint;
        boneComponent.firstAngle = angle;
        boneComponent.length = length;
        
        boneObject.transform.parent = this.transform;
        boneObject.transform.localPosition = joint.transform.localPosition;
        
        boneObject.transform.Translate(new Vector3(length / 2, 0, 0));
        boneObject.transform.Rotate(new Vector3(0, 0, 90), Space.Self );
        boneObject.transform.RotateAround(joint.transform.position, new Vector3(0, 0, 1), angle);
        
        boneComponent.firstEnd =  new Vector3(boneObject.transform.position.x - MathF.Cos(boneObject.firstAngle * Mathf.Deg2Rad) * (boneObject.length / 2), boneObject.transform.position.y - MathF.Sin(boneObject.firstAngle * Mathf.Deg2Rad) * (boneObject.length / 2), 0 );
        boneComponent.secondEnd = new Vector3(
            boneObject.transform.position.x +
            MathF.Cos(boneObject.firstAngle * Mathf.Deg2Rad) * (boneObject.length / 2),
            boneObject.transform.position.y +
            MathF.Sin(boneObject.firstAngle * Mathf.Deg2Rad) * (boneObject.length / 2), 0);

        boneObject.AddComponent<Rigidbody2D>();
        boneObject.GetComponent<Rigidbody2D>().gravityScale = 0;
        boneObject.GetComponent<Rigidbody2D>().drag = 1;
        boneObject.GetComponent<Rigidbody2D>().angularDrag = 0.1f;
        boneObject.GetComponent<Rigidbody2D>().mass = length;
        boneComponent.Rigidbody2D = boneObject.GetComponent<Rigidbody2D>();
        
        joint.AddComponent<HingeJoint2D>().connectedBody = boneObject.GetComponent<Rigidbody2D>();
        
        bones.Add(boneComponent);
    }
    
    // creates a bone and returns if its overlapping with another collider
    public bool IsBoneOverlapping(Joint joint, int angle, float length)
    {
        var checkForOverLap = Instantiate(checkForOverlapPrefab);
        checkForOverLap.GetComponent<PolygonCollider2D>().isTrigger = true;
        checkForOverLap.transform.parent = this.transform;
        checkForOverLap.transform.localPosition = joint.transform.localPosition;
        checkForOverLap.transform.localScale = new Vector3(0.25f, length - 1, 0);

        checkForOverLap.transform.Translate(new Vector3(length / 2, 0, 0));
        checkForOverLap.transform.Rotate(new Vector3(0, 0, 90), Space.Self);
        checkForOverLap.transform.RotateAround(joint.transform.position, new Vector3(0, 0, 1), angle);
        
        Physics2D.SyncTransforms();
        var results = new List<Collider2D>();
        checkForOverLap.GetComponent<PolygonCollider2D>().OverlapCollider(new ContactFilter2D().NoFilter(), results);
        Destroy(checkForOverLap);
        return results.Count > 0;
    }

    public void AddJoint(Bone bone)
    {
        //may need to add check for if a joint exists at that point
        var joint = Instantiate(jointPrefab);
        joint.transform.parent = this.transform;
        joint.transform.position = bone.secondEnd;
        joint.AddComponent<Joint>();
        joint.AddComponent<HingeJoint2D>().connectedBody = bone.GetComponent<Rigidbody2D>();
        
        joints.Add(joint.GetComponent<Joint>());
    }

    public void addMuscle(Bone bone, Bone otherBone)
    {
        var muscleObject = new GameObject("Muscle").AddComponent<LineRenderer>();
        muscleObject.AddComponent<Muscle>();
        muscleObject.transform.parent = this.transform;
        var muscleComponent = muscleObject.GetComponent<Muscle>();
        var lineRenderer = muscleObject.GetComponent<LineRenderer>();

        muscleObject.GetComponent<Muscle>().LineRenderer = muscleObject.GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, bone.transform.position);
        lineRenderer.SetPosition(1, otherBone.transform.position);
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        lineRenderer.AddComponent<Rigidbody2D>();
        lineRenderer.GetComponent<Rigidbody2D>().gravityScale = 0;
        
        lineRenderer.transform.position = (bone.transform.position + otherBone.transform.position) / 2;
        
        bone.AddComponent<SpringJoint2D>();
        bone.GetComponent<SpringJoint2D>().connectedBody = otherBone.GetComponent<Rigidbody2D>();
        bone.GetComponent<SpringJoint2D>().enableCollision = true;
        bone.GetComponent<SpringJoint2D>().frequency = 0.75f;
        bone.GetComponent<SpringJoint2D>().dampingRatio = 0.6f;
        bone.GetComponent<SpringJoint2D>().autoConfigureDistance = false;
        bone.GetComponent<SpringJoint2D>().autoConfigureConnectedAnchor = false;
        muscleComponent.firstBone = bone;
        muscleComponent.secondBone = otherBone;
        muscleComponent.timeScale = 3;
        muscleComponent.force = 10000;
        
        muscles.Add(muscleObject.GetComponent<Muscle>());
    }


    public void UpdateLinePoints(Muscle muscle)
    {
        muscle.LineRenderer.SetPosition(0, muscle.firstBone.transform.position);
        muscle.LineRenderer.SetPosition(1, muscle.secondBone.transform.position);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void Update()
    {
        foreach (var muscle in muscles)
        {
            UpdateLinePoints(muscle);
        }
    }
}
