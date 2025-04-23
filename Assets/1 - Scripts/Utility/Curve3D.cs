#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;


[System.Serializable]
public class Curve3D
{
    public AnimationCurve CurveX;
    public AnimationCurve CurveY;
    public AnimationCurve CurveZ;

    public int KeyCount => CurveX.keys.Length;

    public void AddKey( float time, Vector3 pos )
    {
        CurveX.AddKey( new Keyframe( time, pos.x ) );
        CurveY.AddKey( new Keyframe( time, pos.y ) );
        CurveZ.AddKey( new Keyframe( time, pos.z ) );
    }

    public Vector3 Evaluate( float time )
    {
        Vector3 result = new Vector3( CurveX.Evaluate( time ), CurveY.Evaluate( time ), CurveZ.Evaluate( time ) );
        return result;
    }

    public void SmoothCurve()
    {
#if UNITY_EDITOR
        int keyCount = CurveX.keys.Length;
        for( int i = 0; i < keyCount; i++ )
        {
            AnimationUtility.SetKeyLeftTangentMode( CurveX, i, AnimationUtility.TangentMode.Auto );
            AnimationUtility.SetKeyRightTangentMode( CurveX, i, AnimationUtility.TangentMode.Auto );
            AnimationUtility.SetKeyLeftTangentMode( CurveY, i, AnimationUtility.TangentMode.Auto );
            AnimationUtility.SetKeyRightTangentMode( CurveY, i, AnimationUtility.TangentMode.Auto );
            AnimationUtility.SetKeyLeftTangentMode( CurveZ, i, AnimationUtility.TangentMode.Auto );
            AnimationUtility.SetKeyRightTangentMode( CurveZ, i, AnimationUtility.TangentMode.Auto );
        }
#endif
    }

    public void ClearKeys()
    {
        CurveX.ClearKeys();
        CurveY.ClearKeys();
        CurveZ.ClearKeys();
    }
}