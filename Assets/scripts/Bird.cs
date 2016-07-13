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

    Rigidbody rb;

	// Use this for initialization
	void Start ()
    {
        rb = GetComponent<Rigidbody>();
        _originalPosition = transform.position;

        Reset();
	}

    public void Reset()
    {
        transform.position = _originalPosition;
        SetInMotion(false);
        _force = Vector3.zero;
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
        CheckForce();
        UpdateDistance();

        if (Input.GetKeyDown(boostKey) && forceApplied == false)
        {
            boostParticles.Play();
            forceApplied = true;
            _force *= boostMultiplier;
            ArduinoUI.Instance.UpdateMessage("BOOSTING!");
            Invoke("CancelForce", boostTime);
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
        rb.MovePosition(rb.position + _force * Time.deltaTime);
    }

    void CheckForce()
    {
        ArduinoController controller = ArduinoController.Instance;

        if (forceApplied == false)
        {
            if (controller.force >= forceThreshold)
            {
                boostParticles.Play();
                forceApplied = true;
                _force *= boostMultiplier;
                Invoke("CancelForce", boostTime);
            }
        }
        else
        {
            if (controller.force < forceThreshold)
            {
                forceApplied = false;
            }
        }
    }

    void CancelForce()
    {
        boostParticles.Stop();
        _force = movementForce;
        ArduinoUI.Instance.UpdateMessage("");
    }
}
