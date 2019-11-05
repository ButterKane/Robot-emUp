using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surrounder : MonoBehaviour
{
    [Separator("Surround points listing")]
    public List<Transform> points;
    public Dictionary<int, Transform> pointsDic = new Dictionary<int, Transform>();
    private Dictionary<int, SurroundingPoint> pointsScripts = new Dictionary<int, SurroundingPoint>();

    [Separator("Auto-Assigned Variables")]
    public Transform playerTransform;
    public List<EnemyBehaviour> closestEnemies;

    [Separator("Variables")]
    public float minimalDistanceToFollow = 3f;  // distance before the points stop orienting themselves to face enemy group
    
    //Private
    private bool attributing = false;   // this one is used to attribute enemies to points. It's always true for now, but prepared to change


    void Awake()
    {
        for (int i = 0; i < points.Count; i++)
        {
            pointsDic.Add(i, points[i]);
            pointsScripts[i] = pointsDic[i].GetComponent<SurroundingPoint>();
        }
        StartCoroutine(SequenceEnemiesAttribution());
    }

    private void Update()
    {
        StayOnPlayerPos();
        FaceEnemyMiddlePoint();
    }

    public IEnumerator SequenceEnemiesAttribution()
    {
        attributing = true;
        do
        {
            DeactivateUnreachablePoints();
            AttributeClosestEnemies();
            yield return new WaitForSeconds(1f);
        } while (attributing == true);
    }

    public void AttributeClosestEnemies()
    {
        for (int i = 0; i < pointsScripts.Count; i++)
        {
            pointsScripts[i].closestEnemy = null;
        }

        foreach (var enemy in closestEnemies)
        {
            enemy.ClosestSurroundPoint = null;

            float closestDistance = Mathf.Infinity;

            for (int i = 0; i < pointsDic.Count; i++)
            {
                if (!pointsDic[i].gameObject.activeSelf)    // abort for this point if it's deactivated
                {
                    continue;
                }

                if (pointsScripts[i].closestEnemy == null)
                {
                    if ((enemy.transform.position - pointsDic[i].position).magnitude < closestDistance)
                    {
                        closestDistance = (enemy.transform.position - pointsDic[i].position).magnitude; // We keep the distance in memory

                        if (enemy.ClosestSurroundPoint) // if the enemy already had a closest point, make sure this point forgets the enemy as well
                        {
                            pointsScripts[KeyByValue(pointsDic, enemy.ClosestSurroundPoint)].closestEnemy = null;
                        }

                        pointsScripts[i].closestEnemy = enemy;

                        enemy.ClosestSurroundPoint = pointsDic[i]; // attribute the closest surround point to the enemy
                    }
                }
            }

            if (enemy.ClosestSurroundPoint)
            {
                Debug.DrawLine(enemy.transform.position, enemy.ClosestSurroundPoint.transform.position, Color.blue, 1f) ;
            }
        }
    }

    public void DeactivateUnreachablePoints()
    {
        RaycastHit hit;
        int layerMask = 1 << 12; // Layer 12 = Environment

        for (int i = 0; i < pointsScripts.Count; i++)
        {
            if (Physics.Raycast(transform.position, pointsScripts[i].transform.position - transform.position, out hit, (pointsScripts[i].transform.position - transform.position).magnitude, layerMask))
            {
                pointsScripts[i].gameObject.SetActive(false);
            }
            else
            {
                pointsScripts[i].gameObject.SetActive(true);
            }
        }
    }

    public int KeyByValue(Dictionary<int, Transform> dict, Transform val)
    {
        int key = default;
        foreach (KeyValuePair<int, Transform> pair in dict)
        {
            if (EqualityComparer<Transform>.Default.Equals(pair.Value, val))
            {
                key = pair.Key;
                break;
            }
        }
        return key;
    }

    public void StayOnPlayerPos()
    {
        if (playerTransform)
            transform.position = playerTransform.position;
    }

    public void FaceEnemyMiddlePoint()
    {
        Vector3 pointToFace = new Vector3();

        if (playerTransform == GameManager.i.playerOne.transform)
        {
            pointToFace = GameManager.i.enemyManager.groupOneMiddlePoint;
            closestEnemies = GameManager.i.enemyManager.enemyGroupOne;
        }
        else if (playerTransform == GameManager.i.playerTwo.transform)
        {
            pointToFace = GameManager.i.enemyManager.groupTwoMiddlePoint;
            closestEnemies = GameManager.i.enemyManager.enemyGroupTwo;
        }

        if ((pointToFace - transform.position).magnitude > minimalDistanceToFollow)
        {
            transform.LookAt(SwissArmyKnife.GetFlattedDownPosition(pointToFace, transform.position));
        }



    }

}
