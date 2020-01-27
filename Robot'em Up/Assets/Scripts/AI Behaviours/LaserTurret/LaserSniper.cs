using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSniper : MonoBehaviour
{
    public float speed;
    [HideInInspector] public int damageDealt;
    Vector3 initialPosition;
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform spawnParent;
    [HideInInspector] public LaserSniperTurretBehaviour enemyScript;
    public GameObject impactFX;
    public Vector3 impactFXScale;
    public LayerMask layerToCheck;
    public float distanceToRaycast;
    private float? laserLength = null;

    [HideInInspector] public bool isAimingPlayer;
    [HideInInspector] public float distanceAoEDamage;

    private void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        //rb.velocity = (target.position - transform.position).normalized * speed;
        //maxLifeTime -= Time.deltaTime;
        //if (maxLifeTime <= 0 || !target.GetComponent<PawnController>().IsTargetable())
        //{
        //    Destroy(gameObject);
        //}

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanceToRaycast, layerToCheck))
        {
            GameObject i_impactFX = Instantiate(impactFX, hit.point, Quaternion.identity);
            i_impactFX.transform.localScale = impactFXScale;
            Destroy(i_impactFX, .25f);
            if (isAimingPlayer && Vector3.Distance(hit.point, target.position) < distanceAoEDamage)
            {
                target.GetComponent<PawnController>().Damage(damageDealt);
            }

        }

        RaycastToHitWithLaser();
        UpdateLaserLength(laserLength);

        //UpdateLaserPosition();
    }

    public void UpdateLaserLength(float? givenLength)
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

    public void RaycastToHitWithLaser()
    {
        bool i_touchedSomething = false;
        RaycastHit[] i_hitObjects = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), enemyScript.laserMaxLength);
        if (i_hitObjects.Length > 0)
        {
            foreach (var touched in i_hitObjects)
            {
                Debug.DrawRay(touched.transform.position, Vector3.up * 4, Color.green);
                IHitable i_potentialHitableObject = touched.transform.GetComponent<IHitable>();
                if (i_potentialHitableObject != null)
                {
                    i_potentialHitableObject.OnHit(null, (touched.transform.position - transform.position).normalized, null, enemyScript.damagePerSecond / 60, DamageSource.Laser, Vector3.zero);
                }

                if ((touched.collider.tag == "Player" || touched.collider.tag == "Environment") && i_touchedSomething == false)
                {
                    laserLength = touched.distance;
                    i_touchedSomething = true;
                }
            }
        }
        else // if nothing has been hit (no wall, no player, nada)
        {
            laserLength = enemyScript.laserMaxLength;
        }

    }

    //public void UpdateLaserPosition()
    //{
    //    transform.position = enemyScript.bulletSpawn.position;
    //    transform.LookAt(transform.position + spawnParent.transform.forward);
    //}

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.tag == "Enemy")
        {
            _other.GetComponent<PawnController>().Damage(damageDealt);
            GameObject i_impactFX = Instantiate(impactFX, transform.position, Quaternion.identity);
            i_impactFX.transform.localScale = impactFXScale;
            Destroy(i_impactFX, 1);
        }
        else if (_other.tag == "Player")
        {
            _other.GetComponent<PawnController>().Damage(damageDealt);
            GameObject i_impactFX = Instantiate(impactFX, transform.position, Quaternion.identity);
            i_impactFX.transform.localScale = impactFXScale;
            Destroy(i_impactFX, 1);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // TODO: check the first touched object. 
        //If this object is hitable, stops the laser at that length
        //Apply force to push back hit object
        //Apply damages (must be written as damages per second, and so divided by 60 when used to match the frame update)
    }
}