using UnityEngine;

public class LaserSniper : MonoBehaviour
{
    public float speed;
    [HideInInspector] public int damageDealt;
    Vector3 initialPosition;
    [HideInInspector] public Transform spawnParent;
    [HideInInspector] public LaserSniperTurretBehaviour enemyScript;
    public GameObject impactFX;
    public Vector3 impactFXScale;
    public LayerMask layerToCheck;
    public float distanceToRaycast;
    private float? laserLength = null;
    public float laserWidth = 2;
    public float laserRepulsionRadius = 2;
    public float laserRepulsionForce = 2;
    public bool isLaserActive;
    [HideInInspector] public MeshRenderer laserRenderer;
    [HideInInspector] public bool isAimingPlayer;
    [HideInInspector] public float distanceAoEDamage;
    private float accumulatedDamage;

    private void Awake()
    {
        isLaserActive = true;
        initialPosition = transform.position;
        laserRenderer = GetComponentInChildren<MeshRenderer>();
        accumulatedDamage = 0;
    }

    void Update()
    {
        transform.localScale = new Vector3(laserWidth, laserWidth, transform.localScale.z);
        RaycastToHitWithLaser();
        UpdateLaserLength(laserLength);
    }

    private void UpdateLaserLength(float? givenLength)
    {
        float i_laserLength = 0;
        if (givenLength == null)
        {
            i_laserLength = enemyScript.laserMaxLength;
        }
        else
        {
            i_laserLength = (float)givenLength;
        }
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, i_laserLength);

    }

    private void RaycastToHitWithLaser()
    {
        // First raycast pass to determine the laser length
        float i_closestDistance = enemyScript.laserMaxLength;
        RaycastHit[] i_hitObjects = Physics.SphereCastAll(transform.position, laserWidth, transform.TransformDirection(Vector3.forward), enemyScript.laserMaxLength);
        if (i_hitObjects.Length > 0)
        {
            foreach (var touched in i_hitObjects)
            {
                if ((touched.collider.tag == "Player" || touched.collider.tag == "Environment") && touched.distance < i_closestDistance)
                {
                    i_closestDistance = touched.distance;
                }
            }
        }

        laserLength = i_closestDistance + laserWidth * 1.1f; // This is to compensate the sphere cast radius

        if (isLaserActive)
        {
            // Second raycast pass to determine what the laser hit and damage it
            RaycastHit[] i_hitObjectsWithRightLength = Physics.SphereCastAll(transform.position, laserWidth, transform.TransformDirection(Vector3.forward), i_closestDistance);
            {
                if (i_hitObjectsWithRightLength.Length > 0)
                {
                    foreach (var touched in i_hitObjectsWithRightLength)
                    {
                        Debug.DrawRay(touched.transform.position, Vector3.up * 4, Color.green);
                        IHitable i_potentialHitableObject = touched.collider.GetComponent<IHitable>();

                        if (i_potentialHitableObject != null && touched.transform != enemyScript.transform) 
                        {
                            i_potentialHitableObject.OnHit(null, (touched.transform.position - touched.point).normalized, null, enemyScript.damagePerSecond * Time.deltaTime, DamageSource.Laser, Vector3.zero);

                            LaserRepulsion(touched.point);

                            GameObject i_impactFX = Instantiate(impactFX, touched.point, Quaternion.identity);
                            i_impactFX.transform.localScale = impactFXScale;
                        }
                    }
                }
            }
        }
    }

    private void LaserRepulsion(Vector3 _centerRepulsionPoint)
    {
        RaycastHit[] i_hitObjects = Physics.SphereCastAll(_centerRepulsionPoint, enemyScript.repulseCircleRadius, Vector3.forward);
        if (i_hitObjects.Length > 0)
        {
            foreach (var touched in i_hitObjects)
            {
                if (touched.collider.tag == "Player" || touched.collider.tag == "Enemy")
                {
                    Vector3 i_repulsionDirection = (touched.transform.position - touched.point).normalized;

                    Debug.DrawLine(touched.point, touched.transform.position, Color.magenta);

                    Rigidbody touchedRb = touched.transform.GetComponent<Rigidbody>();
                    if (touchedRb!= null)
                    {
                        touchedRb.AddForce(i_repulsionDirection * enemyScript.repulseCircleStrength, ForceMode.Impulse);

                        if (touched.collider.tag == "Player")
                        {
                            touched.collider.gameObject.GetComponent<PlayerController>().AddSpeedModifier(new SpeedCoef(enemyScript.playerSpeedReductionCoef, Time.deltaTime, SpeedMultiplierReason.Environment, true));
                        }
                    }
                }
            }
        }
    }


}