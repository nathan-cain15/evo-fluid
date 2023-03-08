using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject jointPrefab;
    public GameObject bonePrefab;
    public GameObject checkForOverlapPrefab;
    public GameObject barrier;
    public GameObject food;
    public List<Entity> entities;
    public int entityId = 0;

    public GameController()
    {
        entities = new List<Entity>();
    }
    
    private void CreateEntity(Vector3 position)
    {
        var entity = new GameObject("entity");
        entity.transform.Translate(position);
        entity.AddComponent<Entity>();
        var entityScript = entity.GetComponent<Entity>();
        entities.Add(entityScript);
        
        var joint = Instantiate(jointPrefab, position, quaternion.identity);
        
        joint.transform.parent = entity.transform;
        entityScript.firstJoint = joint.GetComponent<Joint>();
        
        entityScript.joints.Add(joint.GetComponent<Joint>());
        entityScript.bonePrefab = bonePrefab;
        entityScript.jointPrefab = jointPrefab;
        entityScript.checkForOverlapPrefab = checkForOverlapPrefab;
        entityScript.AddBone(entityScript.firstJoint, 100, 5);
        entityScript.AddBone(entityScript.firstJoint, 180, 5);
        entityScript.bones[0].num = 0;
        entityScript.bones[1].num = 1;
        
        entityScript.addMuscle(entityScript.bones[0], entityScript.bones[1]);
        entityScript.muscles[0].timeScale = 10f;

    }
    private void thirdEntity(Vector3 position)
    {
        var entity = new GameObject("entity");
        entity.transform.Translate(position);
        entity.AddComponent<Entity>();
        var entityScript = entity.GetComponent<Entity>();
        entities.Add(entityScript);
        var joint = Instantiate(jointPrefab, position, quaternion.identity);
        
        joint.transform.parent = entity.transform;
        entityScript.firstJoint = joint.GetComponent<Joint>();
        
        entityScript.joints.Add(joint.GetComponent<Joint>());
        entityScript.bonePrefab = bonePrefab;
        entityScript.jointPrefab = jointPrefab;
        entityScript.checkForOverlapPrefab = checkForOverlapPrefab;
        entityScript.AddBone(entityScript.firstJoint, 30, 5);
        entityScript.AddBone(entityScript.firstJoint, 180, 10);
        entityScript.AddBone(entityScript.firstJoint, 330, 5);
        entityScript.bones[0].num = 0;
        entityScript.bones[1].num = 1;
        //entityScript.AddJoint(entityScript.bones[0]);
        entityScript.addMuscle(entityScript.bones[0], entityScript.bones[1]);
        //entityScript.addMuscle(entityScript.bones[1], entityScript.bones[2]);

        //entityScript.AddBone(entityScript.joints[1], 180, 8);
        entityScript.addMuscle(entityScript.bones[1], entityScript.bones[2]);
        
        //entityScript.muscles[0].Push(10);
    }

    private void TestingCreateEntity(Vector3 position)
    {
        var entity = new GameObject("entity");
        entity.transform.Translate(position);
        entity.AddComponent<Entity>();
        var entityScript = entity.GetComponent<Entity>();
        entities.Add(entityScript);
        entityScript.entityId = entityId;
        entityId++;
        
        
        var joint = Instantiate(jointPrefab, position, quaternion.identity);
        joint.GetComponent<Joint>().jointId = 0;
        joint.transform.parent = entity.transform;
        entityScript.firstJoint = joint.GetComponent<Joint>();
        
        entityScript.joints.Add(joint.GetComponent<Joint>());
        entityScript.bonePrefab = bonePrefab;
        entityScript.jointPrefab = jointPrefab;
        entityScript.checkForOverlapPrefab = checkForOverlapPrefab;
        entityScript.AddBone(entityScript.firstJoint, 30, 5);
        entityScript.AddBone(entityScript.firstJoint, 180, 5);
        entityScript.AddBone(entityScript.firstJoint, 330, 5);
        entityScript.bones[0].num = 0;
        entityScript.bones[1].num = 1;
        
        entityScript.AddJoint(entityScript.bones[0]);
        entityScript.AddJoint(entityScript.bones[1]);
        entityScript.AddJoint(entityScript.bones[2]);
        
        entityScript.addMuscle(entityScript.bones[0], entityScript.bones[1]);
        entityScript.addMuscle(entityScript.bones[1], entityScript.bones[2]);
        entityScript.muscles[1].timeScale = 3f;
        entityScript.muscles[0].timeScale = 1f;

        // entityScript.muscles[0].forceOverTime = true;
        // entityScript.muscles[0].forceOverTimeTimestep = 0.06f;
        // entityScript.muscles[0].forceOverTimeForce = 1000f;
        // entityScript.muscles[0].forceOverTimeAmountOfTimes = 10;

        // var results = entityScript.returnSuitablePlacementPoint();
        //
        // if (results != null)
        // {
        //     GameObject duplicate = Instantiate(entity);
        //     var duplicateScript = duplicate.GetComponent<Entity>();
        //     entities.Add(duplicateScript);
        //     duplicate.transform.position = new Vector3(results.Value.x, results.Value.y, 0);
        // }

    }

    void TestReproduction(float time)
    {
        //Debug.Log(entities[0].bones[0].Rigidbody2D.velocity);
        if (time == 0)
        {
            return;
        }

        if (time % 10f == 0)
        {
            var child = entities[0].Reproduce();
            entities.Add(child);
            entities[0].muscles[0].timeScale = 1f;
            entities[0].muscles[1].timeScale = 3f;
            Destroy(entities[0].gameObject);
            entities.Remove(entities[0]);
        }
    }
    

    void ApplyMuscleForces(float time)
    {
        if (time == 0)
        {
            return;
        }
        foreach (var entity in entities)
        {
            foreach (var muscle in entity.muscles)
            {
                if (!muscle.forceOverTime && time % muscle.timeScale == 0)
                {
                    muscle.Pull(muscle.force);
                }
            }
        }
    }

    void ApplyMuscleForcesOverTime(float time)
    {
        if (time == 0)
        {
            return;
        }
        foreach (var entity in entities)
        {
            foreach (var muscle in entity.muscles)
            {
                if (muscle.forceOverTime)
                {
                    //Debug.Log(time % muscle.timeScale < muscle.forceOverTimeAmountOfTimes * muscle.forceOverTimeTimestep);
                    // Debug.Log(muscle.forceOverTimeTimestep * muscle.forceOverTimeAmountOfTimes);
                    // Debug.Log(time % muscle.forceOverTimeTimestep == 0);
                    if (time % muscle.timeScale < muscle.forceOverTimeAmountOfTimes * muscle.forceOverTimeTimestep && time % muscle.forceOverTimeTimestep == 0)
                    {
                        muscle.Pull(muscle.forceOverTimeForce);
                    }
                    
                }
            }
        }
    }

    void spawnFood(float time, int foodRate, Vector2 vec1, Vector2 vec2)
    {
        if (time % 1 == 0)
        {
            
        }
    }

    void spawnStartingFood(int amountOfFood, int energyAmountPerFood, Vector2 vec1, Vector2 vec2)
    {
        var length = vec2.x - vec1.x;
        var height = vec2.y - vec1.y;
        for (int i = 0; i < amountOfFood; i++)
        {
            var food = Instantiate(this.food);
            food.AddComponent<Food>();
            food.transform.position = new Vector2(UnityEngine.Random.Range(vec1.x + 1, vec2.x - 1), UnityEngine.Random.Range(vec1.y + 1, vec2.y - 1));
            food.GetComponent<Food>().energy = energyAmountPerFood;
        }
    }

    void CreateBarrier(Vector2 vec1, Vector2 vec2)
    {
        var length = vec2.x - vec1.x;
        var height = vec2.y - vec1.y;
        var barrier1 = Instantiate(barrier);
        var barrier2 = Instantiate(barrier);
        var barrier3 = Instantiate(barrier);
        var barrier4 = Instantiate(barrier);
        barrier1.transform.position = new Vector3(vec1.x, vec1.y + height * 0.5f);
        barrier1.transform.localScale = new Vector3(1, height, 0);

        barrier2.transform.position = new Vector3(vec1.x + length * 0.5f, vec2.y, 0);
        barrier2.transform.localScale = new Vector3(length, 1, 0);
        
        barrier3.transform.position = new Vector3(vec2.x, vec1.y + height * 0.5f);
        barrier3.transform.localScale = new Vector3(1, height, 0);

        barrier4.transform.position = new Vector3(vec1.x + length * 0.5f, vec1.y, 0);
        barrier4.transform.localScale = new Vector3(length, 1, 0);


    }
    
    void Start()
    {
        var point = new Vector3(0, 5, 0);
        var otherPoint = new Vector3(10, 0, 0);
        var bottomLeft = new Vector2(-100, -100);
        var topRight = new Vector2(100, 100);
        TestingCreateEntity(point);
        CreateBarrier(bottomLeft, topRight);
        //spawnStartingFood(40, 10, bottomLeft, topRight);



    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Debug.DrawLine(entities[0].bones[0].Rigidbody2D.velocity, new Vector3(0, 0, 0));
        //Debug.Log(Time.time);
        TestReproduction(Time.time);
        foreach (var entity in entities)
        {
            foreach (var bone in entity.bones)
            {
                bone.CalculateViscosityVelocity();
            }
        }

        //ApplyMuscleForces(Time.time);
        //ApplyMuscleForcesOverTime(Time.time);
        
    }
}
 