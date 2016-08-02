using UnityEngine;
using System.Collections;

public class PanelBase : MonoBehaviour
{
    public PanelOrientation PanelOrientation;

    RectTransform _RectTransform;
    Vector3 _TargetPosition;
    Vector3 _CurrentPosition;
    float _TranslationSpeed = 2000.0f;

	// Use this for initialization
	protected void Start ()
    {
        _RectTransform = GetComponent<RectTransform>();
        Vector2 scale = GetOrientation(PanelOrientation);
        _RectTransform.anchoredPosition =  new Vector2(1280 * scale.x, 720 * scale.y);
        _CurrentPosition = _RectTransform.anchoredPosition;
        _TargetPosition = _CurrentPosition;
	}

    // Update is called once per frame
    protected void Update ()
    {
        _CurrentPosition = _RectTransform.anchoredPosition;

        if(_CurrentPosition != _TargetPosition)
            _RectTransform.anchoredPosition = Vector3.MoveTowards(_RectTransform.anchoredPosition, _TargetPosition, _TranslationSpeed * Time.deltaTime);
	}

    public Vector2 AnchoredPosition
    {
        get { return _RectTransform.anchoredPosition; }
        set { _RectTransform.anchoredPosition = value; }
    }

    public void Slide(Vector2 targetPosition)
    {
        _TargetPosition = targetPosition;
    }

    Vector2 GetOrientation(PanelOrientation orientation)
    {
        Vector2 pos = Vector2.zero;
        switch (orientation)
        {
            case PanelOrientation.Center:
                pos = Vector2.zero;
                break;
            case PanelOrientation.Up:
                pos = Vector2.up;
                break;
            case PanelOrientation.Down:
                pos = Vector2.down;
                break;
            case PanelOrientation.Left:
                pos = Vector2.left;
                break;
            case PanelOrientation.Right:
                pos = Vector2.right;
                break;           
        }
        return pos;
    }

    public void SetOrientation(PanelOrientation orientation, Vector2 referenceResolution)
    {
        Vector2 scale = GetOrientation(orientation);
        _TargetPosition = new Vector2(scale.x * referenceResolution.x, scale.y * referenceResolution.y);
    }
}

[System.Serializable]
public enum PanelOrientation
{
    Up,
    Down,
    Left,
    Right,
    Center
}
