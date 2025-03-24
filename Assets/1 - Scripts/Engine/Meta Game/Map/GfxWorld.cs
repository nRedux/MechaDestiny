using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class GfxWorld : MonoBehaviour
{

    public GameObject Light;
    public Transform StartPos;
    public Transform TargetPos;

    private TimeData _dayData;
    private Vector3 _lightEulerStart;
    private GfxMoveableMapObject _caravanGfx = null;

    private void Awake()
    {
        if( Light )
            _lightEulerStart = Light.transform.eulerAngles;
        AddListeners();
    }

    private void Start()
    {
        CreateCaravanGraphics();
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    private void AddListeners()
    {
        Events.Instance.AddListener<DoSceneWarmup>( DoSceneWarmup );
    }

    private void RemoveListeners()
    {
        Events.Instance.RemoveListener<DoSceneWarmup>( DoSceneWarmup );
    }

    private void DoSceneWarmup( DoSceneWarmup e )
    {
        RunManager.Instance.RunData.Caravan.Position = StartPos.position;
    }


    private void Update()
    {
        //UpdateSun();
    }

    private void UpdateSun()
    {
        if( Light == null )
            return;

        Light.transform.eulerAngles = _lightEulerStart + new Vector3( RunManager.Instance.RunData.TimeData.Days * 360f, 0f, 0f);
    }


    private async void CreateCaravanGraphics()
    {
        var runData = RunManager.Instance.RunData;
        var caravanAsset = await runData.Caravan.Graphics.GetAssetAsync();

        var instGO = Instantiate<GameObject>( caravanAsset );
        _caravanGfx = instGO.GetComponent<GfxMoveableMapObject>();
        _caravanGfx.Initialize( runData.Caravan );
    }


}
