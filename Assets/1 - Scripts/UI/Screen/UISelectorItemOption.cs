using UnityEngine;
using UnityEngine.UI;

public class UISelectorItemOption<TDataSource> : MonoBehaviour
{
    public delegate void ItemPickedDelegate( UISelectorItemOption<TDataSource> result );

    public TDataSource DataSource { get; private set; }

    private ItemPickedDelegate _pickedCallback;

    public void Initialize( TDataSource dataSource, ItemPickedDelegate onPicked )
    {
        if( dataSource == null )
        {
            throw new System.ArgumentNullException( $"{nameof( dataSource )} argument must not be null" );
        }
        this._pickedCallback = onPicked;
        this.DataSource = dataSource;
        Refresh();
        ListenButtonClicks();
    }

    private bool ListenButtonClicks()
    {
        Button btn = GetComponent<Button>();
        btn.Opt()?.onClick.AddListener( OnCLick );
        return btn.Opt() != null;
    }

    private void OnCLick()
    {
        _pickedCallback?.Invoke( this );
    }

    public virtual void Refresh()
    {
    }
}