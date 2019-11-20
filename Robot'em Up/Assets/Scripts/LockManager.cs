using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockManager : MonoBehaviour
{
	public static List<AimLock> lockedTargets = new List<AimLock>();
	private static LockDatas datas;

	public static void LockEnemy(EnemyBehaviour _enemy)
	{
		if (datas == null) { datas = Resources.Load<LockDatas>("LockData"); }
		if (!datas.enableLock) { return; }
		foreach (AimLock lockF in lockedTargets)
		{
			if (lockF.linkedEnemy == _enemy)
			{
				return;
			}
		}
		AimLock newLock = Instantiate(Resources.Load<GameObject>("LockResource/Lock")).GetComponent<AimLock>();
		newLock.transform.SetParent(FindObjectOfType<Canvas>().transform);
		newLock.Init(_enemy, datas.lockHitboxRadius);
		lockedTargets.Add(newLock);
	}

	public static void UnlockEnemy(EnemyBehaviour _enemy)
	{
		foreach (AimLock lockF in lockedTargets)
		{
			if (lockF.linkedEnemy == _enemy)
			{
				lockF.Unlock();
			}
		}
	}
	public static void UnlockEnemy ( AimLock _enemy )
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
}
