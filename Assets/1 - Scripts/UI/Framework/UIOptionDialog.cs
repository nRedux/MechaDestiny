using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIOptionDialog : MonoBehaviour
{

    public TMP_Text HeaderText;
    public TMP_Text MessageText;
    public Button AcceptButton;
    public Button CancelButton;

    private TMP_Text _acceptText;
    private TMP_Text _cancelText;

    private System.Action _accept;
    private System.Action _cancel;

    private void Awake()
    {
        _acceptText = AcceptButton.GetComponentInChildren<TMP_Text>();
        _cancelText = CancelButton.GetComponentInChildren<TMP_Text>();

        if( AcceptButton != null )
            AcceptButton.onClick.AddListener( AcceptClicked );
        if( CancelButton != null )
            CancelButton.onClick.AddListener( CancelClicked );

    }


    public void ShowAcceptCancel( string headerText, string messageText, System.Action accept, System.Action cancel )
    {
        HeaderText.Opt()?.gameObject.SetActive( headerText != null );
        MessageText.Opt()?.gameObject.SetActive( messageText != null );
        AcceptButton.Opt()?.gameObject.SetActive( true );
        CancelButton.Opt()?.gameObject.SetActive( true );

        _accept = accept;
        _cancel = cancel;
        gameObject.SetActive( true );

        HeaderText.Opt()?.SetText( headerText );
        MessageText.Opt()?.SetText( messageText );
        _acceptText.Opt()?.SetText( "Accept" );
        _cancelText.Opt()?.SetText( "Cancel" );
    }

    public void ShowAccept( string headerText, string messageText, System.Action accept )
    {
        HeaderText.Opt()?.gameObject.SetActive( headerText != null );
        MessageText.Opt()?.gameObject.SetActive( messageText != null );
        AcceptButton.Opt()?.gameObject.SetActive( true );
        CancelButton.Opt()?.gameObject.SetActive( false );

        _accept = accept;
        _cancel = null;
        gameObject.SetActive( true );

        HeaderText.Opt()?.SetText( headerText );
        MessageText.Opt()?.SetText( messageText );
        _acceptText.Opt()?.SetText( "Accept" );
    }

    private void AcceptClicked()
    {
        _accept?.Invoke();
        gameObject.SetActive( false );
    }

    private void CancelClicked()
    {
        _cancel?.Invoke();
        gameObject.SetActive( false );
    }

}
