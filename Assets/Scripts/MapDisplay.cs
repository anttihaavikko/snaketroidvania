using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapDisplay : MonoBehaviour
{
    private Vector3 showPos, hidePos;
    private Vector3 showSize, hideSize;

    // Start is called before the first frame update
    void Start()
    {
        showPos = transform.position;
        showSize = Vector3.one;
        hidePos = showPos + Vector3.down * Screen.height * 0.8f;
        hideSize = new Vector3(0.2f, 0.8f, 1f);

        transform.position = hidePos;
        transform.localScale = hideSize;
    }

    public void Toggle(bool state)
    {
        var targetSize = state ? showSize : hideSize;
        var targetPos = state ? showPos : hidePos;

        Tweener.Instance.MoveTo(transform, targetPos, 0.3f, 0, TweenEasings.BounceEaseOut);
        Tweener.Instance.ScaleTo(transform, targetSize, 0.3f, 0, TweenEasings.BounceEaseOut);
    }
}
