using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surrounder : MonoBehaviour
{
    public List<Transform> points;
    public Dictionary <int, Transform> pointsDic =  new Dictionary<int, Transform>();
    public float maxDistanceFromCenter = 10f;
    private Dictionary <int, SurroundingPoint> pointsScripts = new Dictionary<int, SurroundingPoint>();
    
    void Awake()
    {
        RaycastHit hit;
        for (int i = 0; i < points.Count; i++)
        {
            pointsDic.Add(i, points[i]);
            pointsScripts[i] = pointsDic[i].GetComponent<SurroundingPoint>();
        }
        for (int j = 0; j < pointsDic.Count; j++)
        {
            int layerMask = 1 << 12; // Layer 12 = Environment
            if (Physics.Raycast(transform.position, pointsDic[j].position - transform.position, out hit, (pointsDic[j].position - transform.position).magnitude, layerMask)) 
            {
                Debug.Log("deactivated at index " + j);
                pointsDic[j].gameObject.SetActive(false);   // Deactivate points that spawn inside a wall or environment 
            }
        }
    }
    
    public Transform GetSurroundingPoint()
    {
        List<Transform> availablePoints = GetAvailablePoints(); // Get all the points that are not occupied

        int selectedAvailableIndex = Random.Range(0, availablePoints.Count);

        Transform selectedPoint = availablePoints[selectedAvailableIndex];

        pointsScripts[KeyByValue(pointsDic, selectedPoint)].isOccupied = true; // "activate" the selected point

        return selectedPoint;
    }

    public Vector3 GetAPositionFromPoint(Transform pointTransform)
    {
        Vector3 fromCenterToPoint = (pointTransform.position - transform.position).normalized;

        Vector3 positionFromCenter = transform.position + fromCenterToPoint * Random.Range(10, maxDistanceFromCenter);

        Debug.DrawLine(transform.position, positionFromCenter, Color.green, 3);

        return positionFromCenter;
    }

    public List<Transform> GetAvailablePoints()
    {
        List<Transform> availablePoints = new List<Transform>();

        for (int i = 0; i < pointsScripts.Count; i++)
        {
            if(!pointsScripts[i].isOccupied && pointsScripts[i].gameObject.activeSelf)
            {
                availablePoints.Add(points[i]);
            }
        }

        return availablePoints;
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
}
