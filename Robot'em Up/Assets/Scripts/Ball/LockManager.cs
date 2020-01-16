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
        Canvas i_canvas = GameManager.mainCanvas;

        if (i_canvas != null)
        {
            AimLock i_newLock = Instantiate(Resources.Load<GameObject>("LockResource/Lock")).GetComponent<AimLock>();
            i_newLock.transform.SetParent(i_canvas.transform);
            i_newLock.Init(_target, _hitboxSize);
            lockedTargets.Add(i_newLock);
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
		List<Transform> i_foundTargets = new List<Transform>();
		int i_startPoint = Mathf.RoundToInt((_startValue - 0.05f) * _pathCoordinates.Count);
		i_startPoint = Mathf.Clamp(i_startPoint, 0, _pathCoordinates.Count - 1);
		for (int i = i_startPoint; i < _pathCoordinates.Count - 1; i++)
		{
			Vector3 i_direction = _pathCoordinates[i + 1] - _pathCoordinates[i];
			RaycastHit[] i_hitObjects = Physics.RaycastAll(_pathCoordinates[i], i_direction, i_direction.magnitude);
			List<RaycastHit> i_hitObjectsList = new List<RaycastHit>(i_hitObjects);
			if (i > 1)
			{
				RaycastHit[] i_reverseHitObjects = Physics.RaycastAll(_pathCoordinates[i], -i_direction, i_direction.magnitude);
				foreach (RaycastHit hit in i_reverseHitObjects)
				{
					i_hitObjectsList.Add(hit);
				}
			}
			foreach (RaycastHit hit in i_hitObjectsList)
			{
				IHitable potentialTarget = hit.transform.GetComponent<IHitable>();
				if (potentialTarget != null && potentialTarget.lockable_access)
				{
					i_foundTargets.Add(hit.transform);
					LockManager.LockTarget(hit.transform, potentialTarget.lockHitboxSize_access);
				}
			}
		}
		foreach (AimLock lockedTarget in lockedTargets)
		{
			if (!i_foundTargets.Contains(lockedTarget.linkedTarget))
			{
				LockManager.UnlockTarget(lockedTarget);
			}
		}
	}
}
