using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class Food : MonoBehaviour
{
    public int energy = 20000;
    public GameController GameController;

    public void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.GetComponent<Joint>() != null)
        {
            other.GetComponent<Joint>().entity.energyToReproduce += energy;
        }
        else
        {
            other.GetComponent<Bone>().entity.energyToReproduce += energy;
        }
        GameController.numOfFood--;
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}