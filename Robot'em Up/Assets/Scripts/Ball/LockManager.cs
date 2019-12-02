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
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas != null)
        {
            AimLock newLock = Instantiate(Resources.Load<GameObject>("LockResource/Lock")).GetComponent<AimLock>();
            newLock.transform.SetParent(canvas.transform);
            newLock.Init(_target, _hitboxSize);
            lockedTargets.Add(newLock);
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
		List<Transform> foundTargets = new List<Transform>();
		int startPoint = Mathf.RoundToInt((_startValue - 0.05f) * _pathCoordinates.Count);
		startPoint = Mathf.Clamp(startPoint, 0, _pathCoordinates.Count - 1);
		for (int i = startPoint; i < _pathCoordinates.Count - 1; i++)
		{
			Vector3 direction = _pathCoordinates[i + 1] - _pathCoordinates[i];
			RaycastHit[] hitObjects = Physics.RaycastAll(_pathCoordinates[i], direction, direction.magnitude);
			List<RaycastHit> hitObjectsList = new List<RaycastHit>(hitObjects);
			if (i > 1)
			{
				RaycastHit[] reverseHitObjects = Physics.RaycastAll(_pathCoordinates[i], -direction, direction.magnitude);
				foreach (RaycastHit hit in reverseHitObjects)
				{
					hitObjectsList.Add(hit);
				}
			}
			foreach (RaycastHit hit in hitObjectsList)
			{
				IHitable potentialTarget = hit.transform.GetComponent<IHitable>();
				if (potentialTarget != null && potentialTarget.lockable)
				{
					foundTargets.Add(hit.transform);
					LockManager.LockTarget(hit.transform, potentialTarget.lockHitboxSize);
				}
			}
		}
		foreach (AimLock lockedTarget in lockedTargets)
		{
			if (!foundTargets.Contains(lockedTarget.linkedTarget))
			{
				LockManager.UnlockTarget(lockedTarget);
			}
		}
	}
}
