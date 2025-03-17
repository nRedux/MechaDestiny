using UnityEngine;
using UnityEngine.UI;

public enum UITransitionAction
{
    Show,
    Hide
}

public enum UITransitionStatus
{
    FadingIn,
    Visible,
    FadingOut,
    Invisible
}

public class UITransitionRequestEvent: GameEvent
{
    public UITransitionAction action;
    public bool instant;
    public float duration = 1f;

    public System.Action OnComplete;

    public UITransitionRequestEvent(UITransitionAction action, bool instant = false, float duration = .5f)
    {
        this.instant = instant;
        this.action = action;
        this.duration = duration;
    }

    public UITransitionRequestEvent(UITransitionAction action)
    {
        this.instant = false;
        this.action = action;
    }

    public UITransitionRequestEvent(UITransitionAction action, float duration)
    {
        this.instant = false;
        this.duration = duration;
        this.action = action;
    }
}

public class UITransitionStatusEvent : GameEvent
{
    public UITransitionStatus status;

    public UITransitionStatusEvent(UITransitionStatus status)
    {
        this.status = status;
    }
}

public class UIScreenFader : MonoBehaviour
{
    [SerializeField]
    Image fader;

    Coroutine currentActionRoutine;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        fader.color = Color.clear;
        fader.gameObject.SetActive(false);
        //fader.color = Color.black;
        Events.Instance.AddListener<UITransitionRequestEvent>(OnTransitionEvent);
    }

    public void Show( float duration, System.Action onComplete )
    {
        UITransitionRequestEvent e = new UITransitionRequestEvent( UITransitionAction.Show, duration );
        e.OnComplete = onComplete;
        this.OnTransitionEvent( e );
    }

    public void Hide( float duration, System.Action onComplete )
    {
        UITransitionRequestEvent e = new UITransitionRequestEvent( UITransitionAction.Hide, duration );
        e.OnComplete = onComplete;
        this.OnTransitionEvent( e );
    }

    private void OnTransitionEvent(UITransitionRequestEvent e)
    {
        Color startColor = fader.color;
        Color endColor = fader.color;
        switch ( e.action)
        {
            case UITransitionAction.Show:
                fader.gameObject.SetActive(true);
                endColor.a = 1f;
                CoroutineUtils.EndCoroutine(ref currentActionRoutine);

                if (e.instant)
                {
                    fader.color = endColor;
                    e.OnComplete?.Invoke();
                    return;
                }

                currentActionRoutine = CoroutineUtils.DoInterpolation(e.duration, f =>
                {
                    fader.color = Color.Lerp(startColor, endColor, Mathfx.Hermite(0f, 1f, f));
                    return true;
                }, () =>
                {
                    e.OnComplete?.Invoke();
                });
                break;
            case UITransitionAction.Hide:
                endColor.a = 0f;
                CoroutineUtils.EndCoroutine(ref currentActionRoutine);

                if (e.instant)
                {
                    fader.color = endColor;
                    e.OnComplete?.Invoke();
                    return;
                }

                currentActionRoutine = CoroutineUtils.DoInterpolation(e.duration, f =>
                {
                    fader.color = Color.Lerp(startColor, endColor, Mathfx.Hermite(0f, 1f, f));
                    return true;
                }, () =>
                {
                    fader.gameObject.SetActive(false);
                    e.OnComplete?.Invoke();
                });
                break;
        }
    }
}
