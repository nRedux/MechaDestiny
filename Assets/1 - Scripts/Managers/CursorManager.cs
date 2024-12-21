using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CursorMode
{
    Normal,
    NormalHighlight,
    Move,
    Attack,
    Grab
}


[CreateAssetMenu(menuName = "Engine/Managers/Cursor Manager")]
public class CursorManager : SingletonScriptableObject<CursorManager>
{

    public Texture2D NormalCursor;
    public Texture2D HighlightCursor;
    public Texture2D MoveCursor;
    public Texture2D AttackCursor;
    public Texture2D GrabCursor;

    private CursorMode _cursorMode;

    private void OnEnable()
    {
        SetCursorMode( CursorMode.Normal );
    }

    public void SetCursorMode( CursorMode mode )
    {
        this._cursorMode = mode;
        switch( _cursorMode )
        {
            case CursorMode.Normal:
                Cursor.SetCursor( NormalCursor, Vector2.zero, UnityEngine.CursorMode.Auto );
                break;
            case CursorMode.NormalHighlight:
                Cursor.SetCursor( HighlightCursor, Vector2.zero, UnityEngine.CursorMode.Auto );
                break;
            case CursorMode.Move:
                Cursor.SetCursor( MoveCursor, Vector2.zero, UnityEngine.CursorMode.Auto );
                break;
            case CursorMode.Attack:
                Cursor.SetCursor( AttackCursor, Vector2.zero, UnityEngine.CursorMode.Auto );
                break;
            case CursorMode.Grab:
                Cursor.SetCursor( GrabCursor, Vector2.zero, UnityEngine.CursorMode.Auto );
                break;
        }
    }
}
