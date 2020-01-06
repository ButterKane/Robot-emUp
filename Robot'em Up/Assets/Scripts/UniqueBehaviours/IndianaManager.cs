using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class IndianaManager : MonoBehaviour
{

    [Header("General")]
    public float totalLenght;
    public int damageToPawn;
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
    public GameObject invisibleColliderForward;
    public GameObject invisibleColliderBackward;
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
                invisibleColliderForward.SetActive(false);
                invisibleColliderBackward.transform.position = transform.position + (currentPositionMultiplier - 3) * directionIndiania;
                indianaCamera.onRail = false;
            }

            
           if (currentTimerExplosion < 0)
            {
                currentTimerExplosion = 1 / nbExplosionBySec;
                int internal_temproll = Random.Range(0, 3);
                Vector3 internal_wantedPosition;
                if (internal_temproll == 0)
                {
                    internal_wantedPosition = GameManager.playerOne.transform.position;
                }
                else if (internal_temproll == 1)
                {
                    internal_wantedPosition = GameManager.playerTwo.transform.position;
                }
                else
                {
                    internal_wantedPosition = Vector3.Lerp(GameManager.playerTwo.transform.position, GameManager.playerOne.transform.position, 0.5f);
                }
                internal_wantedPosition = new Vector3(internal_wantedPosition.x + Random.Range(-maxDistancePlayer, maxDistancePlayer), internal_wantedPosition.y, internal_wantedPosition.z + Random.Range(-maxDistancePlayer, maxDistancePlayer));
                IndianaExplosion internal_newExplosion = Instantiate(prefabIndianaExplosion, internal_wantedPosition, Quaternion.Euler(-90,0,0)).GetComponent<IndianaExplosion>();
                internal_newExplosion.myScale = Random.Range(minSizeExplosion, maxSizeExplosion);
                internal_newExplosion.waitTimeForExplosion = explosionWaitingTime;
                internal_newExplosion.indianaManager = this;
                internal_newExplosion.isBarrage = false;
                internal_newExplosion.Initiate();
            }


            if (currentTimerExplosionBarrage < 0)
            {
                currentTimerExplosionBarrage = timeBarrage;

                currentPositionMultiplier++;
                invisibleColliderBackward.transform.position = transform.position + (currentPositionMultiplier - 3f) * directionIndiania;
                invisibleColliderForward.transform.position = transform.position + (currentPositionMultiplier + 3.5f) * directionIndiania;
                indianaCamera.railPositionWanted = transform.position + (currentPositionMultiplier + 1 ) * directionIndiania;
                for (int j = 0; j < nbBarrage; j++)
                {
                    for (int i = 0; i < nbExplosionByBarrage; i++)
                    {
                        Vector3 internal_wantedPosition = startposition + ( currentPositionMultiplier - j ) * directionIndiania;
                        internal_wantedPosition += directionBarrage * i;
                        IndianaExplosion internal_newExplosion = Instantiate(prefabIndianaExplosion, internal_wantedPosition, Quaternion.Euler(-90, 0, 0)).GetComponent<IndianaExplosion>();
                        internal_newExplosion.myScale = maxSizeExplosion;
                        internal_newExplosion.waitTimeForExplosion = explosionWaitingTime;
                        internal_newExplosion.indianaManager = this;
                        internal_newExplosion.isBarrage = true;
                        internal_newExplosion.Initiate();
                    }
                }
                
            }

        }
    }


    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.GetComponent<PlayerController>())
        {
            StartingIndianaMoment();
        }

    }


    private void StartingIndianaMoment()
    {
        activated = true;
        currentTimer = totalLenght;
        currentPositionMultiplier = 0;
        indianaCamera.onRail = true;
        invisibleColliderForward.SetActive(true);
        indianaCamera.railPositionWanted = transform.position + (currentPositionMultiplier) * directionIndiania; ;
    }


#if UNITY_EDITOR // conditional compilation is not mandatory
    [ButtonMethod]
    private void StartIndianaMomentEditor()
    {
        StartingIndianaMoment();
    }
#endif
}
