using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadExplosionTA : MonoBehaviour
{

    public Rigidbody[] groupA;
    public Rigidbody[] groupB;
    public Rigidbody[] groupC;
    public Rigidbody[] groupD;
    public float delayBeforeA;
    public float delayBeforeB;
    public float delayBeforeC;
    public float delayBeforeD;
    public Transform positionExplosionA;
    public Transform positionExplosionB;
    public Transform positionExplosionC;
    public Transform positionExplosionD;
    public float minExplosionForce;
    public float maxExplosionForce;
    public float explosionRadius;
    public float maxRotationSpeed;
    public BoxCollider colliderAB;
    public BoxCollider colliderCD;
    public float delayBeforeColliderOff;

    public void StartExplosionSequence()
    {
        StartCoroutine(ExplosionCoroutine());
    }

    IEnumerator ExplosionCoroutine()
    {
        yield return new WaitForSeconds(delayBeforeA);
        FeedbackManager.SendFeedback("event.MassiveDestrObjectDeath", this, positionExplosionA.position, Vector3.up, Vector3.up);
        for (int i = 0; i < groupA.Length; i++)
        {
            groupA[i].isKinematic = false;
            groupA[i].AddTorque(new Vector3(Random.Range(0.1f, 1) * maxRotationSpeed, Random.Range(0.1f, 1) * maxRotationSpeed, Random.Range(0.1f, 1) * maxRotationSpeed));
            Destroy(groupA[i].gameObject, 5);
            //groupA[i].AddExplosionForce(Random.Range(minExplosionForce, maxExplosionForce), positionExplosionA.position, explosionRadius, 1.5f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(delayBeforeB);
        FeedbackManager.SendFeedback("event.MassiveDestrObjectDeath", this, positionExplosionB.position, Vector3.up, Vector3.up);
        for (int i = 0; i < groupB.Length; i++)
        {
            groupB[i].isKinematic = false;
            groupB[i].AddTorque(new Vector3(Random.Range(0.1f, 1) * maxRotationSpeed, Random.Range(0.1f, 1) * maxRotationSpeed, Random.Range(0.1f, 1) * maxRotationSpeed));
            Destroy(groupB[i].gameObject, 5);
            //groupB[i].AddExplosionForce(Random.Range(minExplosionForce, maxExplosionForce), positionExplosionB.position, explosionRadius, 1.5f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(delayBeforeColliderOff);
        colliderAB.enabled = false;

        yield return new WaitForSeconds(delayBeforeC- delayBeforeColliderOff);
        FeedbackManager.SendFeedback("event.MassiveDestrObjectDeath", this, positionExplosionC.position, Vector3.up, Vector3.up);
        for (int i = 0; i < groupC.Length; i++)
        {
            groupC[i].isKinematic = false;
            groupC[i].AddTorque(new Vector3(Random.Range(0.1f, 1) * maxRotationSpeed, Random.Range(0.1f, 1) * maxRotationSpeed, Random.Range(0.1f, 1) * maxRotationSpeed));
            Destroy(groupC[i].gameObject, 5);
            //groupC[i].AddExplosionForce(Random.Range(minExplosionForce, maxExplosionForce), positionExplosionC.position, explosionRadius, 1.5f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(delayBeforeD);
        FeedbackManager.SendFeedback("event.MassiveDestrObjectDeath", this, positionExplosionD.position, Vector3.up, Vector3.up);
        for (int i = 0; i < groupD.Length; i++)
        {
            groupD[i].isKinematic = false;
            groupD[i].AddTorque(new Vector3(Random.Range(0.1f, 1) * maxRotationSpeed, Random.Range(0.1f, 1) * maxRotationSpeed, Random.Range(0.1f, 1) * maxRotationSpeed));
            Destroy(groupD[i].gameObject, 5);
            //groupD[i].AddExplosionForce(Random.Range(minExplosionForce, maxExplosionForce), positionExplosionD.position, explosionRadius, 1.5f, ForceMode.Impulse);
        }
        
        yield return new WaitForSeconds(delayBeforeColliderOff);
        colliderCD.enabled = false;

        yield return null;
    }
}
