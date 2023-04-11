using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
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
    public int energyReserve = 100000;
    public int energyReserveGivenToChildren = 100000;
    public int energyToReproduce = 0;
    public Entity parent;
    public bool isWaitingToReproduce = false;
    public int entityId;
    public int boneId = 0;
    public int jointId = 0;
    public int muscleId = 0;
    public int CostToCreate;
    public int reproductionAngle;
    
    public Vector2 bottomLeftDebug;
    public Vector2 topRightDebug;

    public Vector2 latestPosition;
    public Vector2 secondLatestPosition;

    public GameController GameController;

    public Entity()
    {
        joints = new List<Joint>();
        bones = new List<Bone>();
        muscles = new List<Muscle>();
    }

    public List<Vector2> getDimensions()
    {
        float posLeft = bones[0].transform.position.x;
        float posRight = bones[0].transform.position.x;
        float posTop = bones[0].transform.position.y;
        float posBottom = bones[0].transform.position.y;

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
        bottomLeftDebug = vector1;
        topRightDebug = vector2;
        var length = vector2.x - vector1.x;
        var height = vector2.y - vector1.y;
        
        var angleList = new List<int>{0, 90, 180, 270};
        var randomAngle = angleList[UnityEngine.Random.Range(0, 4)];
        reproductionAngle = randomAngle;
        if (randomAngle == 90 | randomAngle == 180)
        {
            Vector2 center = new Vector2(vector1[0] + (length / 2), vector1[1] + (height / 2));
            vector1 = new Vector2(center.x - (height / 2), center.y - (length / 2));
            vector2 = new Vector2(center.x + (height / 2), center.y + (length / 2));
        }

        var numList = Enumerable.Range(0, 4).ToList();
        var shuffledList = numList.OrderBy( x => UnityEngine.Random.value ).ToList( );

        foreach (int num in shuffledList)
        {
            //checking left side
            if (num == 0)
            {
                for (int i = 0; i < height; i++)
                {
                    var checkingList = Physics2D.OverlapAreaAll(new Vector2(vector1.x - length - 1, vector1.y + i), new Vector2(vector2.x - 1 - length, vector2.y + i), 0);
            
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
                        new Vector2(vector2.x + i, vector2.y + height + 1), 0);
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
                    var checkingList = Physics2D.OverlapAreaAll(new Vector2(vector1.x + length + 1, vector1.y - height + i), new Vector2(vector2.x + 1 + length, vector2.y - height + i), 0);
            
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
                        new Vector2(vector2.x - length + i, vector2.y - height - 1), 0);
                    if (checkingList.Length == 0)
                    {
                        return new Vector2(vector1.x + i - length * 0.5f, vector1.y - height * 0.5f - 1);
                    }
            
                }
            }
        }
        return null;
    }

    public void DrawRect(Vector2 bottomLeft, Vector2 topRight)
    {
        Debug.DrawLine(bottomLeft, new Vector3(topRight.x, bottomLeft.y));
        Debug.DrawLine(bottomLeft, new Vector3(bottomLeft.x, topRight.y));
        Debug.DrawLine(topRight, new Vector3(bottomLeft.x, topRight.y));
        Debug.DrawLine(topRight, new Vector3(topRight.x, bottomLeft.y));
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

        joint.GetComponent<Joint>().originalPosition = joint.transform.position;
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

        muscleComponent.LineRenderer = muscleObject.GetComponent<LineRenderer>();
        muscleComponent.entity = this;

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
        
        if (pos.HasValue)
        {
            var child = new GameObject("entity");
            child.AddComponent<Entity>();
            var childEntityScript = child.GetComponent<Entity>();
            childEntityScript.parent = this.GetComponent<Entity>();
            child.transform.position = pos.Value;
            childEntityScript.bonePrefab = bonePrefab;
            childEntityScript.jointPrefab = jointPrefab;
            childEntityScript.GameController = GameController;
            childEntityScript.entityId = GameController.entityId;
            childEntityScript.GameController.entityId++;
            childEntityScript.CreateAndMutateJoints(0.7f, 0.05f);
            childEntityScript.transform.Rotate(new Vector3(0, 0, reproductionAngle));
            childEntityScript.ConnectBones();
            //childEntityScript.CreateNewBoneMutations(0.80f, 3);
            childEntityScript.ConnectMuscles();
            childEntityScript.MutateMuscleForce(0.7f, 25);
            childEntityScript.MutateMuscleTimeScale(0.7f, 0.02f);
            childEntityScript.CostToCreate = childEntityScript.CalculateCost();
            childEntityScript.energyReserve = energyReserveGivenToChildren;
            
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
            newJoint.transform.position = this.transform.position + joint.originalPosition - parent.joints[0].originalPosition;
            newJointComponent.parentJoint = joint;
            newJointComponent.entity = this;
            newJointComponent.jointId = jointId;
            jointId++;
            newJointComponent.originalPosition = newJointComponent.transform.position;
            
            var willChangePercentage = UnityEngine.Random.Range(0f, 1f);

            Vector2 transformChange = new Vector2(0, 0);

            if (willChangePercentage > percentStay)
            {
                transformChange = UnityEngine.Random.insideUnitCircle * amount;
            }
            
            newJoint.transform.position =
                new Vector2(newJoint.transform.position.x + transformChange.x, newJoint.transform.position.y + transformChange.y);
            
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
            newBoneComponent.entity = this;
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
            var length = 2 * Vector2.Distance(newBoneComponent.transform.position,
                newBoneComponent.firstJoint.transform.position);
            newBone.transform.localScale = new Vector2(newBone.transform.localScale.x, length);
            newBoneComponent.length = length;
            
            newBoneComponent.length = newBone.transform.localScale.y;
            newBoneComponent.Rigidbody2D.mass = newBoneComponent.length;
            
            //calculate the angle needed to set the bone into place with the joints
            var angle = Vector2.SignedAngle(new Vector2(newBoneComponent.secondJoint.transform.position.x - newBone.transform.position.x, newBoneComponent.secondJoint.transform.position.y - newBone.transform.position.y), new Vector2(1, 0));
            newBone.transform.eulerAngles = new Vector3(0, 0, 90 - angle);
            newBoneComponent.secondJoint.AddComponent<HingeJoint2D>().connectedBody = newBoneComponent.GetComponent<Rigidbody2D>();
            newBoneComponent.firstJoint.AddComponent<HingeJoint2D>().connectedBody = newBoneComponent.GetComponent<Rigidbody2D>();
        }
    }
    
    public void CreateNewBoneMutations(float percentstay, float length)
    {
        for (int i = 0; i < joints.Count; i++)
        {
            var willChangePercentage = UnityEngine.Random.Range(0f, 1f);

            if (willChangePercentage > percentstay)
            {
                var newBone = Instantiate(bonePrefab);
                
                newBone.AddComponent<Bone>();
                var newBoneComponent = newBone.GetComponent<Bone>();
                bones.Add(newBoneComponent);
                newBone.transform.parent = this.transform;
                newBoneComponent.entity = this;
                newBoneComponent.boneId = boneId;
                boneId++;

                var jointPosition = joints[i].transform.position;

                newBoneComponent.transform.localScale = new Vector3(0.25f, length, 0);
                var randAngle = UnityEngine.Random.Range(1, 361);
                Debug.Log(randAngle);
                newBoneComponent.transform.Rotate(new Vector3(0, 0, randAngle));

                newBoneComponent.transform.position = new Vector3(jointPosition.x + newBoneComponent.transform.up.x * length * 0.5f, jointPosition.y + newBoneComponent.transform.up.y * length * 0.5f);
                
                Physics2D.SyncTransforms();
                bool isTouching = newBoneComponent.GetComponent<Collider2D>().IsTouchingLayers(0);

                if (isTouching)
                {
                    bones.Remove(newBoneComponent);
                    Destroy(newBone);
                    return;
                }
                
                //Time.timeScale = 0;
                //set up rigidbody
                newBone.AddComponent<Rigidbody2D>();
                newBoneComponent.Rigidbody2D = newBone.GetComponent<Rigidbody2D>();
                newBoneComponent.Rigidbody2D.gravityScale = 0;
                newBoneComponent.Rigidbody2D.drag = 0.05f;
                newBoneComponent.Rigidbody2D.angularDrag = 0.5f;

                joints[i].AddComponent<HingeJoint2D>().connectedBody = newBoneComponent.GetComponent<Rigidbody2D>();

                var newJoint = Instantiate(jointPrefab);
                
                var newJointComponent = newJoint.GetComponent<Joint>();
                newJoint.transform.parent = this.transform;
                newJointComponent.entity = this;
                newJointComponent.jointId = jointId;
                jointId++;
                
                joints.Add(newJointComponent);

                newJointComponent.transform.position = new Vector3(
                    newBoneComponent.transform.position.x + newBoneComponent.transform.up.x * length * 0.5f,
                    newBoneComponent.transform.position.y + newBoneComponent.transform.up.y * length * 0.5f);

                newJointComponent.AddComponent<HingeJoint2D>().connectedBody =
                    newBoneComponent.GetComponent<Rigidbody2D>();

                newBoneComponent.firstJoint = joints[i];
                newBoneComponent.secondJoint = newJointComponent;

                var closestBone = bones[0];
                var distance = Vector2.Distance(closestBone.transform.position, newBoneComponent.transform.position);

                for (int j = 1; j < bones.Count - 1; j++)
                {
                    var loopDistance = Vector2.Distance(bones[j].transform.position, newBoneComponent.transform.position);
                    if (loopDistance < distance)
                    {
                        distance = loopDistance;
                        closestBone = bones[j];
                    }
                    
                }
                
            var muscleObject = new GameObject("Muscle").AddComponent<LineRenderer>();
            muscleObject.AddComponent<Muscle>();
            muscleObject.transform.parent = this.transform;
            var muscleComponent = muscleObject.GetComponent<Muscle>();
            var lineRenderer = muscleObject.GetComponent<LineRenderer>();
            muscleComponent.LineRenderer = muscleObject.GetComponent<LineRenderer>();
            muscleComponent.entity = this;
            muscleComponent.muscleId = muscleId;
            muscleId++;
            
            muscleComponent.firstBone = newBoneComponent;
            muscleComponent.secondBone = closestBone;
            
            // set up the line renderer
            lineRenderer.SetPosition(0, newBoneComponent.transform.position);
            lineRenderer.SetPosition(1, closestBone.transform.position);
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            // lineRenderer.AddComponent<Rigidbody2D>();
            // lineRenderer.GetComponent<Rigidbody2D>().drag = 0.5f;
            // lineRenderer.GetComponent<Rigidbody2D>().gravityScale = 0;
            
            lineRenderer.transform.position = (newBoneComponent.transform.position + closestBone.transform.position) / 2;
            
            //set up the spring
            newBoneComponent.AddComponent<SpringJoint2D>();
            var boneSpringJoint = newBoneComponent.GetComponent<SpringJoint2D>();
            boneSpringJoint.autoConfigureDistance = false;
            boneSpringJoint.autoConfigureConnectedAnchor = false;
            boneSpringJoint.connectedBody = closestBone.GetComponent<Rigidbody2D>();
            boneSpringJoint.distance = Vector2.Distance(newBoneComponent.transform.position, closestBone.transform.position);
            boneSpringJoint.enableCollision = true;
            boneSpringJoint.frequency = 0.75f;
            boneSpringJoint.dampingRatio = 0.6f;
            muscleComponent.force = 10000;
            muscleComponent.timeScale = 3;
            muscles.Add(muscleObject.GetComponent<Muscle>());

            }
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
            muscleComponent.entity = this;
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
            boneSpringJoint.connectedBody = otherBone.GetComponent<Rigidbody2D>();
            boneSpringJoint.distance = Vector2.Distance(bone.transform.position, otherBone.transform.position);
            boneSpringJoint.enableCollision = true;
            boneSpringJoint.frequency = 0.75f;
            boneSpringJoint.dampingRatio = 0.6f;
            muscleComponent.force = muscle.GetComponent<Muscle>().force;
            muscleComponent.timeScale = muscle.GetComponent<Muscle>().timeScale;
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
                var posOrNegInt = UnityEngine.Random.Range(0, 2);

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
                var posOrNegInt = UnityEngine.Random.Range(0, 2);

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


    public bool CheckIfDead()
    {
        if (energyReserve < muscles[0].force)
        {
            return true;
        }

        return false;
    }

    public bool CheckIfCanReproduce()
    {
        if (CostToCreate <= energyToReproduce)
        {
            return true;
        }

        return false;
    }

    public int CalculateCost()
    {
        int childEnergyCost = 0;
        foreach (var bone in bones)
        {
            childEnergyCost += (int)(bone.energy * bone.length);
        }

        childEnergyCost += muscles.Count * muscles[0].energy;
        childEnergyCost += joints.Count * joints[0].energy;
        return childEnergyCost;
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    void FixedUpdate()
    {
        CheckIfDead();
        foreach (var muscle in muscles)
        {
            UpdateLinePoints(muscle);
        }

        if (Time.time != 0 & Time.time % 5 == 0)
        {
            secondLatestPosition = latestPosition;
            latestPosition = joints[0].transform.position;
        }
       
        //DrawRect(bottomLeftDebug, topRightDebug);

        // if (entityId == 1)
        // {
        //     Time.timeScale = 0;
        // }

       
    }
}
