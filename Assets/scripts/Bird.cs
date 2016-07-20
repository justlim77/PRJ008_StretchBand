using UnityEngine;
using System.Collections;

public class Bird : MonoBehaviour
{
    public Vector3 movementForce;
    public float boostTime = 5.0f;
    public float boostMultiplier = 2.0f;
    public Vector3 force;
    public int forceThreshold = 10;
    public KeyCode boostKey = KeyCode.Space;
    public ParticleSystem boostParticles;

    public float Distance
    {
        get; private set;
    }

    Vector3 _force;
    Vector3 _originalPosition;
    Rigidbody _rb;
    ArduinoController _controller;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _originalPosition = transform.position;
    }

	// Use this for initialization
	void Start ()
    {
        _controller = ArduinoController.Instance;

        Initialize();
	}

    public void Initialize()
    {
        CancelInvoke();
        transform.position = _originalPosition;
        Distance = 0;
        SetInMotion(false);
        boostParticles.Stop();
    }

    public void SetInMotion(bool value)
    {
        _force = value ? movementForce * 1 : Vector3.zero;
    }

    // Update is called once per frame
    bool forceApplied = false;
    void Update()
    {
        GameManager manager = GameManager.Instance;

        // Check for initial stretch to start
        if (manager.gameState.Equals(GameState.Pregame) && _controller.ForceDetected())
        {
            manager.SetState(GameState.Playing);
            Boost();
        }

        CheckForce();
        UpdateDistance();

        // Spacebar start
        if (Input.GetKeyDown(boostKey) && forceApplied == false)
        {
            forceApplied = true;
            Boost();
        }
    }

	void FixedUpdate ()
    {
        Move();
	}

    void UpdateDistance()
    {
        Distance = transform.position.z;
    }

    void Move()
    {
        _rb.MovePosition(_rb.position + _force * Time.deltaTime);
    }

    void CheckForce()
    {
        if (_controller.ForceDetected(forceThreshold))
        {
            Boost();
        }
    }    

    void Boost()
    {
        if (forceApplied == true)
            return;

        forceApplied = true;
        boostParticles.Play();
        _force *= boostMultiplier;
        ArduinoUI.Instance.UpdateMessage("BOOSTING!");

        Invoke("CancelBoost", boostTime);
    }

    void CancelBoost()
    {
        forceApplied = false;
        boostParticles.Stop();
        _force = movementForce;
        ArduinoUI.Instance.UpdateMessage("");
    }
}
