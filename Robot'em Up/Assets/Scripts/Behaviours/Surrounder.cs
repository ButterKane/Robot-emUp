using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surrounder : MonoBehaviour
{
    public List<Transform> points;
    public Dictionary<int, Transform> pointsDic = new Dictionary<int, Transform>();
    public float minDistanceFromCenter = 5f;
    public float maxDistanceFromCenter = 10f;
    public float minimalDistanceToFollow = 3f;
    private Dictionary<int, SurroundingPoint> pointsScripts = new Dictionary<int, SurroundingPoint>();
    public Transform playerTransform;

    void Awake()
    {
        for (int i = 0; i < points.Count; i++)
        {
            pointsDic.Add(i, points[i]);
            pointsScripts[i] = pointsDic[i].GetComponent<SurroundingPoint>();
        }
    }

    private void Update()
    {
        StayOnPlayerPos();
        FaceEnemyMiddlePoint();
    }

    public Transform GetSurroundingPoint(Transform enemy)
    {
        List<Transform> availablePoints = GetAvailablePoints(); // Get all the points that are not occupied

        if (availablePoints.Count <= 0) // check if there's no point available
        {
            return null;
        }

        Transform selectedPoint = null;
        float closestDistance = 1000;

        foreach (var point in availablePoints)
        {
            if ((point.transform.position - enemy.position).magnitude < closestDistance)
            {
                closestDistance = (point.transform.position - enemy.position).magnitude;
                selectedPoint = point;
            }
        }

        if (selectedPoint != null)
            pointsScripts[KeyByValue(pointsDic, selectedPoint)].isOccupied = true; // "activate" the selected point

        return selectedPoint;
    }

    public Vector3 GetAPositionFromPoint(Transform pointTransform)
    {
        Vector3 fromCenterToPoint = (pointTransform.position - transform.position).normalized;

        Vector3 positionFromCenter = transform.position + fromCenterToPoint * Random.Range(minDistanceFromCenter, maxDistanceFromCenter);

        Debug.DrawLine(transform.position, positionFromCenter, Color.green, 3);

        return positionFromCenter;
    }

    public List<Transform> GetAvailablePoints()
    {
        List<Transform> availablePoints = new List<Transform>();
        
        for (int i = 0; i < pointsScripts.Count; i++)
        {
            if (!pointsScripts[i].isOccupied && pointsScripts[i].gameObject.activeSelf)
                availablePoints.Add(points[i]);
        }

        return availablePoints;
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
        }
        else if (playerTransform == GameManager.i.playerTwo.transform)
        {
            pointToFace = GameManager.i.enemyManager.groupTwoMiddlePoint;
        }

        if ((pointToFace - transform.position).magnitude > minimalDistanceToFollow)
        {
            transform.LookAt(pointToFace);
        }

    }

}
