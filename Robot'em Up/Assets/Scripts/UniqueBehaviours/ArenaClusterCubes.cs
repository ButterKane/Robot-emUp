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
    [Header("References")]
    public DoubleBoolArray[] row;
    public GameObject cubeFromCluster;
    [Space]
    [Header("VariablesToTweak")]
    public int nbUsedLayouts;
    public float columnOffset;
    public float rowOffset;
    public float heightOffset;
    public float movingCubeTimeOffset;
    public float movingCubeUpMinTime;
    public float movingCubeUpMaxTime;
    public float movingCubeDownMinTime;
    public float movingCubeDownMaxTime;
    public AnimationCurve movingCubeUpCurve;
    public AnimationCurve movingCubeDownCurve;

    [Space]
    [Header("ClusterLayouts")]
    public DoubleBoolArray[] layout0;
    public DoubleBoolArray[] layout1;
    public DoubleBoolArray[] layout2;
    public DoubleBoolArray[] layout3;
    public DoubleBoolArray[] layout4;

    int layoutState;
    Transform[,] cubesArray;

    private void Start()
    {
        SpawnClusterCubes();
        layoutState = 0;
    }

    [ContextMenu("SpawnClusterCubes")]
    public void SpawnClusterCubes()
    {
        //Resetting values
        layoutState = 0;
        cubesArray = new Transform[row.Length, row[0].column.Length];

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
                    //ROWS
                    spawnPosition += transform.forward * (row.Length-1) * 0.5f * rowOffset;
                    spawnPosition -= transform.forward * rowOffset * j;
                    //COLUMNS
                    spawnPosition -= transform.right * (row[j].column.Length-1) * 0.5f * columnOffset;
                    spawnPosition += transform.right * columnOffset * k;

                    GameObject newCube = Instantiate(cubeFromCluster, spawnPosition - transform.up * heightOffset, Quaternion.identity, transform);
                    cubesArray[j, k] = newCube.transform;
                }
            }
        }
    }


    [ContextMenu("NextClusterLayout")]
    public void NextClusterLayout()
    {
        if(layoutState < nbUsedLayouts)
        {
            DeactivateCurrentLayout();
            layoutState++;
            Invoke("ActivateNewLayout", movingCubeTimeOffset + movingCubeDownMaxTime);
        }
    }

    IEnumerator MoveCube(bool _up, int _row, int _column)
    {
        //SET UP
        Vector3 startPos = cubesArray[_row,_column].position;
        Vector3 endPos = startPos;
        if (_up)
            endPos.y = transform.position.y + heightOffset;
        else
            endPos.y = transform.position.y - heightOffset;
        float timeOffset = Random.Range(0f, movingCubeTimeOffset);
        float movingTime = 0;
        if(_up)
            movingTime = Random.Range(movingCubeUpMinTime, movingCubeUpMaxTime);
        else
            movingTime = Random.Range(movingCubeDownMinTime, movingCubeDownMaxTime);
        float movingTimer = 0;

        yield return new WaitForSeconds(timeOffset);

        //MOVEMENT
        while (movingTimer < movingTime)
        {
            movingTimer += Time.deltaTime;
            if(_up)
                cubesArray[_row, _column].position = Vector3.Lerp(startPos, endPos, movingCubeUpCurve.Evaluate(movingTimer/movingTime));
            else
                cubesArray[_row, _column].position = Vector3.Lerp(startPos, endPos, movingCubeDownCurve.Evaluate(movingTimer / movingTime));
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    void DeactivateCurrentLayout()
    {
        for (int j = 0; j < row.Length; j++)
        {
            for (int k = 0; k < row[j].column.Length; k++)
            {
                if(cubesArray[j, k] != null)
                {
                    Vector3 downPosition = cubesArray[j, k].position;
                    downPosition.y = transform.position.y - heightOffset;
                    switch (layoutState)
                    {
                        case 0:
                            if (layout0[j].column[k])
                            {
                                StartCoroutine(MoveCube(false, j, k));
                                //cubesArray[j, k].position = downPosition;
                            }
                            break;
                        case 1:
                            if (layout1[j].column[k])
                            {
                                StartCoroutine(MoveCube(false, j, k));
                                //cubesArray[j, k].position = downPosition;
                            }
                            break;
                        case 2:
                            if (layout2[j].column[k])
                            {
                                StartCoroutine(MoveCube(false, j, k));
                                //cubesArray[j, k].position = downPosition;
                            }
                            break;
                        case 3:
                            if (layout3[j].column[k])
                            {
                                StartCoroutine(MoveCube(false, j, k));
                                //cubesArray[j, k].position = downPosition;
                            }
                            break;
                        case 4:
                            if (layout4[j].column[k])
                            {
                                StartCoroutine(MoveCube(false, j, k));
                                //cubesArray[j, k].position = upPosition;
                            }
                            break;
                    }
                }                
            }
        }
    }

    public void ActivateNewLayout()
    {
        for (int j = 0; j < row.Length; j++)
        {
            for (int k = 0; k < row[j].column.Length; k++)
            {
                if (cubesArray[j, k] != null)
                {
                    Vector3 upPosition = cubesArray[j, k].position;
                    upPosition.y = transform.position.y + heightOffset;
                    switch (layoutState)
                    {
                        case 0:
                            if (layout0[j].column[k])
                            {
                                StartCoroutine(MoveCube(true, j, k));
                                //cubesArray[j, k].position = upPosition;
                            }
                            break;
                        case 1:
                            if (layout1[j].column[k])
                            {
                                StartCoroutine(MoveCube(true, j, k));
                                //cubesArray[j, k].position = upPosition;
                            }
                            break;
                        case 2:
                            if (layout2[j].column[k])
                            {
                                StartCoroutine(MoveCube(true, j, k));
                                //cubesArray[j, k].position = upPosition;
                            }
                            break;
                        case 3:
                            if (layout3[j].column[k])
                            {
                                StartCoroutine(MoveCube(true, j, k));
                                //cubesArray[j, k].position = upPosition;
                            }
                            break;
                        case 4:
                            if (layout4[j].column[k])
                            {
                                StartCoroutine(MoveCube(true, j, k));
                                //cubesArray[j, k].position = upPosition;
                            }
                            break;
                    }
                }
            }
        }
    }
}
