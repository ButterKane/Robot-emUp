using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzleElevator : PuzzleActivable
{

    public Vector3 downPosition;
    public Vector3 upPosition;
    [Range(0, 10)]
    public float speed;
    public float delayBeforeMoving = 1f;
    public enum ElevatorState { MovingDown, MovingUp }
    public ElevatorState state;
    private float journeyLength;
    private float currentDelayBeforeMoving;
    float progression = 0;
    private float delayBlocked;

    public List<PawnController> blockingObjects = new List<PawnController>();


    void Awake()
    {
        currentDelayBeforeMoving = delayBeforeMoving;
        journeyLength = Vector3.Distance(downPosition, upPosition);
    }

    private void OnCollisionEnter ( Collision collision )
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerController foundPlayer = collision.gameObject.GetComponent<PlayerController>();
            if (foundPlayer != null && foundPlayer.transform.position.y <= transform.position.y && !blockingObjects.Contains(foundPlayer))
            {

                blockingObjects.Add(foundPlayer);
            }
        }
        if (collision.gameObject.tag == "Enemy")
        {
            EnemyBehaviour foundEnemy = collision.gameObject.GetComponent<EnemyBehaviour>();
            if (foundEnemy != null && foundEnemy.transform.position.y <= transform.position.y && !blockingObjects.Contains(foundEnemy))
            {
                blockingObjects.Add(foundEnemy);
            }
        }
    }

    private void OnCollisionExit ( Collision collision )
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerController foundPlayer = collision.gameObject.GetComponent<PlayerController>();
            if (foundPlayer != null && foundPlayer.transform.position.y <= transform.position.y && blockingObjects.Contains(foundPlayer))
            {
                blockingObjects.Remove(foundPlayer);
            }
        }
        if (collision.gameObject.tag == "Enemy")
        {
            EnemyBehaviour foundEnemy = collision.gameObject.GetComponent<EnemyBehaviour>();
            if (foundEnemy != null && foundEnemy.transform.position.y <= transform.position.y && blockingObjects.Contains(foundEnemy))
            {
                blockingObjects.Remove(foundEnemy);
            }
        }
    }

    private bool IsBlocked()
    {
        if (blockingObjects.Count > 0)
        {
            return true;
        }
        return false;
    }

    void Update()
    {
        if (currentDelayBeforeMoving > 0)
        {
            currentDelayBeforeMoving -= Time.deltaTime;
            return;
        } 

        if (state == ElevatorState.MovingUp && isActivated)
        {
            progression += Time.deltaTime * speed;
            if (progression < journeyLength)
            {
                transform.position = Vector3.Lerp(downPosition, upPosition, progression / journeyLength);
            }
            else
            {
                currentDelayBeforeMoving = delayBeforeMoving;
                state = ElevatorState.MovingDown;
            }
        }

        if ((IsBlocked() || delayBlocked > 0))
        {
            if (IsBlocked())
            {
                delayBlocked = 0.5f;
            }
            delayBlocked -= Time.deltaTime;
            if (state == ElevatorState.MovingDown)
            {
                progression += Time.deltaTime * speed;
                progression = Mathf.Clamp(progression, 0, journeyLength);
                if (progression > 0 && progression < journeyLength)
                {
                    transform.position = Vector3.Lerp(downPosition, upPosition, progression / journeyLength);
                }
            }
        }

        if (state == ElevatorState.MovingDown)
        {
            Debug.Log(transform.name + " " + !isActivated + " " + !IsBlocked() + " " + delayBlocked);
        }
        if (state == ElevatorState.MovingDown && !isActivated && !IsBlocked() && delayBlocked <= 0)
        {
            Debug.Log(transform.name + " Moving down " + progression);
            progression -= Time.deltaTime * speed;
            progression = Mathf.Clamp(progression, 0, journeyLength);
            if (progression > 0)
            {
                transform.position = Vector3.Lerp(downPosition, upPosition, progression / journeyLength);
            }
            else
            {
                currentDelayBeforeMoving = delayBeforeMoving;
                state = ElevatorState.MovingUp;
            }
        }

    }

    override public void Activate()
    {
        if (!isActivated)
        {
            isActivated = true;
            state = ElevatorState.MovingUp;
        }
    }

    override public void Desactivate()
    {
        Debug.Log("Desactivating");
        if (isActivated)
        {
            isActivated = false;
            state = ElevatorState.MovingDown;
        }
    }

#if UNITY_EDITOR // conditional compilation is not mandatory
    [ButtonMethod]
    private void SetPositionsToCurrent()
    {
        downPosition = transform.position;
        upPosition = transform.position;
        upPosition = new Vector3(upPosition.x, upPosition.y + 2, upPosition.z);
    }
#endif
}
