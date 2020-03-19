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
    private Vector3 ColliderForwardWantedPosition;
    private Vector3 ColliderBackwardWantedPosition;

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
            invisibleColliderForward.transform.position = Vector3.Lerp(invisibleColliderForward.transform.position, ColliderForwardWantedPosition, 0.1f);
            invisibleColliderBackward.transform.position = Vector3.Lerp(invisibleColliderBackward.transform.position, ColliderBackwardWantedPosition, 0.1f);



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
                int i_temproll = Random.Range(0, 3);
                Vector3 i_wantedPosition;
                if (i_temproll == 0)
                {
                    i_wantedPosition = GameManager.playerOne.transform.position;
                }
                else if (i_temproll == 1)
                {
                    i_wantedPosition = GameManager.playerTwo.transform.position;
                }
                else
                {
                    i_wantedPosition = Vector3.Lerp(GameManager.playerTwo.transform.position, GameManager.playerOne.transform.position, 0.5f);
                }
                i_wantedPosition = new Vector3(i_wantedPosition.x + Random.Range(-maxDistancePlayer, maxDistancePlayer), i_wantedPosition.y, i_wantedPosition.z + Random.Range(-maxDistancePlayer, maxDistancePlayer));
                IndianaExplosion i_newExplosion = Instantiate(prefabIndianaExplosion, i_wantedPosition, Quaternion.Euler(-90,0,0)).GetComponent<IndianaExplosion>();
                i_newExplosion.myScale = Random.Range(minSizeExplosion, maxSizeExplosion);
                i_newExplosion.waitTimeForExplosion = explosionWaitingTime;
                i_newExplosion.indianaManager = this;
                i_newExplosion.isBarrage = false;
                i_newExplosion.Initiate();
            }


            if (currentTimerExplosionBarrage < 0)
            {
                currentTimerExplosionBarrage = timeBarrage;

                currentPositionMultiplier++;
                ColliderBackwardWantedPosition = transform.position + (currentPositionMultiplier - 4f) * directionIndiania;
                ColliderForwardWantedPosition = transform.position + (currentPositionMultiplier + 3.5f) * directionIndiania;
                indianaCamera.railPositionWanted = transform.position + (currentPositionMultiplier + 1 ) * directionIndiania;
                for (int j = 0; j < nbBarrage; j++)
                {
                    for (int i = 0; i < nbExplosionByBarrage; i++)
                    {
                        Vector3 i_wantedPosition = startposition + ( currentPositionMultiplier - j ) * directionIndiania;
                        i_wantedPosition += directionBarrage * i;
                        IndianaExplosion i_newExplosion = Instantiate(prefabIndianaExplosion, i_wantedPosition, Quaternion.Euler(-90, 0, 0)).GetComponent<IndianaExplosion>();
                        i_newExplosion.myScale = maxSizeExplosion;
                        i_newExplosion.waitTimeForExplosion = explosionWaitingTime;
                        i_newExplosion.indianaManager = this;
                        i_newExplosion.isBarrage = true;
                        i_newExplosion.Initiate();
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


        ColliderBackwardWantedPosition = transform.position + (currentPositionMultiplier - 3f) * directionIndiania;
        ColliderForwardWantedPosition = transform.position + (currentPositionMultiplier + 3.5f) * directionIndiania;
        invisibleColliderForward.transform.position = ColliderForwardWantedPosition;
        invisibleColliderBackward.transform.position = ColliderBackwardWantedPosition;
    }


    public void StartIndianaColumn(Transform zPosition)
    {
        Debug.Log("STARTING COLUMN");
        for (int i = 0; i < 50; i++)
        {
                Vector3 i_wantedPosition = startposition;
                i_wantedPosition = new Vector3(zPosition.position.x, i_wantedPosition.y, i_wantedPosition.z);
                i_wantedPosition += directionIndiania * i;
                IndianaExplosion i_newExplosion = Instantiate(prefabIndianaExplosion, i_wantedPosition, Quaternion.Euler(-90, 0, 0)).GetComponent<IndianaExplosion>();
                i_newExplosion.myScale = maxSizeExplosion;
                i_newExplosion.waitTimeForExplosion = explosionWaitingTime;
                i_newExplosion.indianaManager = this;
                i_newExplosion.isBarrage = true;
                i_newExplosion.Initiate();
        }
    }


#if UNITY_EDITOR // conditional compilation is not mandatory
    [ButtonMethod]
    private void StartIndianaMomentEditor()
    {
        StartingIndianaMoment();
    }
#endif
}
