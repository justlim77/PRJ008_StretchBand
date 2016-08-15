using UnityEngine;
using System.Collections;
using System;

public class Collectable : MonoBehaviour, IInteractable
{
    Transform _Target = null;
    Vector3 _InitialPos = Vector3.zero;
    Vector3 _InitialScale = Vector3.zero;

    public delegate void CollectableCollectedEventHandler(object sender, EventArgs e);
    public static event CollectableCollectedEventHandler CollectableCollected;

    float accelerationFactor = 0.5f;

    void Awake()
    {
        _InitialPos = this.transform.position;
        _InitialScale = this.transform.localScale;
    }

    public bool Initialize()
    {
        _Target = null;

        CancelInvoke();

        transform.position = _InitialPos;
        transform.localScale = _InitialScale;

        _HasInteracted = false;
        _Registered = false;

        accel = 0.0f;

        GetComponent<Renderer>().enabled = true;

        GameManager.GameStateChanged -= GameManager_GameStateChanged;

        return true;
    }

    bool _HasInteracted = false;
    public void Interact(object sender, object arg)
    {
        if (_HasInteracted)
            return;

        if (arg is Transform)
        {
            _HasInteracted = true;
            GameManager.GameStateChanged += GameManager_GameStateChanged;
             _Target = (Transform)arg;
        }
    }

    private void GameManager_GameStateChanged(object sender, GameStateChangedEventArgs e)
    {
        switch (e.GameState)
        {
            case GameState.End:
                Initialize();
                break;
        }
    }

    float accel = 0.0f;
    bool _Registered = false;
	void Update ()
    {
        if (_Target != null)
        {
            accel += accelerationFactor; 
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Constants.collectable_shrink_speed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, _Target.position, Constants.collectable_magnet_speed * accel * Time.deltaTime);

            if (!_Registered)
            {
                float dist = Vector3.Distance(transform.position, _Target.position);
                if (dist < 0.1f)
                {
                    _Registered = true;
                    OnCollectableCollected();
                    GetComponent<Renderer>().enabled = false;
                    Invoke("Initialize", Constants.collectable_destroy_time);
                }
            }
        }
	}

    private void OnCollectableCollected()
    {
        if (CollectableCollected != null)
        {
            CollectableCollected(this, null);
        }
    }
}
