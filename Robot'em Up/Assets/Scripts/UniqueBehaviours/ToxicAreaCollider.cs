using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicAreaCollider : MonoBehaviour, IHitable
{
    public ToxicAreaManager manager;
    public float multiplicator = 1f;

	[SerializeField] protected bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] protected float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay (Collider _other)
    {

        if (_other.gameObject.GetComponent<PlayerController>())
        {
            if (_other.gameObject.GetComponent<PlayerController>().playerIndex == XInputDotNetPure.PlayerIndex.One)
            {
                manager.ToxicValue_P1 += Time.deltaTime * multiplicator;
            }
            if (_other.gameObject.GetComponent<PlayerController>().playerIndex == XInputDotNetPure.PlayerIndex.Two)
            {
                manager.ToxicValue_P2 += Time.deltaTime * multiplicator;
            }
        }
  }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default)
    {
        if (_source == DamageSource.Dunk)
        {
            Destroy(gameObject);
        }
      
    }
}
