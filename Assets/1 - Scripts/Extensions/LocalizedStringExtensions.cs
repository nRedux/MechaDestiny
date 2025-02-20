using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public static class LocalizedStringExtensions
{
    public static string TryGetLocalizedString( this LocalizedString _this, string debugStringName )
    {
        string result = string.Empty;
        bool failure = false;
        try
        {
            result = _this.GetLocalizedString();
        }
        catch( System.Exception e )
        {
            failure = true;
            Debug.LogException( e );
        }
        finally
        {
            if( failure )
            {
                result = $"FAIL_LOC: {debugStringName}";
            }
        }
        return result;
    }

    public static string TryGetLocalizedString( this LocalizedString _this, Object failContext = null )
    {
        string result = string.Empty;
        bool failure = false;
        try
        {
            result = _this.GetLocalizedString();
        }
        catch( System.Exception e )
        {
            failure = true;
           // Debug.LogError( "TryGetLocalization: Bad LocalizedString setup.", failContext );
        }
        finally
        {
            if( failure )
            {
                result = $"FAIL_LOC";
            }
        }
        return result;
    }
}
