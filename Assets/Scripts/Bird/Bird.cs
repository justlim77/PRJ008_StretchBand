using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;

public enum AnimationState
{
    Idle,
    Landing,
    Takeoff,
    Fly,
    Glide,
    Walking,
    Eat,
    Jump,
    Hunting,
    Die
}

public enum BoostState
{
   Ignition,
   Boosting,
   Cancelled
}

public class BoostStateChangedEventArgs : EventArgs
{
    public BoostState BoostState;
}

public class AnimationStateChangedEventArgs : EventArgs
{
    public AnimationState AnimationState;
}

public class FruitAmountChangedEventArgs : EventArgs
{
    public int BoostBerries;
    public int TotalBerries;
}

public class Bird : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 Center;
    public Vector3 MovementForce;
    public Vector3 Force;
    public Vector3 GuidedForce;

    public float TakeoffSpeed = 3.0f;
    public Ease takeOffEaseType = Ease.InSine;
    public float LandingSpeed = 3.0f;
    public Ease landingEaseType = Ease.OutBounce;
    public float LandingForwardBuffer = 5.0f;
    [SerializeField] float _GroundHeight = 0.5f;

    [Header("Flight Bounds")]
    public float HorizontalMin;
    public float HorizontalMax;
    public float VerticalMin;
    public float VerticalMax;

    [Header("Boost")]
    public int ForceThreshold = 10;
    public float BoostTime = 5.0f;
    public float BoostWindow = 10.0f;
    public float BoostMultiplier = 2.0f;
    public KeyCode BoostKey = KeyCode.Space;
    public ParticleSystem boostParticles;
    public float BoostRadius = 5.0f;
    public bool CanBoost { get; set; }

    #region Events and Delegates
    public delegate void FruitAmountChangedEventHandler(object sender, FruitAmountChangedEventArgs e);
    public static event FruitAmountChangedEventHandler FruitAmountChanged;

    public delegate void DistanceChangedEventHandler(object sender, float distance);
    public static event DistanceChangedEventHandler DistanceChanged;

    public delegate void BoostStateChangedEventHandler(object sender, BoostStateChangedEventArgs e);
    public static event BoostStateChangedEventHandler BoostStateChanged;

    public delegate void AnimationStateChangedEventHandler(object sender, AnimationStateChangedEventArgs e);
    public static event AnimationStateChangedEventHandler AnimationStateChanged;
    #endregion

    [Header("Magnet")]
    [SerializeField] Transform _MagnetPoint;
    Transform MagnetPoint
    {
        get
        {
            if (_MagnetPoint == null)
            {
                Debug.LogWarning("No Magnet Point found, returning transform");
                return transform;
            }
            return _MagnetPoint;
        }
    }

    float Distance
    {
        get { return transform.position.z; }
    }

    SphereCollider _collider;
    float _originalRadius;
    public float radius
    {
        get { return _collider.radius; }
        set { _collider.radius = value; }
    }

    BoostState _boostState = BoostState.Cancelled;
    BoostState boostState
    {
        get
        {
            return _boostState;
        }
        set
        {
            _boostState = value;
            OnBoostStateChanged();
        }
    }

    Animator _Animator;
    Vector3 _force;
    Vector3 _OriginalPosition;
    Rigidbody _Rigidbody;
    ArduinoController _ArduinoController;

    int _totalBerries = 0;
    public int totalBerries
    {
        get
        {
            return _totalBerries;
        }
        set
        {
            _totalBerries = value;
            OnFruitAmountChanged();
        }
    }

    int _boostBerries = 0;
    public int boostBerries
    {
        get
        {
            return _boostBerries;
        }
        set
        {
            _boostBerries = value;
            OnFruitAmountChanged();
        }
    }

    void Awake()
    {
        _Animator = GetComponentInChildren<Animator>();
        _Rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();

        _OriginalPosition = transform.position;
        _originalRadius = _collider.radius;
    }

	// Use this for initialization
	void Start ()
    {
        _ArduinoController = ArduinoController.Instance;

        BerriesRequiredToBoost = 10;

        Initialize();
	}

    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
        Collectable.CollectableCollected += Collectable_CollectableCollected;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
        Collectable.CollectableCollected -= Collectable_CollectableCollected;
    }

    private void Collectable_CollectableCollected(object sender, EventArgs e)
    {
        if(boostBerries < BerriesRequiredToBoost)
            boostBerries++;

        totalBerries++;

        // If collected enough berries to boost
        if (boostBerries >= BerriesRequiredToBoost)
        {
            if (boostState == BoostState.Cancelled)
            {
                boostState = BoostState.Ignition;
                StartCoroutine(EnableBoostWindow());
            }
        }
    }

    IEnumerator EnableBoostWindow()
    {
        CanBoost = true;

        yield return new WaitForSeconds(BoostWindow);

        CanBoost = false;
        boostBerries = 0;
        boostState = BoostState.Cancelled;
    }

    private void GameManager_GameStateChanged(object sender, GameStateChangedEventArgs e)
    {
        switch (e.GameState)
        {
            case GameState.Pregame:
                Initialize();
                break;
            case GameState.Playing:
                SetInMotion(true);
                _CanMove = true;
                break;
            case GameState.End:
                SetInMotion(false);
                Land();
                break;
        }
    }

    public void Initialize()
    {
        CancelInvoke();
        StopAllCoroutines();
        ResetAnimationTriggers();
        animationState = AnimationState.Idle;
        totalBerries = 0;
        boostBerries = 0;
        boostParticles.Stop();
        CanBoost = false;
        _CanMove = false;
        takeoff = false;
        SetInMotion(false);
        transform.position = _OriginalPosition;
        transform.rotation = Quaternion.identity;
    }

    public void SetInMotion(bool value)
    {
        _force = value ? MovementForce * 1 : Vector3.zero;
    }

    // Update is called once per frame
    bool forceApplied = false;
    bool takeoff = false;
    void Update()
    {
        if (BoostDetected())
        {
            if (!takeoff)
            {
                StartCoroutine(TakeOff());
            }

            if (CanBoost)
            {
                Boost();
            }
        }

        UpdateDistance();
    }

	void FixedUpdate()
    {
        if (_CanMove)
        {
            Move();
        }
	}

    float _CachedDistance = 0f;
    void UpdateDistance()
    {
        if (_CachedDistance != Distance)
        {
            _CachedDistance = Distance;
            OnDistanceChanged();
        }
    }

    bool _CanMove = false;
    void Move()
    {
        InteractionManager interactionManager = InteractionManager.Instance;
        if (interactionManager != null)
        {
            Vector2 flightScreenPosition = Vector2.Lerp(interactionManager.GetLeftHandScreenPos(), interactionManager.GetRightHandScreenPos(), 0.5f);
            //Debug.Log(flightScreenPosition);

            float x = MathF.ConvertRange(0, 1, HorizontalMin, HorizontalMax, flightScreenPosition.x);
            float y = MathF.ConvertRange(0, 1, VerticalMin, VerticalMax, flightScreenPosition.y);

            GuidedForce = new Vector3(x, y, _Rigidbody.position.z);
        }

        Vector3 targetPos = Vector3.Lerp(_Rigidbody.position, GuidedForce, 3 * Time.deltaTime);
        _Rigidbody.position = targetPos;

        _Rigidbody.MovePosition(_Rigidbody.position + _force * Time.deltaTime);
    }

    bool BoostDetected()
    {
        return ((Input.GetKeyDown(BoostKey) || _ArduinoController.ForceDetected(ForceThreshold, true)) && forceApplied == false);
    }    

    void Boost()
    {
        forceApplied = true;
        StopAllCoroutines();     // Ensure countdown is stopped
        boostState = BoostState.Boosting;       // Change boost state to Boosting
        animationState = AnimationState.Glide;  // Change to glide animation
        radius = BoostRadius;                   // Increase collection radius
        boostParticles.Play();                  // Play boost particles
        _force *= BoostMultiplier;              // Multiply forward force

        Invoke("CancelBoost", BoostTime);
    }

    void CancelBoost()
    {
        forceApplied = false;
        CanBoost = false;
        boostBerries = 0;                       // Reset boost berry bar
        //CancelInvoke();                         // Cancel all invokes
        boostState = BoostState.Cancelled;      // Change boost state to Cancelled
        animationState = AnimationState.Fly;    // Change to fly animation
        radius = _originalRadius;               // Revert collection radius
        boostParticles.Stop();                  // Stop boost particles
        _force = MovementForce;                 // Revert forward force
    }

    IEnumerator TakeOff()
    {
        takeoff = true;

        animationState = AnimationState.Takeoff;   // Set animation to takeoff state
        transform.DOMove(Center, TakeoffSpeed).SetEase(takeOffEaseType);
        do
        {
            //transform.position = Vector3.MoveTowards(this.transform.position, Center, TakeoffSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        } while (Vector3.Distance(transform.position, Center) != 0f);

        GameManager instance = GameManager.Instance;
        if(instance)
            instance.SetState(GameState.Playing);   // Set 
        forceApplied = false;   // Enable boosting
    }

    IEnumerator Landing()
    {
        CanBoost = false;
        _CanMove = false;

        CancelBoost();

        Vector3 targetPos = BirdHouse.Instance.LandingPosition;
        transform.DOLookAt(targetPos, LandingSpeed);
        transform.DOMove(targetPos, LandingSpeed).SetEase(landingEaseType);
        do
        {            
            //transform.LookAt(targetPos);
            //transform.position = Vector3.MoveTowards(this.transform.position, targetPos, LandingSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
            Debug.Log(Vector3.Distance(transform.position, targetPos));
        } while (Vector3.Distance(transform.position, targetPos) > 0.01f);
        animationState = AnimationState.Landing;
        transform.rotation = Quaternion.identity;
        GameManager.Instance.SetState(GameState.Postgame);
        //takeoff = false;
    }

    public void Land()
    {
        StartCoroutine(Landing());
    }

    AnimationState _AnimationState;
    public AnimationState animationState
    {
        get { return _AnimationState; }
        set {
            _AnimationState = value;
            string trigger = value.ToString().ToLower();
            _Animator.SetTrigger(trigger);
            OnAnimationStateChanged();
        }
    }

    public void ResetAnimationTriggers()
    {
        AnimatorControllerParameter param;
        for (int i = 0; i < _Animator.parameters.Length; i++)
        {
            param = _Animator.parameters[i];
            Debug.Log("Parameter name: " + param.name);

            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                _Animator.ResetTrigger(param.nameHash);
            }
        }
    }

    public int BerriesRequiredToBoost { get; private set; }

    void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactable.Interact(this, MagnetPoint);
        }
    }

    private void OnFruitAmountChanged()
    {
        if (FruitAmountChanged != null)
        {
            FruitAmountChanged(this, new FruitAmountChangedEventArgs() { BoostBerries = boostBerries, TotalBerries = totalBerries });
        }
    }

    private void OnDistanceChanged()
    {
        if (DistanceChanged != null)
        {
            DistanceChanged(this, Distance);
        }
    }

    private void OnBoostStateChanged()
    {
        if (BoostStateChanged != null)
        {
            BoostStateChanged(this, new BoostStateChangedEventArgs() { BoostState = boostState });
        }
    }

    private void OnAnimationStateChanged()
    {
        if (AnimationStateChanged != null)
        {
            AnimationStateChanged(this, new AnimationStateChangedEventArgs() { AnimationState = _AnimationState });
        }
    }
}
