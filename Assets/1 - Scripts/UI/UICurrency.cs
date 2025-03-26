using System;
using TMPro;
using UnityEngine;

public abstract class UICurrency : MonoBehaviour
{
    [SerializeField]
    private TMP_Text ValueText;

    protected void UpdateAmount( int amount )
    {
        ValueText.Opt()?.SetText( amount.ToString() );
    }
}
