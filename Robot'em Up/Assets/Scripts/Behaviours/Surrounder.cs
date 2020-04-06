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
    private List<EnemyBehaviour> closestEnemies = new List<EnemyBehaviour>();

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
        StartCoroutine(SequenceEnemiesAttribution_C());
    }

    private void Update()
    {
        StayOnPlayerPos();
        FaceEnemyMiddlePoint();
    }

    #region Private functions
    private void FaceEnemyMiddlePoint()
    {
        Vector3 i_pointToFace = new Vector3();

        if (playerTransform == GameManager.playerOne.transform)
        {
            i_pointToFace = EnemyManager.i.groupOneMiddlePoint;
            closestEnemies = EnemyManager.i.enemyGroupOne;
        }
        else if (playerTransform == GameManager.playerTwo.transform)
        {
            i_pointToFace = EnemyManager.i.groupTwoMiddlePoint;
            closestEnemies = EnemyManager.i.enemyGroupTwo;
        }

        if ((i_pointToFace - transform.position).magnitude > minimalDistanceToFollow)
        {
            transform.LookAt(SwissArmyKnife.GetFlattedDownPosition(i_pointToFace, transform.position));
        }
    }

    private void StayOnPlayerPos()
    {
        if (playerTransform)
            transform.position = playerTransform.position;
    }

    private void AttributeClosestEnemies()
    {
        for (int i = 0; i < pointsScripts.Count; i++)
        {
            pointsScripts[i].closestEnemy = null;   // empty the list
        }

        foreach (var enemy in closestEnemies)
        {
            enemy.closestSurroundPoint = null;

            float i_closestDistance = Mathf.Infinity;

            for (int i = 0; i < pointsDic.Count; i++) // for all points available, check which is the closest
            {
                if (!pointsDic[i].gameObject.activeSelf)    // abort for this point if it's deactivated
                {
                    continue;
                }

                if (pointsScripts[i].closestEnemy == null)
                {
                    if ((enemy.transform.position - pointsDic[i].position).magnitude < i_closestDistance)
                    {
                        i_closestDistance = (enemy.transform.position - pointsDic[i].position).magnitude; // We keep the distance in memory

                        if (enemy.closestSurroundPoint) // if the enemy already had a closest point, make sure this point forgets the enemy as well
                        {
                            pointsScripts[KeyByValue(pointsDic, enemy.closestSurroundPoint)].closestEnemy = null;
                        }

                        pointsScripts[i].closestEnemy = enemy;

                        enemy.closestSurroundPoint = pointsDic[i]; // attribute the closest surround point to the enemy
                    }
                }
            }

            if (enemy.closestSurroundPoint)
            {
                Debug.DrawLine(enemy.transform.position, enemy.closestSurroundPoint.transform.position, Color.blue, 1f);
            }
        }
    }

    private void DeactivateUnreachablePoints()
    {
        RaycastHit hit;
        int i_layerMask = 1 << 12; // Layer 12 = Environment

        for (int i = 0; i < pointsScripts.Count; i++)
        {
            if (Physics.Raycast(transform.position, pointsScripts[i].transform.position - transform.position, out hit, (pointsScripts[i].transform.position - transform.position).magnitude, i_layerMask))
            {
                pointsScripts[i].gameObject.SetActive(false);
            }
            else
            {
                pointsScripts[i].gameObject.SetActive(true);
            }
        }
    }

    private int KeyByValue(Dictionary<int, Transform> _dict, Transform _val)
    {
        int i_key = default;
        foreach (KeyValuePair<int, Transform> pair in _dict)
        {
            if (EqualityComparer<Transform>.Default.Equals(pair.Value, _val))
            {
                i_key = pair.Key;
                break;
            }
        }
        return i_key;
    }
    #endregion

    #region IEnumerator functions
    public IEnumerator SequenceEnemiesAttribution_C()
    {
        attributing = true;
        do
        {
            DeactivateUnreachablePoints();
            AttributeClosestEnemies();
            yield return new WaitForSeconds(1f);
        } while (attributing == true);
    }
    #endregion
}
