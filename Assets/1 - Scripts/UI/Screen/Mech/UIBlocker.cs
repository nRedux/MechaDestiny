using UnityEngine;
using UnityEngine.EventSystems;

public class UIBlocker : MonoBehaviour, IPointerClickHandler
{

    private System.Action _onClicked;

    public void ListenForClicks( System.Action handler )
    {
        if( _onClicked == handler )
            return;
        _onClicked += handler;
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        _onClicked?.Invoke();
    }

}
