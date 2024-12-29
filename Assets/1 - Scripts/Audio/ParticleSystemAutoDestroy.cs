using UnityEngine;

public class ParticleSystemAutoDestroy : MonoBehaviour
{
    private ParticleSystem[] _ps;

    void Start()
    {
        _ps = GetComponentsInChildren<ParticleSystem>();
        if( _ps.Length == 0 )
            enabled = false;
    }

    void Update()
    {
        bool done = true;
        _ps.Do( x => done &= !x.IsAlive() );
        if( done )
        {
            transform.DestroyChildren();
            Destroy( gameObject );
        }
    }
}
