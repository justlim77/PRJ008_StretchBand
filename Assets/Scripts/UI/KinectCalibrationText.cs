using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KinectCalibrationText : MonoBehaviour
{
    private Text _Text;
    public Text Text
    {
        get
        {
            if (_Text == null)
            {
                _Text = GetComponent<Text>();
            }
            return _Text;
        }
    }
}
