using MyBox;
using UnityEngine;

public class EnemyBoss : PawnController, IHitable
{
    [Space(2)]
    [Separator("References")]
    public HealthBar healthBar1;
    public HealthBar healthBar2;
    public GameObject healthBarPrefab;

    [Separator("Boss Variables")]
    public Renderer[] renderers;
    public Color normalColor = Color.blue;
    public Color attackingColor = Color.red;
    [SerializeField] protected bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
    [SerializeField] protected float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }


    public enum BossState
    {
        Idle,
        Stagger,
        PunchAttack,
        RangeAttack,
        Shield,
        HammerPunchAttack,
        GroundAttack,
        Laser,
        ChangingPhase
    }

    public BossState bossState = BossState.Idle;
    public int Health1Bar_Value_Max = 100;
    public int Health2Bar_Value_Max = 50;
    [ReadOnly] public int Health1Bar_CurrentValue = 0;
    [ReadOnly] public int Health2Bar_CurrentValue = 0;
    

    [Space(2)]
    [Header("Punch Attack")]
    public int PunchAttack_DamageInflicted = 20; 
    public float RecoverTime = 1.5f;

    public void Start()
    {
        Health1Bar_CurrentValue = Health1Bar_Value_Max;
        Health2Bar_CurrentValue = Health2Bar_Value_Max;
        healthBar1 = Instantiate(healthBarPrefab, CanvasManager.i.mainCanvas.transform).GetComponent<HealthBar>();
        healthBar1.customHealthBar = true;
        healthBar1.target = this;

        healthBar2 = Instantiate(healthBarPrefab, CanvasManager.i.mainCanvas.transform).GetComponent<HealthBar>();
        healthBar2.customHealthBar = true;
        healthBar2.customDeltaPosition = -1;
        healthBar2.target = this;
        
        healthBar1.ToggleHealthBar(true);
        healthBar2.ToggleHealthBar(true);
    }


    public void Update()
    {
        healthBar1.customValueToCheck = (float)Health1Bar_CurrentValue / Health1Bar_Value_Max;
        healthBar2.customValueToCheck = (float)Health2Bar_CurrentValue / Health2Bar_Value_Max;
    }


    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default)
    {
        InflictDamage(_damages, 1f);
        FeedbackManager.SendFeedback("event.BallHittingEnemy", this, _ball.transform.position, _impactVector, _impactVector);
        EnergyManager.IncreaseEnergy(0.2f);
        if (_source == DamageSource.Dunk)
        {
            InflictDamage(_damages, 1.5f);
        }
        
    }

    public void InflictDamage(float _damages, float modifier=1)
    {
        if (Health1Bar_CurrentValue > 0)
        {
            Health1Bar_CurrentValue -= (int)(_damages * modifier);

            if (Health1Bar_CurrentValue <= 0)
            {
                Health1Bar_CurrentValue = 0;
            }
        }
        else if (Health2Bar_CurrentValue > 0)
        {

            Health2Bar_CurrentValue -= (int)(_damages * modifier);

            if (Health2Bar_CurrentValue <= 0)
            {
                Health2Bar_CurrentValue = 0;
            }
        }
        else
        {
            // shouldnotHappen
        }
        
    }



}
    

