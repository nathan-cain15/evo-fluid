using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = System.Random;

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
    public bool firstIteration = true;
    
    public int entityId;
    public int boneId = 0;
    public int jointId = 0;
    public int muscleId = 0;


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
        Physics2D.SyncTransforms();
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
                    var checkingList = Physics2D.OverlapAreaAll(new Vector2(vector1.x - length - 1, vector1.y + i), new Vector2(vector2.x - 1 - length, vector2.y + i));
            
                    if (checkingList.Length == 0)
                    {
                        return new Vector2(vector1.x - length * 0.5f - 1, vector1.y + i + (height / 2));
                    }
                }
            }
           
            //checking top
            if (num == 1)
            {
                for (int i = 0; i < length; i++)
                {
                    var checkingList = Physics2D.OverlapAreaAll(new Vector2(vector1.x + i, vector1.y + height + 1),
                        new Vector2(vector2.x + i, vector2.y + height + 1));
                    if (checkingList.Length == 0)
                    {
                        return new Vector2(vector1.x + i + length * 0.5f, vector1.y + height * 1.5f + 1);
                    }
            
                }
            }
           
            //checking right side
            if (num == 2)
            {
                for (int i = 0; i < height * 2; i++)
                {
                    var checkingList = Physics2D.OverlapAreaAll(new Vector2(vector1.x + length + 1, vector1.y - height + i), new Vector2(vector2.x + 1 + length, vector2.y - height + i));
            
                    if (checkingList.Length == 0)
                    {
                        return new Vector2(vector1.x + length * 1.5f + 1, vector1.y - height + i + (height / 2));
                    }
                }
            }

            //checking bottom
            if (num == 3)
            {
                for (int i = 0; i < length * 2; i++)
                {
                    var checkingList = Physics2D.OverlapAreaAll(new Vector2(vector1.x - length + i, vector1.y - height - 1),
                        new Vector2(vector2.x - length + i, vector2.y - height - 1));
                    if (checkingList.Length == 0)
                    {
                        return new Vector2(vector1.x + i - length * 0.5f, vector1.y - height * 0.5f - 1);
                    }
            
                }
            }
        }
        return null;
    }
    
    
    //used to create starting entity
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
        boneObject.GetComponent<Rigidbody2D>().drag = 0.5f;
        boneObject.GetComponent<Rigidbody2D>().angularDrag = 0.5f;
        boneObject.GetComponent<Rigidbody2D>().mass = length;
        boneComponent.Rigidbody2D = boneObject.GetComponent<Rigidbody2D>();
        
        joint.AddComponent<HingeJoint2D>().connectedBody = boneObject.GetComponent<Rigidbody2D>();

        boneComponent.boneId = boneId;
        boneId++;
        
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
    
    // used to create starting entity
    public void AddJoint(Bone bone)
    {
        //may need to add check for if a joint exists at that point
        var joint = Instantiate(jointPrefab);
        joint.transform.parent = this.transform;
        joint.transform.position = bone.secondEnd;
        joint.GetComponent<Joint>().entity = this;
        joint.AddComponent<HingeJoint2D>().connectedBody = bone.GetComponent<Rigidbody2D>();
        bone.secondJoint = joint.GetComponent<Joint>();
        joint.GetComponent<Joint>().jointId = jointId + 1;
        jointId++;
        
        joints.Add(joint.GetComponent<Joint>());
    }

    //used to create starting entity
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
        lineRenderer.GetComponent<Rigidbody2D>().drag = 0.5f;
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

        muscleComponent.muscleId = muscleId;
        muscleId++;
        
        muscles.Add(muscleObject.GetComponent<Muscle>());
    }
    
    public void UpdateLinePoints(Muscle muscle)
    {
        muscle.LineRenderer.SetPosition(0, muscle.firstBone.transform.position);
        muscle.LineRenderer.SetPosition(1, muscle.secondBone.transform.position);
    }

    [CanBeNull]
    public Entity Reproduce()
    {
        Physics2D.SyncTransforms();
        var pos = returnSuitablePlacementPoint();
        Debug.Log(pos);
        if (pos.HasValue)
        {
            var child = new GameObject("entity");
            child.AddComponent<Entity>();
            var childEntityScript = child.GetComponent<Entity>();
            childEntityScript.parent = this.GetComponent<Entity>();
            child.transform.position = pos.Value;
            childEntityScript.bonePrefab = bonePrefab;
            childEntityScript.jointPrefab = jointPrefab;
            childEntityScript.CreateAndMutateJoints(0.75f, 1f);
            childEntityScript.ConnectBones();
            childEntityScript.ConnectMuscles();
            return childEntityScript;
        }

        return null;
    }
    
    //creates joints from parent entity, still have to add mutation
    // not sure how to make the percent stay or change for x and y, either making it coupled or not
    public void CreateAndMutateJoints(float percentStay, float amount)
    {
        foreach (var joint in parent.joints)
        {
            var newJoint = Instantiate(jointPrefab);
            var newJointComponent = newJoint.GetComponent<Joint>();
            newJoint.transform.parent = this.transform;
            newJoint.transform.position = this.transform.position + joint.transform.position - parent.joints[0].transform.position;
            newJointComponent.parentJoint = joint;
            newJointComponent.entity = this;
            newJointComponent.jointId = jointId;
            jointId++;
            
            var xChangeRandomPercent = UnityEngine.Random.Range(0f, 1f);
            var yChangeRandomPercent = UnityEngine.Random.Range(0f, 1f);
            var xChange = 0f;
            var yChange = 0f;

            if (xChangeRandomPercent > percentStay)
            {
                var posOrNegInt = UnityEngine.Random.Range(0, 1);
                if (posOrNegInt == 1)
                {
                    xChange = amount;
                }
                else
                {
                    xChange = -amount;
                }
                
            }

            if (yChangeRandomPercent > percentStay)
            {
                var posOrNegInt = UnityEngine.Random.Range(0, 1);
                if (posOrNegInt == 1)
                {
                    yChange = amount;
                }
                else
                {
                    yChange = -amount;
                }
            }

            newJoint.transform.position =
                new Vector2(newJoint.transform.position.x + xChange, newJoint.transform.position.y + yChange);
            
            joints.Add(newJointComponent);
        }

    }

    // connect the bones of the mutated joints
    public void ConnectBones()
    {
        foreach (var bone in parent.bones)
        {
            //create the bone and add it to the entity
            var newBone = Instantiate(bonePrefab);
            newBone.AddComponent<Bone>();
            var newBoneComponent = newBone.GetComponent<Bone>();
            bones.Add(newBoneComponent);
            newBoneComponent.parentBone = bone;
            var jointPos1 = bone.firstJoint.transform.localPosition;
            var jointPos2 = bone.secondJoint.transform.localPosition;
            newBone.transform.parent = this.transform;
            newBoneComponent.boneId = boneId;
            boneId++;
            
            //set up rigidbody
            newBone.AddComponent<Rigidbody2D>();
            newBoneComponent.Rigidbody2D = newBone.GetComponent<Rigidbody2D>();
            newBoneComponent.Rigidbody2D.gravityScale = 0;
            newBoneComponent.Rigidbody2D.drag = 0.05f;
            newBoneComponent.Rigidbody2D.angularDrag = 0.5f;
            
            // find the corresponding joints based on the shared parent positions
            newBoneComponent.firstJoint = joints.Find(x => x.jointId == bone.firstJoint.jointId);
            newBoneComponent.secondJoint = joints.Find(x => x.jointId == bone.secondJoint.jointId);

            newBone.transform.position = new Vector2((newBoneComponent.firstJoint.transform.position.x + newBoneComponent.secondJoint.transform.position.x) / 2, (newBoneComponent.firstJoint.transform.position.y + newBoneComponent.secondJoint.transform.position.y) / 2);

            //set the length of the bone
            newBone.transform.localScale = new Vector2(newBone.transform.localScale.x,
                2 * Vector2.Distance(newBoneComponent.transform.position, newBoneComponent.firstJoint.transform.position));
            newBoneComponent.length = newBone.transform.localScale.y;
            newBoneComponent.Rigidbody2D.mass = newBoneComponent.length;
            
            //calculate the angle needed to set the bone into place with the joints
            var angle = Vector2.SignedAngle(new Vector2(newBoneComponent.secondJoint.transform.position.x - newBone.transform.position.x, newBoneComponent.secondJoint.transform.position.y - newBone.transform.position.y), new Vector2(1, 0));
            newBone.transform.eulerAngles = new Vector3(0, 0, 90 - angle);
            newBoneComponent.firstJoint.AddComponent<HingeJoint2D>().connectedBody = newBoneComponent.GetComponent<Rigidbody2D>();
            newBoneComponent.secondJoint.AddComponent<HingeJoint2D>().connectedBody = newBoneComponent.GetComponent<Rigidbody2D>();
        }
    }

    public void ConnectMuscles()
    {
        foreach (var muscle in parent.muscles)
        {
            // create child muscle
            var muscleObject = new GameObject("Muscle").AddComponent<LineRenderer>();
            muscleObject.AddComponent<Muscle>();
            muscleObject.transform.parent = this.transform;
            var muscleComponent = muscleObject.GetComponent<Muscle>();
            var lineRenderer = muscleObject.GetComponent<LineRenderer>();
            muscleComponent.LineRenderer = muscleObject.GetComponent<LineRenderer>();
            muscleComponent.muscleId = muscleId;
            muscleId++;

            // find the corresponding bones based on the shared parent 
            var bone = bones.Find(x => x.boneId == muscle.firstBone.boneId);
            var otherBone = bones.Find(x => x.boneId == muscle.secondBone.boneId);
            muscleComponent.firstBone = bone;
            muscleComponent.secondBone = otherBone;
            
            // set up the line renderer
            lineRenderer.SetPosition(0, bone.transform.position);
            lineRenderer.SetPosition(1, otherBone.transform.position);
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            // lineRenderer.AddComponent<Rigidbody2D>();
            // lineRenderer.GetComponent<Rigidbody2D>().drag = 0.5f;
            // lineRenderer.GetComponent<Rigidbody2D>().gravityScale = 0;
           
            lineRenderer.transform.position = (bone.transform.position + otherBone.transform.position) / 2;
            
            //set up the spring
            bone.AddComponent<SpringJoint2D>();
            var boneSpringJoint = bone.GetComponent<SpringJoint2D>();
            boneSpringJoint.autoConfigureDistance = false;
            boneSpringJoint.autoConfigureConnectedAnchor = false;
            muscleComponent.timeScale = 3;
            boneSpringJoint.connectedBody = otherBone.GetComponent<Rigidbody2D>();
            boneSpringJoint.distance = Vector2.Distance(bone.transform.position, otherBone.transform.position);
            boneSpringJoint.enableCollision = true;
            boneSpringJoint.frequency = 0.75f;
            boneSpringJoint.dampingRatio = 0.6f;
            muscleComponent.force = 10000f;
            muscleComponent.timeScale = 3f;
            //Debug.Log(boneSpringJoint.distance);
            muscles.Add(muscleObject.GetComponent<Muscle>());
            
        }
    }
    
    public void MutateMuscleForce(float percentStay, int changeAmount)
    {
        foreach (var muscle in muscles)
        {
            var percent = UnityEngine.Random.Range(0f, 1f);

            if (percent > percentStay)
            {
                var posOrNegInt = UnityEngine.Random.Range(0, 1);

                if (posOrNegInt == 1)
                {
                    muscle.force += changeAmount;
                }
                else
                {
                    muscle.force -= changeAmount;
                }
            }
        }
    }

    public void MutateMuscleTimeScale(float percentStay, float changeAmount)
    {
        foreach (var muscle in muscles)
        {
            var percent = UnityEngine.Random.Range(0f, 1f);

            if (percent > percentStay)
            {
                var posOrNegInt = UnityEngine.Random.Range(0, 1);

                if (posOrNegInt == 1)
                {
                    muscle.timeScale += changeAmount;
                }
                else
                {
                    muscle.timeScale -= changeAmount;
                }
            }
        }
        
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

        // if (entityId == 1)
        // {
        //     Time.timeScale = 0;
        // }

       
    }
}
