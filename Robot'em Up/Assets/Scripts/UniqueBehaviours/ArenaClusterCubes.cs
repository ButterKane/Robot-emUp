using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DoubleBoolArray
{
    public bool[] column;
}

public class ArenaClusterCubes : MonoBehaviour
{
    public DoubleBoolArray[] row;
    public GameObject cubeFromCluster;
    public float columnOffset;
    public float rowOffset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Do Something")]
    public void aze()
    {
        for (int i = transform.childCount-1; i > -1; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        for (int j = 0; j < row.Length; j++)
        {
            for (int k = 0; k < row[j].column.Length; k++)
            {
                if (row[j].column[k])
                {
                    Vector3 spawnPosition = transform.position;
                    spawnPosition -= transform.forward * (row.Length-1) * 0.5f * rowOffset;
                    spawnPosition += transform.forward * rowOffset * j;
                    spawnPosition -= transform.right * (row[j].column.Length-1) * 0.5f * columnOffset;
                    spawnPosition += transform.right * columnOffset * k;
                    Instantiate(cubeFromCluster, spawnPosition, Quaternion.identity, transform);
                }

            }
        }


        print("hey");
    }
}
