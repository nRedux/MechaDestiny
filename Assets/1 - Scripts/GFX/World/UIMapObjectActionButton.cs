using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using UnityEngine;
using UnityEngine.UI;

public class UIMapObjectActionButton : MonoBehaviour
{

    public Image Graphic;

    private GfxMapObjectAction Action;

    private Button _button;
    private System.Action<GfxMapObjectAction> _onClick;

    private void Awake()
    {
        SetupButton();
    }

    private void SetupButton()
    {
        _button = GetComponent<Button>();
        if( _button == null )
            return;
        _button.onClick.AddListener( OnClick );
    }


    public void Initialize( GfxMapObjectAction action, System.Action<GfxMapObjectAction> onClick )
    {
        this.Action = action;
        this._onClick = onClick;
        Refresh( action );
    }

    private void Refresh( GfxMapObjectAction action )
    {
        if( this.Graphic != null )
            this.Graphic.sprite = action.Opt()?.BtnImage;
    }


    public void OnClick()
    {
        this._onClick?.Invoke( Action );
    }
}
