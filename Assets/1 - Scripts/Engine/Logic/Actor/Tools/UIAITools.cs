using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sirenix.Utilities;
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
    private AITools.Record _activeRecord;

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
        TurnInput.Opt()?.onValueChanged.AddListener( OnUserTurnInputChange );

        RecordsDropdown.Opt()?.onValueChanged.AddListener( OnRecordChanged );
        NotesRegionRoot.Opt()?.gameObject.SetActive( false );
        RecordDisplay.Initialize();

        if( AITools.Instance != null )
        {
            AITools.Instance.RecordsUpdated = NewRecordsAvailable;
        }
        
    }


    private void NewRecordsAvailable()
    {
        RebuildOptions();
    }


    private void OnRecordChanged( int index )
    {
        SelectRecord( index );
        if( _activeRecord == null )
            return;
        PresentRecordContent();
        //Do record display things.
    }


    private void UpdateGridRender()
    {
        if( _activeRecord == null ) return;

        UIManager.Instance.ClearDebugTextOverlays();
        RecordDisplay.Clear();
        int range = 0;
        if( _activeRecord.BoolWindow != null )
        {
            //Display cells
            range = _activeRecord.FloatWindow.Width / 2;
            RecordDisplay.RenderCells( _activeRecord.BoolWindow, maxDistance: range );
        }
        if( _activeRecord.FloatWindow != null )
        {
            range = _activeRecord.FloatWindow.Width / 2;
            //display values of all cells
            _activeRecord.FloatWindow.Do( x => UIManager.Instance.CreateDebugOverlay( x.world, x.value.ToString( "0.00" ) ), range );
            //Display cells
            RecordDisplay.RenderCells( _activeRecord.FloatWindow, maxDistance: range );
        }
    }


    private void UpdatePathRender( )
    {

    }


    public override void OnHide()
    {
        UIManager.Instance.ClearDebugTextOverlays();
        RecordDisplay.Clear();
        base.OnHide();
    }


    private void OnUserTurnInputChange( string input )
    {
        int res = 0;
        if( Int32.TryParse( input, out res ) )
            Turn = res;

        SelectFirstRecord();

        if( TurnInput )
            TurnInput.text = Turn.ToString();

        Refresh();
    }


    private void SelectFirstRecord()
    {
        if( _records == null )
        {
            _activeRecord = null;
            return;
        }

        _activeRecord = _records.FirstOrDefault();
    }

    private void SelectLatestRecord()
    {
        if( _records == null )
        {
            _activeRecord = null;
            return;
        }

        _activeRecord = _records.LastOrDefault();
    }


    private void SelectRecord( int index )
    {
        if( index >= _records.Count )
        {
            _activeRecord = null;
            return;
        }

        _activeRecord = _records[index];
    }


    private void OnPrevTurnClick()
    {
        Turn--;
        RebuildOptions();
        SelectFirstRecord();
        if( TurnInput )
            TurnInput.text = Turn.ToString();
        Refresh();
    }


    private void OnNextTurnClick()
    {
        Turn++;
        RebuildOptions();
        SelectFirstRecord();
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
        RebuildOptions();

        if( _activeRecord == null ) return;
        PresentRecordContent();
    }

    private void PresentRecordContent()
    {
        UpdateNotesDisplay();
        UpdateGridRender();
        UpdatePathRender();
    }


    private void UpdateNotesDisplay()
    {
        if( !string.IsNullOrEmpty( _activeRecord.NoteContent ) )
        {
            NotesRegionRoot.Opt()?.gameObject.SetActive( true );
            NotesTextArea.SetText( _activeRecord.NoteContent );
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
