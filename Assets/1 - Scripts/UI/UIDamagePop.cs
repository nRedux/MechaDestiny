using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDamagePop : MonoBehaviour
{

    public TMP_Text Value;

    public AnimationCurve FadeCurve;
    
    public float RiseDistance = 30;
    public float RiseDuration = .5f;

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Vector2 _riseStart;


    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
    }


    public void Initialize( string number )
    {
        if( this.Value == null )
            return;
        this.Value.text = number;
    }


    /// <summary>
    /// Make the animation of the number rising up from initial position. Destroys itself when done.
    /// </summary>
    public void DoRise()
    {
        _riseStart = _rectTransform.anchoredPosition;
        CoroutineUtils.DoInterpolation( RiseDuration, f =>
        {
            _rectTransform.anchoredPosition = _riseStart + Vector2.up * f * RiseDistance;
            _canvasGroup.alpha = FadeCurve.Evaluate( f );
            return true;
        },
        () => { Destroy( gameObject ); }
        );
    }
}
