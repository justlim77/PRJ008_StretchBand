using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Signpost : MonoBehaviour
{
    public Text Label;

    public void UpdateSign(float distance)
    {
        Label.text = string.Format("{0}  m", distance);
    }
}
