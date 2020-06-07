using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MyBox;

[RequireComponent(typeof(Collider))]
public class EnemyChecker : MonoBehaviour
{

    public List<EnemyBehaviour> enemytoCheck;
    public bool clearedCheck;
    public bool cleared;
    public UnityEvent onTriggerEnterAction;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!cleared)
        {
            clearedCheck = true;
            foreach (var item in enemytoCheck)
            {
                if (item != null)
                {
                    if (item.GetHealth() > 0)
                    {
                        clearedCheck = false;
                    }
                }
            }
            if (clearedCheck)
            {
                Cleared();
                cleared = true;
            }
        }
    }
    void OnBecameVisible()
    {
    }
    void OnBecameInvisible()
    {

    }
    void Cleared()
    {
        onTriggerEnterAction.Invoke();

    }

    void OnTriggerStay (Collider _other)
    {
        if (_other.gameObject.GetComponent<EnemyBehaviour>())
        {
            EnemyBehaviour enemy = _other.gameObject.GetComponent<EnemyBehaviour>();
            if (!enemytoCheck.Contains(enemy))
            {
                enemytoCheck.Add(enemy);
            }
        }
  }
}
