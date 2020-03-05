using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossDatas", menuName = "GameDatas/BossDatas")]
public class BossSettings : ScriptableObject
{
	[System.Serializable]
	public class GlobalSettings
	{
		[Tooltip("The default mode of the boss")] public BossMode startingMode;
		[Tooltip("")] public float moveSpeed;
		[Tooltip("Rotation speed of the bottom part of the boss")] public float angularSpeed;
		[Tooltip("Max distance between boss and player before the boss stops")] public float stopDistance;
		[Tooltip("")] public float obstacleAvoidanceRadius;
		[Tooltip("")] public float obstacleAvoidanceHeight;
		[Tooltip("")] public float topPartRotationSpeed;
		[Tooltip("")] public float firstPhaseHP;
		[Tooltip("")] public float secondPhaseHP;
		[Tooltip("Health bar offset height")] public float healthBarHeight;
		[Tooltip("Minimum distance between boss and players to attack them")] public float minAttackRange = 15f;
		[Tooltip("Minimum angle between boss and players to attack them")] public float maxAttackAngle = 5f;
	}

	[System.Serializable]
	public class StaggerSettings
	{
		[Tooltip("Energy gained by hitting boss when he's staggered (between 0 and 1)")] public float energyGainedOnHit;
	}

	[System.Serializable]
	public class LaserSettings
	{
		[Tooltip("Laser total duration")] public float duration;
		[Tooltip("")] public float rotationSpeed;
		[Tooltip("Damages when player collide with laser")] public float damagesOnEnter;
		[Tooltip("Damages per second when player stay in the laser")] public float damagesPerSecond;
		[Tooltip("Normal laser width")] public float defaultWidth;
		[Tooltip("Laser width when it is not charged")] public float chargingWidth;
		[Tooltip("")] public float chargeDuration;
		[Tooltip("Delay before laser charges (how much time the laser stay small)")] public float chargeDelay;
		[Tooltip("")] public AnimationCurve chargeSpeedCurve;
		[Tooltip("")] public float maxLength;
		[Tooltip("laser goes from a length of 0 to a length of 'maxLength' with this speed when it spawns")] public float maxLengthIncrementationSpeed;
	}

	[System.Serializable]
	public class BulletStormSettings
	{
		[Tooltip("Rotation speed of the bottom part of the boss when it is bullet storm phase")] public float bodyRotationSpeed;
		[Tooltip("")] public float minDelayBetweenBullets;
		[Tooltip("")] public float maxDelayBetweenBullets;
		[Tooltip("")] public bool lockableBullet;
		[Tooltip("")] public bool hitableBullet;
		[Tooltip("")] public float bulletMoveSpeed;
		[Tooltip("")] public float bulletDamages;
		[Tooltip("")] public float bulletRotationSpeed;
		[Tooltip("")] public float bulletSize;
		[Tooltip("Bullet starts with a scale of 0 and goes to his normal scale in X seconds")] public float bulletScaleDuration;
		[Tooltip("")] public float bulletLifetime;
		[Tooltip("Random angle determined for the bullet direction (Between 0 and 90)")] public float bulletRandomness;
		[Tooltip("Duration for the canon to go out and in the legs")] public float canonRetractionDuration;
		[Tooltip("Canon is scaled when shooting")] public float canonScaleMultiplierOnShoot;
		[Tooltip("Duration to get back to normal scale after shooting")] public float canonScaleRecoverSpeed;
		[Tooltip("")] public float canonMaxAngle;
		[Tooltip("")] public float canonRotationSpeed;
	}

	[System.Serializable]
	public class TilesSettings
	{
		[Tooltip("Delay before the tile respawns after being destroyed")] public float reapparitionDelay;
		[Tooltip("Fall animation duration (also used for recover animation)")] public float fallDuration;
		[Tooltip("Used for fall and recover animation")] public AnimationCurve fallSpeedCurve;
	}

	[System.Serializable]
	public class ElectricalPlateSettings
	{
		[Tooltip("")] public GameObject platePrefab;
		[Tooltip("The boss is blocked X time before spawning the electric plates")] public float groundAttackPreparationDuration;
		[Tooltip("Duration before destroying electric plates")] public float duration;
		[Tooltip("")] public float activationDuration;
		[Tooltip("")] public float desactivationDuration;
	}

	[System.Serializable]
	public class WeakPointSettings
	{
		[Tooltip("")] public float legMaxHP = 50;
	}
	[System.Serializable]
	public class PunchSettings
	{
		[Tooltip("Distance between boss and punch hitbox")] public float distance = 2f;
		[Tooltip("")] public float cooldown;
		[Tooltip("")] public float chargeDuration = 2f;
		[Tooltip("")] public AnimationCurve chargeSpeedCurve;
		[Tooltip("")] public float damages = 5f;
		[Tooltip("")] public float pushForce = 10f;
		[Tooltip("")] public float pushHeight = 3f;
		[Tooltip("")] public float hitboxSize = 2f;
		[Tooltip("")] public Color chargingColor;
		[Tooltip("")] public Color hitColor;
	}

	[System.Serializable]
	public class HammerSettings
	{
		[Tooltip("")] public float cooldown = 5f;
		[Tooltip("")] public float chargeDuration = 2f;
		[Tooltip("")] public AnimationCurve chargeSpeedCurve;
		[Tooltip("")] public Color chargingColor;
		[Tooltip("")] public Color hitColor;
	}

	[System.Serializable]
	public class TeaBagSettings
	{
		[Tooltip("Players in this radius are damaged by the teabag attack")] public float damageRadius;
		[Tooltip("Players in this radius can trigger the teabag attack")] public float triggerRadius;
		[Tooltip("Players in this radius are bumped by the teabag")] public float bumpRadius;
		[Tooltip("Only visual hitbox size")] public float hitboxSize;
		[Tooltip("Players have to stay this time before being teabagged")] public float maxTimeBeforeTeaBag = 5f;
		[Tooltip("")] public float previewDuration = 2f;
		[Tooltip("")] public float damages;
		[Tooltip("")] public float pushForce;
	}

	[System.Serializable]
	public class RangeModeSettings
	{
		[Tooltip("Shoulders rotates toward players at this speed")] public float shoulderRotationSpeed;
		[Tooltip("Controls the y position of the turret when they're detached, must be inverted")] public AnimationCurve turretDetachSpeedCurve;
		[Tooltip("Only the prefab matter, the name and color don't do anything")] public EnemyData enemyToSpawn;
	}

	public GlobalSettings globalSettings;
	public StaggerSettings staggerSettings;
	public LaserSettings laserSettings;
	public BulletStormSettings bulletStormSettings;
	public TilesSettings tilesSettings;
	public ElectricalPlateSettings electricalPlateSettings;
	public WeakPointSettings weakPointSettings;
	public PunchSettings punchSettings;
	public HammerSettings hammerSettings;
	public TeaBagSettings teaBagSettings;
	public RangeModeSettings rangeModeSettings;


	public static BossSettings GetDatas()
	{
		return Resources.Load<BossSettings>("EnemyResource/BossResource/BossDatas");
	}
}
