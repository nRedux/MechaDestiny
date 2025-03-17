using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CircleDecal : MonoBehaviour
{
    DecalProjector Projector;
    Material _materialCopy;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        Projector = GetComponent<DecalProjector>();
        if( Projector == null )
            return;
        Material original = Projector.material;
        if( original == null )
            return;
        _materialCopy = new Material( original );
        Projector.material = _materialCopy;
    }

    public void SetColor( Color color )
    {
        _materialCopy.SetColor( "_Tint", color );
    }

    public void SetThickness( float thickness )
    {
        _materialCopy.SetFloat( "_Thickness", thickness );
    }

    public void SetRadius( float radius )
    {
        _materialCopy.SetFloat( "_Radius", radius );
    }
}
