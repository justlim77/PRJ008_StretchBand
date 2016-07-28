using UnityEngine;
using System.Collections;

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


    public float Distance
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

    float _GroundHeight = 0.5f;
    Animator _Animator;
    Vector3 _Force;
    Vector3 _OriginalPosition;
    Rigidbody _Rigidbody;
    ArduinoController _ArduinoController;

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

        Initialize();
	}

    public void Initialize()
    {
        CancelInvoke();
        transform.position = _OriginalPosition;
        SetInMotion(false);
        BoostParticles.Stop();
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
        GameManager manager = GameManager.Instance;

        // Check for initial stretch to start
        if (manager.GameState.Equals(GameState.Pregame) && _ArduinoController.ForceDetected())
        {
            manager.SetState(GameState.Playing);
            Boost();
        }

        // Spacebar start
        if (Input.GetKeyDown(BoostKey) && forceApplied == false)
        {
            forceApplied = true;

            if (takeoff == false)
            {
                takeoff = true;
                StartCoroutine(TakeOff());
            }
            else
            {
                Boost();        
            }
        }
        
        CheckForce();
        UpdateDistance();
    }

	void FixedUpdate ()
    {
        Move();
	}

    void UpdateDistance()
    {
        //Distance = transform.position.z;
    }

    void Move()
    {
        if (GameManager.Instance.GameState == GameState.Playing)
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
    }

    void CheckForce()
    {
        if (_ArduinoController.ForceDetected(ForceThreshold))
        {
            Boost();
        }
    }    

    void Boost()
    {
        forceApplied = true;

        SetAnimation(AnimationState.Glide); // Change to glide animation
        Radius = BoostRadius;               // Increase collection radius
        BoostParticles.Play();              // Play boost particles
        _Force *= BoostMultiplier;          // Multiply forward force
        ArduinoUI.Instance.UpdateMessage("BOOSTING!");  // Update message

        Invoke("CancelBoost", BoostTime);
    }

    void CancelBoost()
    {
        forceApplied = false;

        SetAnimation(AnimationState.Fly);   // Change to fly animation
        Radius = _OriginalRadius;           // Revert collection radius
        BoostParticles.Stop();              // Stop boost particles
        _Force = MovementForce;             // Revert forward force
        ArduinoUI.Instance.UpdateMessage("");   // Update message
    }

    IEnumerator TakeOff()
    {
        SetAnimation(AnimationState.Takeoff);   // Set animation to takeoff state
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
        Vector3 targetPos = GameManager.Instance.BirdHouse.LandingPosition;
        do
        {
            transform.position = Vector3.MoveTowards(this.transform.position, targetPos, LandingSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        } while (Vector3.Distance(transform.position, targetPos) != 0f);
        SetAnimation(AnimationState.Landing);
        takeoff = false;
    }

    public void Land()
    {
        StartCoroutine(Landing());
    }

    void SetAnimation(AnimationState state)
    {
        string trigger = state.ToString().ToLower();
        _Animator.SetTrigger(trigger);
    }

    void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactable.Interact(this, MagnetPoint);
        }
    }
}
