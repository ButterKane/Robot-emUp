using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Analytics;

public class DestructibleObject : Dummy
{
    public GameObject chosenMesh;
    public GameObject[] meshes;
    private GameObject child;
    private MeshCollider meshCollider;
    // Start is called before the first frame update
    void Awake()
    {
        if (chosenMesh != null)
        {
            child = Instantiate(chosenMesh, transform);
        }
        else
        {
            child = Instantiate(meshes[Random.Range(0, meshes.Length - 1)], transform);
        }
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = child.GetComponent<MeshFilter>().sharedMesh;
		meshCollider.convex = true;
        hitCount_access = 0;
    }

    public override void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default)
    {
        transform.DOShakeScale(0.2f, 0.1f, 200).OnComplete(ResetScale);
        hitCount_access++;
        if (hitCount_access >= maxHealth)
        {
            DestroyTheObject();
        }
        else
        {
            FeedbackManager.SendFeedback(hitEvent, this, transform.position - _impactVector *0.5f, transform.up, transform.up ) ;
        }
    }

    public void DestroyTheObject()
    {
		Analytics.CustomEvent("ObjectDestroyed", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
		FeedbackManager.SendFeedback(deathEvent, this, transform.position, transform.up, transform.up);
        Destroy(gameObject);
    }
}
