using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KinectProgressBar : MonoBehaviour
{
    private Image _Image;
    public Image Image
    {
        get
        {
            if (_Image == null)
            {
                _Image = GetComponent<Image>();
            }
            return _Image;
        }
    }
}
