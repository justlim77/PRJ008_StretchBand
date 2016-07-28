using UnityEngine;
using System.Collections;
using System;

public class Collectable : MonoBehaviour, IInteractable
{
    public float magnetSpeed = 5.0f;
    public float shrinkSpeed = 1.0f;
    public float destroyTime = 3.0f;

    Transform _Target;

    bool _HasInteracted = false;
    public void Interact(object sender, object arg)
    {
        if (_HasInteracted)
            return;

        if (arg is Transform)
        {
            _HasInteracted = true;

             _Target = (Transform)arg;
            Destroy(gameObject, destroyTime);
        }
    }

    bool _Registered = false;
	void Update ()
    {
        if (_Target != null)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, shrinkSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, _Target.position, magnetSpeed * Time.deltaTime);

            if (!_Registered)
            {
                float dist = Vector3.Distance(transform.position, _Target.position);
                if (dist < 0.1f)
                {
                    _Registered = true;
                    GameManager.Instance.AddBerry();
                }
            }
        }
	}
}
