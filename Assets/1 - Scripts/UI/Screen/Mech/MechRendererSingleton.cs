using System;
using UnityEngine;


public class MechRendererException : System.Exception
{
    public MechRendererException() : base() { }
    public MechRendererException( string message ) : base( message ) { }
    public MechRendererException( string message, System.Exception innerException ) : base( message, innerException ) { }
}

[CreateAssetMenu( menuName = "Engine/Utilities/Mech Renderer")]
public class MechRendererSingleton: SingletonScriptableObject<MechRendererSingleton>
{

    public MechRenderer MechRenderer;

    [NonSerialized]
    private MechRenderer _mechRendererInstance;


    public MechRenderer GetRenderer()
    {
        if( _mechRendererInstance != null )
            return _mechRendererInstance;
        if( MechRenderer == null )
            throw new MechRendererException( $"{nameof(MechRenderer)} not assigned. Can't generate mech renders." );
        _mechRendererInstance = Instantiate<MechRenderer>( MechRenderer );
        return _mechRendererInstance;
    }

}
