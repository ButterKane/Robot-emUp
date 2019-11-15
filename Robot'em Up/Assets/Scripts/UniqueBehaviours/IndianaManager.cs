using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class IndianaManager : MonoBehaviour
{

    public bool activated;
    public float totalLenght;
    [ReadOnly] public float currentTimer;
    [ReadOnly] public float currentTimerExplosion;
    [ReadOnly] public float currentTimerExplosionBarrage;
    public Vector3 directionIndiania;
    public Vector3 startposition;
    public float currentPositionMultiplier;
    public float minSizeExplosion;
    public float maxSizeExplosion;
    public float maxDistancePlayer;
    public float nbExplosionBySec;
    public float timeBarrage;
    public int nbBarrage;
    public Vector3 directionBarrage;
    public float explosionWaitingTime;


    public GameObject prefabIndianaExplosion;

    // Start is called before the first frame update
    void Start()
    {
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
            }

           if (currentTimerExplosion < 0)
            {
                currentTimerExplosion = 1 / nbExplosionBySec;
                int temproll = Random.Range(0, 3);
                Vector3 wantedPosition;
                if (temproll == 1)
                {
                    wantedPosition = GameManager.i.playerOne.transform.position;
                }
                else if (temproll == 2)
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
                newExplosion.Initiate();
            }


            if (currentTimerExplosionBarrage < 0)
            {
                currentTimerExplosionBarrage = timeBarrage;
                currentPositionMultiplier++;
                for (int i = 0; i < nbBarrage; i++)
                {
                    Vector3 wantedPosition = startposition + currentPositionMultiplier * directionIndiania;
                    wantedPosition += directionBarrage * i;
                    IndianaExplosion newExplosion = Instantiate(prefabIndianaExplosion, wantedPosition, Quaternion.Euler(-90, 0, 0)).GetComponent<IndianaExplosion>();
                    newExplosion.myScale = maxSizeExplosion;
                    newExplosion.waitTimeForExplosion = explosionWaitingTime;
                    newExplosion.Initiate();
                }
                
            }

        }
    }


#if UNITY_EDITOR // conditional compilation is not mandatory
    [ButtonMethod]
    private void StartIndianaMoment()
    {
        activated = true;
        currentTimer = totalLenght;
        currentPositionMultiplier = 0;
    }
#endif
}
