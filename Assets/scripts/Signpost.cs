using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Signpost : MonoBehaviour
{
    public Text label;
    
	// Use this for initialization
	void Start () {
	
	}

    public void UpdateLabel(Vector3 position, float distance)
    {
        transform.position = position;
        label.text = string.Format("{0} m", distance);
    }
}
