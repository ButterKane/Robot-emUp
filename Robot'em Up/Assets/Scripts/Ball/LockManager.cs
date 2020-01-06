using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockManager : MonoBehaviour
{
	public static List<AimLock> lockedTargets = new List<AimLock>();
	private static LockDatas datas;

	public static void LockTarget(Transform _target, float _hitboxSize)
	{
		if (datas == null) { datas = Resources.Load<LockDatas>("LockData"); }
		if (!datas.enableLock) { return; }
		foreach (AimLock lockF in lockedTargets)
		{
			if (lockF.linkedTarget == _target)
			{
				return;
			}
		}
        Canvas internal_canvas = GameManager.mainCanvas;

        if (internal_canvas != null)
        {
            AimLock internal_newLock = Instantiate(Resources.Load<GameObject>("LockResource/Lock")).GetComponent<AimLock>();
            internal_newLock.transform.SetParent(internal_canvas.transform);
            internal_newLock.Init(_target, _hitboxSize);
            lockedTargets.Add(internal_newLock);
        }
    }

	public static void UnlockTarget(Transform _target)
	{
		foreach (AimLock lockF in lockedTargets)
		{
			if (lockF.linkedTarget == _target)
			{
				lockF.Unlock();
			}
		}
	}
	public static void UnlockTarget ( AimLock _enemy )
	{
		_enemy.Unlock();
	}

	public static void UnlockAll()
	{
		 if (lockedTargets.Count == 0) { return; }
		foreach (AimLock lockF in lockedTargets)
		{
			lockF.Unlock();
		}
	}
	public static void LockTargetsInPath ( List<Vector3> _pathCoordinates, float _startValue )
	{
		List<Transform> internal_foundTargets = new List<Transform>();
		int internal_startPoint = Mathf.RoundToInt((_startValue - 0.05f) * _pathCoordinates.Count);
		internal_startPoint = Mathf.Clamp(internal_startPoint, 0, _pathCoordinates.Count - 1);
		for (int i = internal_startPoint; i < _pathCoordinates.Count - 1; i++)
		{
			Vector3 internal_direction = _pathCoordinates[i + 1] - _pathCoordinates[i];
			RaycastHit[] internal_hitObjects = Physics.RaycastAll(_pathCoordinates[i], internal_direction, internal_direction.magnitude);
			List<RaycastHit> internal_hitObjectsList = new List<RaycastHit>(internal_hitObjects);
			if (i > 1)
			{
				RaycastHit[] internal_reverseHitObjects = Physics.RaycastAll(_pathCoordinates[i], -internal_direction, internal_direction.magnitude);
				foreach (RaycastHit hit in internal_reverseHitObjects)
				{
					internal_hitObjectsList.Add(hit);
				}
			}
			foreach (RaycastHit hit in internal_hitObjectsList)
			{
				IHitable potentialTarget = hit.transform.GetComponent<IHitable>();
				if (potentialTarget != null && potentialTarget.lockable)
				{
					internal_foundTargets.Add(hit.transform);
					LockManager.LockTarget(hit.transform, potentialTarget.lockHitboxSize);
				}
			}
		}
		foreach (AimLock lockedTarget in lockedTargets)
		{
			if (!internal_foundTargets.Contains(lockedTarget.linkedTarget))
			{
				LockManager.UnlockTarget(lockedTarget);
			}
		}
	}
}
