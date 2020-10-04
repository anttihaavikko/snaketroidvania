using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubble : MonoBehaviour {

	public TMP_Text textArea;
    public Vector3 hiddenSize;
    public Transform followTarget;
    public int soundOffset;

    private bool shown;
	private string message = "";
	private int messagePos = -1;
    private bool hidesWithAny = false;
    private Vector3 shownSize;

    public bool done = false;

	private AudioSource audioSource;

	private List<string> messageQue;

	public Color hiliteColor;
    string hiliteColorHex;

    bool useColors = true;
    private bool canSkip = false;

    private string[] options;
    private string[] optionActions;
    private int optionSelection;


    // Use this for initialization
    void Start () {
		audioSource = GetComponent<AudioSource> ();

		messageQue = new List<string> ();

        shownSize = transform.localScale;
        transform.localScale = hiddenSize;

        Invoke("EnableSkip", 0.25f);

        SetColor(hiliteColor);
    }

    void EnableSkip()
    {
        canSkip = true;
    }

    void SkipOrPopOrHide()
    {

    }

    // Update is called once per frame
    void Update () {

        if(shown && followTarget)
        {
            transform.position = followTarget.position + Vector3.right;
        }

        if (canSkip)
        {
            if (Input.anyKeyDown)
            {
                if (!done)
                {
                    SkipMessage();
                }
                else
                {
                    CheckQueuedMessages();
                }
            }
        }

        if (done) return;

		if (Random.value < 0.8f) {
			return;
		}

		if (messagePos >= 0 && !done) {
			messagePos++;

            if (messagePos > message.Length) return;

			string msg = message.Substring (0, messagePos);

			int openCount = msg.Split('(').Length - 1;
			int closeCount = msg.Split(')').Length - 1;

            if (openCount > closeCount && useColors) {
				msg += ")";
			}

			string letter = message.Substring (messagePos - 1, 1);

            if(letter == "<")
            {
                messagePos = message.IndexOf(">", messagePos, System.StringComparison.Ordinal) + 1;
                msg = message.Substring(0, messagePos);
            }

            SetText(msg);

            if (messagePos == 1 || letter == " " || letter == "." || letter == "!") {
                AudioManager.Instance.PlayEffectAt(14, transform.position, 1.325f);
                AudioManager.Instance.PlayEffectAt(15, transform.position, 1.245f);
                AudioManager.Instance.PlayEffectAt(16, transform.position, 1.545f);
            }

            if (messagePos >= message.Length) {
				messagePos = -1;

				done = true;
			}
		}
	}

    void SetText(string msg)
    {
        textArea.text = useColors ? msg.Replace("(", "<color=" + hiliteColorHex + ">").Replace(")", "</color>") : msg;
    }

	public int QueCount() {
		return messageQue.Count;
	}

	public void SkipMessage() {
		done = true;
		messagePos = -1;
        SetText(message);
	}

    public void Show()
    {
        Tweener.Instance.ScaleTo(transform, shownSize, 0.6f, 0f, TweenEasings.BounceEaseOut);
        shown = true;
        gameObject.SetActive(true);

        AudioManager.Instance.Highpass();
    }

    public void ShowMessage(string str, bool colors = true) {
        hidesWithAny = false;
        canSkip = false;
        Invoke("EnableSkip", 0.25f);

        Show();

        useColors = colors;

        done = false;
		message = str;
		textArea.text = "";

		Invoke ("ShowText", 0.4f);
    }

	public void QueMessage(string str) {
		messageQue.Add (str);
	}

	public void CheckQueuedMessages() {
		if (messageQue.Count > 0) {
			PopMessage ();
		}
        else
        {
            Hide();
        }
	}

	private void PopMessage() {
		string msg = messageQue [0];
		messageQue.RemoveAt (0);
		ShowMessage (msg);
	}

	private void ShowText() {
		messagePos = 0;
        CancelInvoke("DisableAfterDelay");
        gameObject.SetActive(true);
    }

	public void HideAfter (float delay) {
		Invoke ("Hide", delay);
	}

	public void Hide() {

        SkipMessage();

        //done = true;
        //messagePos = -1;

        Tweener.Instance.ScaleTo(transform, hiddenSize, 0.15f, 0f, TweenEasings.QuadraticEaseOut);
        Invoke("DisableAfterDelay", 0.3f);

        AudioManager.Instance.Highpass(false);

        shown = false;
		//textArea.text = "";
	}

    void DisableAfterDelay()
    {
        gameObject.SetActive(false);
    }

	public bool IsShown() {
		return shown;
	}

	public void SetColor(Color color) {
        hiliteColorHex = "#" + ColorUtility.ToHtmlStringRGB (color);
	}
}
