using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag=="Player"){
            EventManager.OnTreasurePickedUp(other.gameObject);
            Destroy(gameObject);
        }
        // Debug.Log("Treasure picked!!!");
    }
}
