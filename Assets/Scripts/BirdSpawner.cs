using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<lb_BirdController>().SpawnAmount(10);
    }
}
