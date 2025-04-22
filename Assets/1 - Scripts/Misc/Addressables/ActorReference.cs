using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;

[System.Serializable]
public class ActorReference : DataProviderReference<ActorAsset, Actor>
{
    public ActorReference( string guid ) : base( guid ) { }
}

public class ActorReferenceDescriptor : SimpleReferenceDescriptor<ActorReference> { }
