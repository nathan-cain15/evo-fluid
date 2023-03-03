using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
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
    public float energy = 0f;
    public Entity parent;


    public Entity()
    {
        joints = new List<Joint>();
        bones = new List<Bone>();
        muscles = new List<Muscle>();
    }

    public List<Vector2> getDimensions()
    {
        float posLeft = transform.position.x;
        float posRight = transform.position.x;
        float posTop = transform.position.y;
        float posBottom = transform.position.y;

        var boneEndVecs = new List<Vector2>();

        foreach (var bone in bones)
        {
            Vector2 boneEnd1 = -bone.transform.up * (bone.length / 2) + bone.transform.position;
            Vector2 boneEnd2 = bone.transform.up * (bone.length / 2) + bone.transform.position;
            boneEndVecs.Add(boneEnd1);
            boneEndVecs.Add(boneEnd2);
        }

        var boneVecs = boneEndVecs.Distinct().ToList();

        foreach (var vec in boneVecs)
        {
            if (vec.x > posRight)
            {
                posRight = vec.x;
            }
            else if (vec.x < posLeft)
            {
                posLeft = vec.x;
            }

            if (vec.y > posTop)
            {
                posTop = vec.y;
            }
            else if (vec.y < posBottom)
            {
                posBottom = vec.y;
            }
        }

        List<Vector2> dimensionCoords = new List<Vector2>();
        dimensionCoords.Add(new Vector2(posLeft, posBottom));
        dimensionCoords.Add(new Vector2(posRight, posTop));
        
        return dimensionCoords;
    }
    public Vector2? returnSuitablePlacementPoint()
    {
        var dimensions = getDimensions();
        var vector1 = dimensions[0];
        var vector2 = dimensions[1];
        var length = vector2.x - vector1.x;
        var height = vector2.y - vector1.y;

        var numList = Enumerable.Range(0, 4).ToList();
        var shuffledList = numList.OrderBy( x => UnityEngine.Random.value ).ToList( );
       
        foreach (int num in shuffledList)
        {
            //checking left side
            if (num == 0)
            {
                for (int i = 0; i < height; i++)
                {
                    var checkingList = Physics2D.OverlapAreaAll(new Vector2(vector1.x - length - 3, vector1.y + i), new Vector2(vector2.x - 3 - length, vector2.y + i));
            
                    if (checkingList.Length == 0)
                    {
                        return new Vector2(vector1.x - length * 0.5f - 3, vector1.y + i + (height / 2));
                    }
                }
            }
           
            //checking top
            if (num == 1)
            {
                for (int i = 0; i < length; i++)
                {
                    var checkingList = Physics2D.OverlapAreaAll(new Vector2(vector1.x + i, vector1.y + height + 3),
                        new Vector2(vector2.x + i, vector2.y + height + 3));
                    if (checkingList.Length == 0)
                    {
                        return new Vector2(vector1.x + i + length * 0.5f, vector1.y + height * 1.5f + 3);
                    }
            
                }
            }
           
            //checking right side
            if (num == 2)
            {
                for (int i = 0; i < height * 2; i++)
                {
                    var checkingList = Physics2D.OverlapAreaAll(new Vector2(vector1.x + length + 3, vector1.y - height + i), new Vector2(vector2.x + 3 + length, vector2.y - height + i));
            
                    if (checkingList.Length == 0)
                    {
                        return new Vector2(vector1.x + length * 1.5f + 3, vector1.y - height + i + (height / 2));
                    }
                }
            }

            //checking bottom
            if (num == 3)
            {
                for (int i = 0; i < length * 2; i++)
                {
                    var checkingList = Physics2D.OverlapAreaAll(new Vector2(vector1.x - length + i, vector1.y - height - 3),
                        new Vector2(vector2.x - length + i, vector2.y - height - 3));
                    if (checkingList.Length == 0)
                    {
                        return new Vector2(vector1.x + i - length * 0.5f, vector1.y - height * 0.5f - 3);
                    }
            
                }
            }
        }
        return null;
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
        boneComponent.entity = this;
        
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
        joint.GetComponent<Joint>().entity = this;
        joint.AddComponent<HingeJoint2D>().connectedBody = bone.GetComponent<Rigidbody2D>();
        bone.secondJoint = joint.GetComponent<Joint>();
        
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

    public void Reproduce()
    {
        var pos = returnSuitablePlacementPoint();
        if (pos.HasValue)
        {
            var child = new GameObject("entity");
            child.AddComponent<Entity>();
            var childEntityScript = child.GetComponent<Entity>();
            childEntityScript.parent = this.GetComponent<Entity>();
            childEntityScript.transform.position = pos.Value;
            childEntityScript.bonePrefab = bonePrefab;
            childEntityScript.jointPrefab = jointPrefab;
            childEntityScript.CreateAndMutateJoints();
            childEntityScript.ConnectBones();
            childEntityScript.ConnectMuscles();
        }
    }
    
    //creates joints from parent entity, still have to add mutation
    public void CreateAndMutateJoints()
    {
        foreach (var joint in parent.joints)
        {
            var newJoint = Instantiate(jointPrefab);
            newJoint.transform.parent = this.transform;
            newJoint.transform.localPosition = joint.transform.localPosition;
            newJoint.AddComponent<Joint>();
            newJoint.GetComponent<Joint>().parentJoint = joint;
            joints.Add(newJoint.GetComponent<Joint>());
        }

    }
    
    public void ConnectBones()
    {
        foreach (var bone in parent.bones)
        {
            var newBone = Instantiate(bonePrefab);
            newBone.AddComponent<Bone>();
            var newBoneScript = newBone.GetComponent<Bone>();
            bones.Add(newBoneScript);
            newBoneScript.parentBone = bone;
            var jointPos1 = bone.firstJoint.transform.localPosition;
            var jointPos2 = bone.secondJoint.transform.localPosition;
            newBone.transform.parent = this.transform;
            
            newBone.AddComponent<Rigidbody2D>();
            newBoneScript.Rigidbody2D = newBone.GetComponent<Rigidbody2D>();
            newBoneScript.Rigidbody2D.gravityScale = 0;
            newBoneScript.Rigidbody2D.drag = 1;
            newBoneScript.Rigidbody2D.angularDrag = 0.1f;
            
            newBoneScript.firstJoint = joints.Find(x => x.parentJoint.transform.localPosition == jointPos1);
            newBoneScript.secondJoint = joints.Find(x => x.parentJoint.transform.localPosition == jointPos2);

            newBone.transform.position = new Vector2((newBoneScript.firstJoint.transform.position.x + newBoneScript.secondJoint.transform.position.x) / 2, (newBoneScript.firstJoint.transform.position.y + newBoneScript.secondJoint.transform.position.y) / 2);

            newBone.transform.localScale = new Vector2(newBone.transform.localScale.x,
                2 * Vector2.Distance(newBoneScript.transform.position, newBoneScript.firstJoint.transform.position));
            newBoneScript.length = newBone.transform.localScale.y;
            newBoneScript.Rigidbody2D.mass = newBoneScript.length;

            var angle = Vector2.SignedAngle(new Vector2(newBoneScript.secondJoint.transform.position.x - newBone.transform.position.x, newBoneScript.secondJoint.transform.position.y - newBone.transform.position.y), new Vector2(1, 0));
            
            newBone.transform.eulerAngles = new Vector3(0, 0, 90 - angle);
            newBoneScript.firstJoint.AddComponent<HingeJoint2D>().connectedBody = bone.GetComponent<Rigidbody2D>();
            newBoneScript.secondJoint.AddComponent<HingeJoint2D>().connectedBody = bone.GetComponent<Rigidbody2D>();
        }
    }

    public void ConnectMuscles()
    {
        foreach (var muscle in parent.muscles)
        {
            var muscleObject = new GameObject("Muscle").AddComponent<LineRenderer>();
            muscleObject.AddComponent<Muscle>();
            muscleObject.transform.parent = this.transform;
            var muscleComponent = muscleObject.GetComponent<Muscle>();
            var lineRenderer = muscleObject.GetComponent<LineRenderer>();
            
            muscleObject.GetComponent<Muscle>().LineRenderer = muscleObject.GetComponent<LineRenderer>();
            Debug.Log(bones[0].parentBone);
            var bone = bones.Find(x => x.parentBone.transform.position == muscle.firstBone.transform.position);
            var otherBone = bones.Find(x => x.parentBone.transform.position == muscle.secondBone.transform.position);
            Debug.Log(bone);
            Debug.Log(otherBone);
            
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
    }
    
    // percentStay and percentChange has to add up to 1
    public void MutateMuscleforce(float percentStay, float percentChange, int changeAmount)
    {
        
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void Update()
    {
        getDimensions();
        foreach (var muscle in muscles)
        {
            UpdateLinePoints(muscle);
        }
    }
}
