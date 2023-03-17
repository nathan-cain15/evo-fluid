using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using JetBrains.Annotations;
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
    public Vector2 bottomLeft;
    public Vector2 topRight;

    public GameController()
    {
        entities = new List<Entity>();
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
        entityScript.AddBone(entityScript.firstJoint, 90, 5);
        entityScript.AddBone(entityScript.firstJoint, 180, 5);
        entityScript.AddBone(entityScript.firstJoint, 270, 5);
        entityScript.bones[0].num = 0;
        entityScript.bones[1].num = 1;
        
        entityScript.AddJoint(entityScript.bones[0]);
        entityScript.AddJoint(entityScript.bones[1]);
        entityScript.AddJoint(entityScript.bones[2]);
        
        entityScript.addMuscle(entityScript.bones[0], entityScript.bones[1]);
        entityScript.addMuscle(entityScript.bones[1], entityScript.bones[2]);
        entityScript.muscles[1].timeScale = 2f;
        entityScript.muscles[0].timeScale = 2f;

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

        if (time % 2f == 0)
        {
            var child = entities[0].Reproduce();
            if (child != null)
            {
                child.entityId = entityId;
                entityId++;
                entities.Add(child);
            }
           
            //Destroy(entities[0].gameObject);
            //entities.Remove(entities[0]);
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
                if (!muscle.forceOverTime && Decimal.Round((decimal)time, 2) % Decimal.Round((decimal)muscle.timeScale) == 0)
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
                        muscle.Push(muscle.forceOverTimeForce);
                    }
                    
                }
            }
        }
    }
    

    [CanBeNull]
    Entity StarterEntity(Vector2 position)
    {
        var entity = new GameObject("entity");
        entity.transform.Translate(position);
        entity.AddComponent<Entity>();
        var entityScript = entity.GetComponent<Entity>();
        
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
        entityScript.AddBone(entityScript.firstJoint, 150, 5);
        entityScript.AddJoint(entityScript.bones[0]);
        entityScript.AddJoint(entityScript.bones[1]);
        if (entityScript.bones.Count != 2)
        {
            Destroy(entityScript.gameObject);
            return null;
        }
        entities.Add(entityScript);
        
        entityScript.addMuscle(entityScript.bones[0], entityScript.bones[1]);
    
        entityScript.CostToCreate = entityScript.CalculateCost(); 
        entityScript.transform.Rotate(new Vector3(0, 0, UnityEngine.Random.Range(0, 361)));
        
        Physics2D.SyncTransforms();
        
        return entityScript;
    }

    void SpawnStartingEntities(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 position = new Vector2(UnityEngine.Random.Range(bottomLeft.x + 4, topRight.x - 4), UnityEngine.Random.Range(bottomLeft.y + 4, topRight.y - 4));
            var entity = StarterEntity(position);
            if (entity == null)
            {
                i--;
            }
            
        }
        
    }

    void spawnFood(float time, int foodRate, int amountSpawned)
    {
        // note - the energy amount per food is set in the prefab
        if (time % foodRate == 0)
        {
            for (int i = 0; i < amountSpawned; i++)
            {
                var food = Instantiate(this.food);
                food.transform.position = new Vector2(UnityEngine.Random.Range(bottomLeft.x + 1, topRight.x - 1), UnityEngine.Random.Range(bottomLeft.y + 1, topRight.y - 1));
            }
        }
    }

    void spawnStartingFood(int amountOfFood)
    {
        // note - the energy amount per food is set in the prefab
        var length = topRight.x - bottomLeft.x;
        var height = topRight.y - bottomLeft.y;
        for (int i = 0; i < amountOfFood; i++)
        {
            var food = Instantiate(this.food);
            food.transform.position = new Vector2(UnityEngine.Random.Range(bottomLeft.x + 1, topRight.x - 1), UnityEngine.Random.Range(bottomLeft.y + 1, topRight.y - 1));
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

    void CheckEntitiesToReproduce()
    {
        for (int i = 0; i < entities.Count; i++) 
        {
            if (entities[i].CheckIfCanReproduce())
            {
                var child = entities[i].Reproduce();

                if (child != null)
                {
                    entities.Add(child);
                    entities[i].energyToReproduce -= entities[i].CostToCreate;

                }
                else
                {
                    entities[i].isWaitingToReproduce = true;
                }
            }
        }
    }

    void CheckIfAnyEntityIsWaitingToReproduce()
    {
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i].isWaitingToReproduce)
            {
                var child = entities[i].Reproduce();

                if (child != null)
                {
                    entities.Add(child);
                    entities[i].energyToReproduce -= entities[i].CostToCreate;
                    entities[i].isWaitingToReproduce = false;
                }
            }
        }
    }
    
    void Start()
    {
        var point = new Vector3(0, 5, 0);
        var otherPoint = new Vector3(10, 0, 0);
        var varBottomLeft = new Vector2(-200, -200);
        var varTopRight = new Vector2(200, 200);
        this.bottomLeft = varBottomLeft;
        this.topRight = varTopRight;
        StarterEntity(point);
        CreateBarrier(bottomLeft, topRight);
        SpawnStartingEntities(5);
        spawnStartingFood(5000);
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i].CheckIfDead())
            {
                Destroy(entities[i].gameObject);
                entities.Remove(entities[i]);
            }
        }
        ApplyMuscleForces(Time.time);
       // Debug.Log(Time.time);
        CheckEntitiesToReproduce();
        CheckIfAnyEntityIsWaitingToReproduce();
        //TestReproduction(Time.time);
        spawnFood(Time.time, 1, 30);
        
        // debug
        foreach (var entity in entities)
        {
            foreach (var muscle in entity.muscles)
            {
                if (muscle.timeScale == 2.98f)
                {
                    //Debug.DrawLine(new Vector3(0, 0,0), muscle.transform.position);
                }
            }
        }

        // if (Decimal.Round((decimal)Time.time, 2) % Decimal.Round(2.981111M, 2) == 0)
        // {
        //     Debug.Log(Decimal.Round(2.98111M));
        //     Debug.Log(Time.time);
        // }

    }
}
 