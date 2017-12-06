using UnityEngine;
using System.Collections;

public class AIZombiStateMachine : AIStateMachine
{

    #region//Inspector Assigned properties

    [SerializeField]
    [Range(10.0f, 360f)]
    float fov = 50.0f;

    [SerializeField]
    [Range(0.0f, 1f)]
    float sightDistance = 0.7f;

    [SerializeField]
    [Range(0.0f, 1f)]
    float hearingDistance = 1.0f;

    [SerializeField]
    [Range(0.0f, 1f)]
    float agression = 0.6f;

    [SerializeField]
    [Range(0, 100)]
    int health = 100;

    [SerializeField]
    [Range(0.0f, 1f)]
    float intelligence = 0.6f;

    [SerializeField]
    [Range(0.0f, 1f)]
    float satisfaction = 1f;

    [SerializeField]
    [Range(0.0f, 1f)]
    float replenishRate = 0.5f;

    [SerializeField]
    [Range(0.0f, 1f)]
    float depletionRate = 0.1f;

    #endregion

    #region Hashes

    private int speedHash = Animator.StringToHash("Speed");
    private int IsFeedingHash = Animator.StringToHash("IsFeeding");
    private int SeekingHash = Animator.StringToHash("Seeking");
    private int AttackTypeHash = Animator.StringToHash("AttackType");

    #endregion

    //Private
    //-1 - turns left, 1 - turns right
    private int _seeking = 0;
    private bool _isFeeding = false;
    private bool _isCrawling = false;
    private int _attackType = 0; 
    private float _speed = 0.0f;

    //Public
    public float FOV { get { return fov; } }
    public float SightDistance { get { return sightDistance; } }
    public float HearingDistance { get { return hearingDistance; } }
    public float Intelligence { get { return intelligence; } }
    public bool IsCrawling { get { return _isCrawling; } }

    public float Agression { get { return agression; } set { agression = value; } }
    public int Health { get { return health; } set { health = value; } }
    public float Satisfaction { get { return satisfaction; } set { satisfaction = value; } }
    public float ReplenishRate { get { return replenishRate; } }
    public int Seeking { get { return _seeking; } set { _seeking = value; } }
    public bool Feeding { get { return _isFeeding; } set { _isFeeding = value; } }
    public int AttackType { get { return _attackType; } set { _attackType = value; } }

    public float Speed
    {
        get
        {
            return _speed;
        }
        set
        {
            _speed = value;
        }
    }

    protected override void Update()
    {
        base.Update();

        this.BodyAnimator.SetFloat(speedHash, this.Speed);
        this.BodyAnimator.SetBool(IsFeedingHash, this.Feeding);
        this.BodyAnimator.SetInteger(SeekingHash, this.Seeking);
        this.BodyAnimator.SetInteger(_attackType, this.AttackType);
    }

}
