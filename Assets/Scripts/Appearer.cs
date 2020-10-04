using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Appearer : MonoBehaviour
{
	public float appearAfter = -1f;
	public float hideDelay;
    public bool silent;
    public bool hiddenOnWeb;
    public bool soundOnZero;
    public float volume = 0.6f;

    public TMP_Text text;
    private Vector3 size;

    // Start is called before the first frame update
    void Start()
    {
        size = transform.localScale;
        transform.localScale = Vector3.zero;

		if (appearAfter >= 0 && (!hiddenOnWeb || Application.platform != RuntimePlatform.WebGLPlayer))
			Invoke("Show", appearAfter);
    }

    private Vector3 SoundPos()
    {
        return soundOnZero ? Vector3.zero : transform.position;
    }

    public void Show(bool autoHide = false)
    {
        if(!silent)
        {
            var p = SoundPos();
            AudioManager.Instance.PlayEffectAt(1, transform.position, 1f * volume);
            AudioManager.Instance.PlayEffectAt(4, transform.position, 0.669f * volume);
            AudioManager.Instance.PlayEffectAt(9, transform.position, 0.506f * volume);
            AudioManager.Instance.PlayEffectAt(22, transform.position, 0.735f * volume);
        }

        // Debug.Log("Showing " + name);
        Tweener.Instance.ScaleTo(transform, size, 0.3f, 0f, TweenEasings.BounceEaseOut);

        if (autoHide)
            HideWithDelay();
    }

    public void Hide()
	{
        CancelInvoke("Show");

        // Debug.Log("Hiding " + name);

        if(!silent)
        {
            var p = SoundPos();
            AudioManager.Instance.PlayEffectAt(1, transform.position, 1f * volume);
            AudioManager.Instance.PlayEffectAt(4, transform.position, 0.669f * volume);
            AudioManager.Instance.PlayEffectAt(9, transform.position, 0.506f * volume);
            AudioManager.Instance.PlayEffectAt(22, transform.position, 0.735f * volume);
        }

        Tweener.Instance.ScaleTo(transform, Vector3.zero, 0.2f, 0f, TweenEasings.QuadraticEaseOut);

        Invoke("AfterHide", 0.2f);
	}

    void AfterHide()
    {
        gameObject.SetActive(false);
    }

    public void HideWithDelay()
	{
		Invoke("Hide", hideDelay);
	}

    public void ShowWithText(string t, float delay)
    {
        if (text)
            text.text = t;

        Invoke("Show", delay);
    }
}
