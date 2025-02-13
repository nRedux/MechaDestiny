using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class UIDamagePop : MonoBehaviour
{

    public TMP_Text Value;

    public AnimationCurve FadeCurve;
    
    public float RiseDistance = 30;
    public float RiseDuration = .5f;

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private RectTransform _container;
    private Vector3 _offset = Vector2.zero;
    private Vector3 _worldPosTarget;


    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
    }


    public void Initialize( Transform container, string number, Vector3 worldPosTarget )
    {
        transform.SetParent( container, false );
        if( this.Value == null )
            return;
        _worldPosTarget = worldPosTarget;
        this.Value.text = number;
        _container = container as RectTransform;

        UpdatePosition();
        DoRise();
    }


    /// <summary>
    /// Make the animation of the number rising up from initial position. Destroys itself when done.
    /// </summary>
    public void DoRise()
    {
        _offset = Vector3.zero;
        CoroutineUtils.DoInterpolation( RiseDuration, f =>
        {
            _offset = Vector3.up * f * RiseDistance;
            _canvasGroup.alpha = FadeCurve.Evaluate( f );
            return true;
        },
        () => { Destroy( gameObject ); }
        );
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if( _container == null )
            return;
        if( _rectTransform != null )
            _rectTransform.SetCanvasScreenPosition( _container, _worldPosTarget + _offset );
    }
}
