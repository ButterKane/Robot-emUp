using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class IndianaManager : MonoBehaviour
{

    [Header("General")]
    public float totalLenght;
    public int DamageToPawn;
    public Vector3 directionIndiania;
    public Vector3 startposition;
    [Header("State")]
    public bool activated;
    [ReadOnly] public float currentTimer;
    [ReadOnly] public float currentTimerExplosion;
    [ReadOnly] public float currentTimerExplosionBarrage;
    [ReadOnly] public float currentPositionMultiplier;
    [Header("Explosions")]
    public float minSizeExplosion;
    public float maxSizeExplosion;
    public float maxDistancePlayer;
    public float explosionWaitingTime;
    public float nbExplosionBySec;
    [Header("Barrage")]
    public float timeBarrage;
    public int nbExplosionByBarrage;
    public int nbBarrage;
    public Vector3 directionBarrage;
    [Header("References - DO NOT TOUCH!")]
    public IndianaCamera indianaCamera;
    public GameObject prefabIndianaExplosion;
    public GameObject InvisibleColliderForward;
    public GameObject InvisibleColliderBackward;
    private BoxCollider boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            currentTimer -= Time.deltaTime;
            currentTimerExplosion -= Time.deltaTime;
            currentTimerExplosionBarrage -= Time.deltaTime;

            if (currentTimer < 0)
            {
                activated = false;
                InvisibleColliderForward.SetActive(false);
                InvisibleColliderBackward.transform.position = transform.position + (currentPositionMultiplier - 3) * directionIndiania;
                indianaCamera.OnRail = false;
            }

            
           if (currentTimerExplosion < 0)
            {
                currentTimerExplosion = 1 / nbExplosionBySec;
                int temproll = Random.Range(0, 3);
                Vector3 wantedPosition;
                if (temproll == 0)
                {
                    wantedPosition = GameManager.i.playerOne.transform.position;
                }
                else if (temproll == 1)
                {
                    wantedPosition = GameManager.i.playerTwo.transform.position;
                }
                else
                {
                    wantedPosition = Vector3.Lerp(GameManager.i.playerTwo.transform.position, GameManager.i.playerOne.transform.position, 0.5f);
                }
                wantedPosition = new Vector3(wantedPosition.x + Random.Range(-maxDistancePlayer, maxDistancePlayer), wantedPosition.y, wantedPosition.z + Random.Range(-maxDistancePlayer, maxDistancePlayer));
                IndianaExplosion newExplosion = Instantiate(prefabIndianaExplosion, wantedPosition, Quaternion.Euler(-90,0,0)).GetComponent<IndianaExplosion>();
                newExplosion.myScale = Random.Range(minSizeExplosion, maxSizeExplosion);
                newExplosion.waitTimeForExplosion = explosionWaitingTime;
                newExplosion.indianaManager = this;
                newExplosion.isBarrage = false;
                newExplosion.Initiate();
            }


            if (currentTimerExplosionBarrage < 0)
            {
                currentTimerExplosionBarrage = timeBarrage;

                currentPositionMultiplier++;
                InvisibleColliderBackward.transform.position = transform.position + (currentPositionMultiplier - 3f) * directionIndiania;
                InvisibleColliderForward.transform.position = transform.position + (currentPositionMultiplier + 3.5f) * directionIndiania;
                indianaCamera.RailPositionWanted = transform.position + (currentPositionMultiplier + 1 ) * directionIndiania;
                for (int j = 0; j < nbBarrage; j++)
                {
                    for (int i = 0; i < nbExplosionByBarrage; i++)
                    {
                        Vector3 wantedPosition = startposition + ( currentPositionMultiplier - j ) * directionIndiania;
                        wantedPosition += directionBarrage * i;
                        IndianaExplosion newExplosion = Instantiate(prefabIndianaExplosion, wantedPosition, Quaternion.Euler(-90, 0, 0)).GetComponent<IndianaExplosion>();
                        newExplosion.myScale = maxSizeExplosion;
                        newExplosion.waitTimeForExplosion = explosionWaitingTime;
                        newExplosion.indianaManager = this;
                        newExplosion.isBarrage = true;
                        newExplosion.Initiate();
                    }
                }
                
            }

        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            StartingIndianaMoment();
        }

    }


    private void StartingIndianaMoment()
    {
        activated = true;
        currentTimer = totalLenght;
        currentPositionMultiplier = 0;
        indianaCamera.OnRail = true;
        InvisibleColliderForward.SetActive(true);
        indianaCamera.RailPositionWanted = transform.position + (currentPositionMultiplier) * directionIndiania; ;
    }


#if UNITY_EDITOR // conditional compilation is not mandatory
    [ButtonMethod]
    private void StartIndianaMomentEditor()
    {
        StartingIndianaMoment();
    }
#endif
}
