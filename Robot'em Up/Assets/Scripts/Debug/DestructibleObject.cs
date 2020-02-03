using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : Dummy
{
    public GameObject[] meshes;
    private GameObject child;
    private MeshCollider meshCollider;
    // Start is called before the first frame update
    void Start()
    {
        child = Instantiate(meshes[Random.Range(0, meshes.Length - 1)], transform);
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = child.GetComponent<MeshFilter>().sharedMesh;
        hitCount_access = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default)
    {
        base.OnHit(_ball, _impactVector, _thrower, _damages, _source, _bumpModificators);
        if (hitCount_access >= maxHealth)
        {
            DestroyTheObject();
        }
    }

    public void DestroyTheObject()
    {
        Destroy(gameObject);
    }
}
