using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using UnityEngine.UI;

public class UIAITools : UIPanel
{
    public Button NextTurnBtn;
    public Button PrevTurnBtn;
    public TMP_InputField TurnInput;
    public TMP_Dropdown RecordsDropdown;
    public GridOverlay RecordDisplay;

    public GameObject NotesRegionRoot;
    public TMP_Text NotesTextArea;

    private int _turn = 0;
    private List<AITools.Record> _records = null;
    private int _selectedRecord = 0;

    public int Turn
    {
        set
        {
            int max = AITools.Instance.Opt()?.GetTurnMax() ?? 0;
            _turn = Mathf.Clamp( value, 0, max );
        }
        get => _turn;
    }


    protected override void Awake()
    {
        base.Awake();
        NextTurnBtn.Opt()?.onClick.AddListener( OnNextTurnClick );
        PrevTurnBtn.Opt()?.onClick.AddListener( OnPrevTurnClick );
        TurnInput.Opt()?.onValueChanged.AddListener( OnTurnChange );

        RecordsDropdown.Opt()?.onValueChanged.AddListener( OnRecordChanged );
        NotesRegionRoot.Opt()?.gameObject.SetActive( false );
        RecordDisplay.Initialize();
    }


    private void OnRecordChanged( int index )
    {
        _selectedRecord = index;
        UpdateGridRender( _selectedRecord );
        //Do record display things.
    }

    private void UpdateGridRender( int recordIndex )
    {
        if( _records == null ) return;
        if( recordIndex < 0 || recordIndex >= _records.Count ) return;
        var selected = _records[recordIndex];
        UIManager.Instance.ClearDebugTextOverlays();
        RecordDisplay.Clear();
        if( selected.BoolWindow != null )
        {
            RecordDisplay.RenderCells( selected.BoolWindow );
        }
        if( selected.FloatWindow != null )
        {
            selected.FloatWindow.Do( x => UIManager.Instance.CreateDebugOverlay( x.world, x.value.ToString() ) );
            RecordDisplay.RenderCells( selected.FloatWindow );
        }
    }


    public override void OnHide()
    {
        UIManager.Instance.ClearDebugTextOverlays();
        RecordDisplay.Clear();
        base.OnHide();
    }

    private void OnTurnChange( string input )
    {
        int res = 0;
        if( Int32.TryParse( input, out res ) )
            Turn = res;

        _selectedRecord = 0;

        if( TurnInput )
            TurnInput.text = Turn.ToString();

        Refresh();
    }


    private void OnPrevTurnClick()
    {
        Turn--;
        _selectedRecord = 0;
        if( TurnInput )
            TurnInput.text = Turn.ToString();
        Refresh();
    }


    private void OnNextTurnClick()
    {
        Turn++;
        _selectedRecord = 0;
        if( TurnInput )
            TurnInput.text = Turn.ToString();
        Refresh();
    }


    public override void Show()
    {
        if( AITools.Instance == null )
            return;
        base.Show();
    }


    public void Refresh()
    {
        UpdateGridRender(0);
        RebuildOptions();
        UpdateNotesDisplay();
    }

    private void UpdateNotesDisplay()
    {
        var selected = _records[_selectedRecord];

        if( !string.IsNullOrEmpty( selected.NoteContent ) )
        {
            NotesRegionRoot.Opt()?.gameObject.SetActive( true );
            NotesTextArea.SetText( selected.NoteContent );
        }
        else
        {
            NotesRegionRoot.Opt()?.gameObject.SetActive( false );
        }
    }


    private void RebuildOptions()
    {
        RecordsDropdown.Opt()?.ClearOptions();

        _records = AITools.Instance.Opt()?.GetTurnRecords( Turn );
        List<TMP_Dropdown.OptionData> opDatum = new List<TMP_Dropdown.OptionData>();
        _records?.Do( x =>
        {
            var opData = new TMP_Dropdown.OptionData( x.GetDescription() );
            opDatum.Add( opData );

        } );
        RecordsDropdown.Opt()?.AddOptions( opDatum );
    }



}
