using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

[CreateAssetMenu(menuName = "Engine/Tools/AITools")]
public class AITools : SingletonScriptableObject<AITools>
{
    public class Record
    {
        public string Context;
        public string Description;
        public StringBuilder Notes = new StringBuilder();
        public int Turn;
        public int ID;
        public BoolWindow BoolWindow;
        public FloatWindow FloatWindow;
        public string NoteContent;

        public List<GridStarNode> Path { get; internal set; }

        public string GetDescription()
        {
            return $"{Turn}::{ID} - {Context} | {Description}";
        }

        public void Note(string note )
        {
            Notes.Append( note + "| " );
            NoteContent = Notes.ToString();
        }

        public void ClearNotes()
        {
            Notes.Clear();
            NoteContent = string.Empty;
        }
    }


    public System.Action RecordsUpdated;
    
    
    private int _nextRecordID = 0;
    private List<Record> _records = new List<Record>();


    private void OnEnable()
    {
        base.Initialize();
        Debug.Log( "Initialize AITools" );
        _records = new List<Record>();
    }

    protected override void Initialize()
    {


    }

    public Record RecordWindow( BoolWindow boolWindow, string context, string description )
    {
        int turn = GameEngine.Instance.Game.TurnManager.TurnNumber;

        Record r = new Record()
        {
            Turn = turn,
            ID = _nextRecordID++,
            Context = context,
            Description = description,
            BoolWindow = boolWindow
        };

        AddRecord( r );
        return r;
    }

    public Record RecordWindow( FloatWindow floatWindow, string context, string description )
    {
        int turn = GameEngine.Instance.Game.TurnManager.TurnNumber;

        Record r = new Record()
        {
            Turn = turn,
            ID = _nextRecordID++,
            Context = context,
            Description = description,
            FloatWindow = floatWindow
        };

        AddRecord( r );
        return r;
    }

    public Record RecordPath( List<GridStarNode> path, string context, string description )
    {
        int turn = GameEngine.Instance.Game.TurnManager.TurnNumber;

        Record r = new Record()
        {
            Turn = turn,
            ID = _nextRecordID++,
            Context = context,
            Description = description,
            Path = path
        };

        AddRecord( r );
        return r;
    }

    private void AddRecord( Record r )
    {
        _records.Add( r );
        RecordsUpdated?.Invoke();
    }

    public int GetTurnMax()
    {
        if( _records.Count == 0 )
            return 0;
        return _records.Max( x => x.Turn );
    }

    public List<Record> GetTurnRecords( int turn )
    {
        return _records.Where( x => x.Turn == turn ).ToList();
    }

}
