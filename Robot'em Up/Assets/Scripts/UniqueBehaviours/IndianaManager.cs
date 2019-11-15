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
    public float minSizeExplosion;
    public float maxSizeExplosion;
    public float maxDistancePlayer;
    public float nbExplosionBySec;


    public GameObject prefabIndianaExplosion;

    // Start is called before the first frame update
    void Start()
    {
        currentTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            currentTimer -= Time.deltaTime;
            currentTimerExplosion -= Time.deltaTime;

            if (currentTimer < 0)
            {
                activated = false;
            }

                if (currentTimerExplosion < 0)
            {
                currentTimerExplosion = 1 / nbExplosionBySec;
                int temproll = Random.Range(0, 2);
                Vector3 wantedPosition;
                if (temproll == 1)
                {
                    wantedPosition = GameManager.i.playerOne.transform.position;
                }
                else
                {
                    wantedPosition = GameManager.i.playerTwo.transform.position;
                }
                wantedPosition = new Vector3(wantedPosition.x + Random.Range(-maxDistancePlayer, maxDistancePlayer), wantedPosition.y, wantedPosition.z + Random.Range(-maxDistancePlayer, maxDistancePlayer));
                IndianaExplosion newExplosion = Instantiate(prefabIndianaExplosion, wantedPosition, Quaternion.Euler(-90,0,0)).GetComponent<IndianaExplosion>();
                newExplosion.myScale = Random.Range(minSizeExplosion, maxSizeExplosion);
                newExplosion.Initiate();
            }
        }
    }


#if UNITY_EDITOR // conditional compilation is not mandatory
    [ButtonMethod]
    private void StartIndianaMoment()
    {
        activated = true;
        currentTimer = totalLenght;
    }
#endif
}
