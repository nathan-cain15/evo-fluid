using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject jointPrefab;
    public GameObject bonePrefab;
    public GameObject checkForOverlapPrefab;
    public List<Entity> entities;

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
        entityScript.AddBone(entityScript.firstJoint, 100, 10);
        entityScript.AddBone(entityScript.firstJoint, 180, 10);
        entityScript.bones[0].num = 0;
        entityScript.bones[1].num = 1;
        //entityScript.AddJoint(entityScript.bones[0]);
        entityScript.addMuscle(entityScript.bones[0], entityScript.bones[1]);

        //entityScript.AddBone(entityScript.joints[1], 180, 8);
        //entityScript.addMuscle(entityScript.bones[1], entityScript.bones[2]);
        
        //entityScript.muscles[0].Push(10);
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
        
        var joint = Instantiate(jointPrefab, position, quaternion.identity);
        
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
        //entityScript.AddJoint(entityScript.bones[0]);
        entityScript.addMuscle(entityScript.bones[0], entityScript.bones[1]);
        //entityScript.addMuscle(entityScript.bones[1], entityScript.bones[2]);

        //entityScript.AddBone(entityScript.joints[1], 180, 8);
        entityScript.addMuscle(entityScript.bones[1], entityScript.bones[2]);
        entityScript.muscles[1].timeScale = 4.5f;

        //entityScript.muscles[0].Push(10);

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
                if (time % muscle.timeScale == 0)
                {
                    muscle.Pull(muscle.force);
                }
            }
        }
    }
    
    void Start()
    {
        var point = new Vector3(0, 5, 0);
        var otherPoint = new Vector3(10, 0, 0);
        TestingCreateEntity(point);
        //CreateEntity(otherPoint);
        //entities[0].muscles[0].Push(500);
        //entities[1].muscles[0].Pull(500);

        

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ApplyMuscleForces(Time.time);
        //entities[0].muscles[1].Pull(10);
    }
}
 