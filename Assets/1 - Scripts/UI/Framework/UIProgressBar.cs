using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressBar : MonoBehaviour
{

    public Image Fill;
    
    private float _fillAmount = 1f;
    public float Amount 
    { 
        set 
        {
            _fillAmount = value;
            UpdateFill( value );
        }
        get
        {
            return _fillAmount;
        }
    }


    private void UpdateFill( float amount ) 
    {
        Fill.fillAmount = amount;
    }

}
