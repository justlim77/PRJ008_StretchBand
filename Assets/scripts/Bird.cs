using UnityEngine;
using System.Collections;
using System;

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
    public float LandingSpeed = 3.0f;
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
    public float BoostMultiplier = 2.0f;
    public KeyCode BoostKey = KeyCode.Space;
    public ParticleSystem BoostParticles;
    public float BoostRadius = 5.0f;
    public BoostBar BoostBar;
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

    SphereCollider _Collider;
    float _OriginalRadius;
    public float Radius
    {
        get { return _Collider.radius; }
        set { _Collider.radius = value; }
    }

    BoostState _BoostState = BoostState.Cancelled;
    BoostState BoostState
    {
        get
        {
            return _BoostState;
        }
        set
        {
            _BoostState = value;
            OnBoostStateChanged();
        }
    }

    Animator _Animator;
    Vector3 _Force;
    Vector3 _OriginalPosition;
    Rigidbody _Rigidbody;
    ArduinoController _ArduinoController;

    int _TotalBerries = 0;
    public int TotalBerries
    {
        get
        {
            return _TotalBerries;
        }
        set
        {
            _TotalBerries = value;
            OnFruitAmountChanged();
        }
    }

    int _BoostBerries = 0;
    public int Berries
    {
        get
        {
            return _BoostBerries;
        }
        set
        {
            _BoostBerries = value;
            OnFruitAmountChanged();
        }
    }

    void Awake()
    {
        _Animator = GetComponentInChildren<Animator>();
        _Rigidbody = GetComponent<Rigidbody>();
        _Collider = GetComponent<SphereCollider>();

        _OriginalPosition = transform.position;
        _OriginalRadius = _Collider.radius;
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
        Berries++;
        TotalBerries++;

        // If collected enough berries to boost
        if (Berries >= BerriesRequiredToBoost)
        {
            if (BoostState == BoostState.Cancelled)
            {
                BoostState = BoostState.Ignition;
                StartCoroutine(EnableBoostWindow());
            }
        }
    }

    IEnumerator EnableBoostWindow()
    {
        CanBoost = true;

        yield return new WaitForSeconds(BoostTime);

        CanBoost = false;

        Berries = 0;
        BoostState = BoostState.Cancelled;
    }

    private void GameManager_GameStateChanged(object sender, GameStateChangedEventArgs e)
    {
        switch (e.GameState)
        {
            case GameState.Pregame:
                Initialize();
                break;
            case GameState.Playing:
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
        AnimationState = AnimationState.Idle;
        TotalBerries = 0;
        Berries = 0;
        BoostBar.Reset();
        BoostParticles.Stop();
        CanBoost = false;
        _CanMove = false;
        takeoff = false;
        SetInMotion(false);
        transform.position = _OriginalPosition;
        transform.rotation = Quaternion.identity;
    }

    public void SetInMotion(bool value)
    {
        _Force = value ? MovementForce * 1 : Vector3.zero;
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

        _Rigidbody.MovePosition(_Rigidbody.position + _Force * Time.deltaTime);
    }

    bool BoostDetected()
    {
        return ((Input.GetKeyDown(BoostKey) || _ArduinoController.ForceDetected(ForceThreshold, true)) && forceApplied == false);
    }    

    void Boost()
    {
        forceApplied = true;

        BoostState = BoostState.Boosting;
        AnimationState = AnimationState.Glide;          // Change to glide animation
        Radius = BoostRadius;                           // Increase collection radius
        BoostBar.SetBoostColor();                       // Change bar color
        BoostParticles.Play();                          // Play boost particles
        _Force *= BoostMultiplier;                      // Multiply forward force

        Invoke("CancelBoost", BoostTime);
    }

    void CancelBoost()
    {
        forceApplied = false;

        BoostState = BoostState.Cancelled;
        AnimationState = AnimationState.Fly;    // Change to fly animation
        Radius = _OriginalRadius;               // Revert collection radius
        BoostParticles.Stop();                  // Stop boost particles
        Berries = 0;                            // Reset boost berry bar
        _Force = MovementForce;                 // Revert forward force
    }

    IEnumerator TakeOff()
    {
        takeoff = true;

        AnimationState = AnimationState.Takeoff;   // Set animation to takeoff state
        do
        {
            transform.position = Vector3.MoveTowards(this.transform.position, Center, TakeoffSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        } while (Vector3.Distance(transform.position, Center) != 0f);

        GameManager.Instance.SetState(GameState.Playing);   // Set 
        forceApplied = false;   // Enable boosting
    }

    IEnumerator Landing()
    {
        CanBoost = false;
        _CanMove = false;

        CancelBoost();

        Vector3 targetPos = GameManager.Instance.BirdHouse.LandingPosition;
        do
        {
            transform.LookAt(targetPos);
            transform.position = Vector3.MoveTowards(this.transform.position, targetPos, LandingSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        } while (Vector3.Distance(transform.position, targetPos) != 0f);
        AnimationState = AnimationState.Landing;
        transform.rotation = Quaternion.identity;

        //takeoff = false;
    }

    public void Land()
    {
        StartCoroutine(Landing());
    }

    AnimationState _AnimationState;
    public AnimationState AnimationState
    {
        get { return _AnimationState; }
        set {
            _AnimationState = value;
            string trigger = value.ToString().ToLower();
            _Animator.SetTrigger(trigger);
            OnAnimationStateChanged();
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
            FruitAmountChanged(this, new FruitAmountChangedEventArgs() { BoostBerries = Berries, TotalBerries = TotalBerries });
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
            BoostStateChanged(this, new BoostStateChangedEventArgs() { BoostState = BoostState });
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
