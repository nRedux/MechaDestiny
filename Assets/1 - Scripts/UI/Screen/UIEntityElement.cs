using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEntityElement : MonoBehaviour
{

    private UIEntity _uiEntity;

    protected virtual void Awake()
    {
        Initialize();
    }

    protected virtual void Start()
    {
        
    }


    protected virtual void OnDestroy()
    {
        if( _uiEntity == null )
            return;

        _uiEntity.EntityAssigned -= OnEntityAssigned;
    }


    private void Initialize()
    {
        _uiEntity = GetComponentInParent<UIEntity>(includeInactive: true);

        if( _uiEntity == null )
        {
            Debug.LogError("UIEntity not found in parents.", gameObject);
            return;
        }

        _uiEntity.EntityAssigned += OnEntityAssigned;
    }


    public virtual void OnEntityAssigned( IEntity entity )
    {

    }

}
