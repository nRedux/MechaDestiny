using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimeControl : MonoBehaviour
{

    public Button PlayButton;
    public Sprite PlaySprite;
    public Sprite PauseSprite;

    private Image PlayButtonImage;


    // Start is called before the first frame update
    void Awake()
    {
        PlayButtonImage = PlayButton.GetComponent<Image>();

        PlayButton.onClick.AddListener( PlayButtonClick );
        UpdatePlayButtonImage( TimeManager.IsPlaying );
        AddListeners();
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    private void AddListeners()
    {
        Events.Instance.AddListener<TimeModeChanged>( OnTimeModeChanged );
    }

    private void RemoveListeners()
    {
        Events.Instance.RemoveListener<TimeModeChanged>( OnTimeModeChanged );
    }

    private void OnTimeModeChanged( TimeModeChanged e )
    {
        UpdatePlayButtonImage( e.IsPlaying );
    }

    private void PlayButtonClick()
    {
        TimeManager.TogglePlaying();
    }

    private void UpdatePlayButtonImage( bool playing )
    {
        if( !playing )
            PlayButtonImage.sprite = PlaySprite;
        else
            PlayButtonImage.sprite = PauseSprite;
    }

}
