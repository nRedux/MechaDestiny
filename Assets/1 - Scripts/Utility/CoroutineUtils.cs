using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoroutineUtils : Singleton<CoroutineUtils> {

	public delegate bool Interpolator(float f);

    public event System.Action OnLateUpdate;
    public event System.Action OnEndOfFrame;

    static CoroutineUtils()
    {
        DoInstantiate = HandleInstatiate;
    }

    public void Update()
    {
        LuaBehaviorManager.Instance.Update();
    }

    public void LateUpdate()
    {
        OnLateUpdate?.Invoke();
        CoroutineUtils.DoWaitForEndOfFrame( () => { OnEndOfFrame?.Invoke(); } );
    }

    public static Coroutine BeginCoroutine(IEnumerator routine)
	{
		return CoroutineUtils.Instance.StartCoroutine(routine);
	}

	public static void EndCoroutine(ref Coroutine routine)
	{
        if (routine == null)
            return;
		CoroutineUtils.Instance.StopCoroutine(routine);
		//Be nice and set to null - I find myself setting them to null after the fact anyway
		routine = null;
	}

    private static void HandleInstatiate()
    {
        GameObject instance = new GameObject("COROUTINE_UTILS");
        instance.AddComponent<CoroutineUtils>();
        DontDestroyOnLoad(instance);
    }

    public static Coroutine DoCanvasGroupFade(CanvasGroup group, float targetAlpha, float duration, System.Action onFinished = null, float delay = 0f)
    {
        if (group == null)
            return null;
        float startAlpha = group.alpha;
        return BeginCoroutine(DoInterpolationRoutine(duration, f =>
        {
            if (group == null)
                return false;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, f);
            return true;
        }, () =>
        {
	        if(group != null)
				group.alpha = targetAlpha;
            onFinished?.Invoke();
        }, 
        delay));
    }

    public static Coroutine FadeOutCanvasGroup( CanvasGroup group, float duration)
    {
        if( group == null )
            return null;
        float startAlpha = group.alpha;
        return BeginCoroutine( DoInterpolationRoutine( duration, f =>
        {
            if( group == null )
                return false;
            group.alpha = Mathf.Lerp( startAlpha, 0f, f );
            return true;
        }, () =>
        {
            if( group != null )
                group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        },
        0f) );
    }

    public static Coroutine DoInterpolation(float duration, Interpolator step, System.Action onFinished = null, float delay = 0f)
    {
        return BeginCoroutine(DoInterpolationRoutine(duration, step, onFinished, delay));
    }

    public static IEnumerator DoInterpolationRoutine(float duration, Interpolator step, System.Action onFinished = null, float delay = 0f)
	{
		if( step == null)
		{
			Debug.LogError("step is null.");
			yield break;
		}
        yield return new WaitForSeconds(delay);

		float time = 0f;
		while( !Mathf.Approximately(duration, time))
		{
			time = Mathf.Clamp(time + Time.deltaTime, 0f, duration);
			float t = time / duration;
			if(!step(t))
				yield break;
			yield return null;
		}

		if(onFinished != null)
		{
			onFinished();
		}
	}


	public class WaitInterpolation: CustomYieldInstruction
	{
		float time = 0f;
		float duration = 0f;
		Interpolator step;
		float start, end;

		public WaitInterpolation(float duration, float start, float end, Interpolator step)
		{
			this.start = start;
			this.end = end;
			this.duration = duration;
			this.step = step;
		}

		public override bool keepWaiting
		{
			get
			{
				time = Mathf.Clamp(time + Time.deltaTime, 0f, duration);
				float t = time / duration;
				if(!step(Mathfx.Hermite(start, end, t)))
					return false;
				return !Mathf.Approximately(duration, time);
			}
		}
	}

    public static Coroutine DoInterpolation(float duration, float start, float end, Interpolator step, System.Func<float, float, float, float> interpolator = null, System.Action onComplete = null, float delay = 0f)
    {
        
        return BeginCoroutine(DoInterpolationRoutine(duration, start, end, step, interpolator, onComplete, delay));
    }

	public static IEnumerator DoInterpolationRoutine(float duration, float start, float end, Interpolator step, System.Func<float, float, float, float> interpolator = null, System.Action onComplete = null, float delay = 0f)
	{
		if(step == null)
		{
			Debug.LogError("step is null.");
			yield break;
		}
        yield return new WaitForSeconds(delay);

		float time = 0f;
		while(!Mathf.Approximately(duration, time))
		{
			time = Mathf.Clamp(time + Time.deltaTime, 0f, duration);
			float t = time / duration;
            if (interpolator == null)
                interpolator = Mathf.Lerp;
			if(!step(interpolator(start, end, t)))
				yield break;
			yield return null;
		}

        if (onComplete != null)
            onComplete();
	}


    public static Coroutine DoWaitUntil(System.Func<bool> predicate, System.Action onWaitEnd)
    {
        return BeginCoroutine(WaitUntilRoutine(predicate, onWaitEnd));
    }

    public static IEnumerator WaitUntilRoutine(System.Func<bool> predicate, System.Action onWaitEnd)
    {
        yield return new WaitUntil(predicate);
        onWaitEnd();
    }

    public static Coroutine DoDelay(float delay, System.Action action)
    {
        return BeginCoroutine(DelayRoutine(delay, action));
    }

    public static Coroutine DoWaitForEndOfFrame( System.Action action)
    {
        return BeginCoroutine(EndOfFrameRoutine(action));
    }

    public static Coroutine DoAfter(System.Func<bool> waitCondition, System.Action action)
    {
        return BeginCoroutine(WaitRoutine(waitCondition, action));
    }

    public static IEnumerator DelayRoutine(float delay, System.Action action)
	{
		if(action == null)
			throw new System.ArgumentException("action argument can't be null");
		yield return new WaitForSeconds(delay);
		action();
	}

    public static IEnumerator EndOfFrameRoutine(System.Action action)
    {
        if (action == null)
            throw new System.ArgumentException("action argument can't be null");
        yield return new WaitForEndOfFrame();
        action();
    }


    public static IEnumerator WaitRoutine(System.Func<bool> waitCondition, System.Action action)
    {
        if (action == null)
            throw new System.ArgumentException("action argument can't be null");
        yield return new WaitWhile(waitCondition);
        action();
    }

    public static Coroutine DoDelayUnityFrameEnd( System.Action action, int frames = 1)
    {
        return BeginCoroutine(DelayUnityFrameEnd( action, frames));
    }


    public static IEnumerator DelayUnityFrameEnd( System.Action action, int count = 1)
	{
        if( count < 1 )
            throw new System.ArgumentException("Count must be greater than or equal to 1");
        if (action == null)
            throw new System.ArgumentException("action argument can't be null");
        for ( int i = 0; i < count; ++i)
        {
            yield return new WaitForEndOfFrame();
        }

		action();
	}

	public static IEnumerator WaitForSeconds(float seconds, bool interruptable)
	{
		float time = seconds;
		while( time > 0f)
		{
			time -= Time.deltaTime;
			if( interruptable && (Input.anyKey || Input.GetMouseButton(0) || Input.GetMouseButton(1)) )
			{
				yield break;
			}
			yield return null;
		}
	}

    /*
    public static Coroutine DoElementTransition(LayoutElement element, float duration, CardinalAxis axis, float targetSize, System.Action onComplete = null)
    {
        float startVal = 0f;
        switch(axis)
        {
            case CardinalAxis.Horizontal:
                startVal = element.preferredWidth;
                break;
            case CardinalAxis.Vertical:
                startVal = element.preferredHeight;
                break;
        }
        return DoInterpolation(duration, startVal, targetSize, (f) =>
        {
            switch (axis)
            {
                case CardinalAxis.Horizontal:
                    element.preferredWidth = f;
                    break;
                case CardinalAxis.Vertical:
                    element.preferredHeight = f;
                    break;
            }
            return true;
        }, Mathfx.Hermite, onComplete);
    }
    */

    public static Coroutine DoTransformPositionLerp(Transform t, Vector3 targetPos, float duration, System.Func<float, float, float, float> interpolator = null, System.Action onComplete = null, float delay = 0f)
    {
        Vector3 startPos = t.position;
        return DoInterpolation(duration, 0f, 1f, (f) =>
        {
            t.position = Vector3.Lerp(startPos, targetPos, f);
            return true;
        }, interpolator, onComplete, delay: delay);
    }

    public static Coroutine DoTransformLerp( Transform t, Transform targetPos, float duration, System.Func<float, float, float, float> interpolator = null, System.Action onComplete = null, float delay = 0f )
    {
        Vector3 offset = t.position - targetPos.position;

        Vector3 startPos = t.position;
        return DoInterpolation( duration, 0f, 1f, ( f ) =>
        {
            t.position = targetPos.position + Vector3.Lerp( offset, Vector3.zero, f );
            return true;
        }, interpolator, onComplete, delay: delay );
    }

}
