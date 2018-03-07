using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VoiceCommands : MonoBehaviour
{
    KeywordRecognizer keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    public Rigidbody prefab;
    public float speed = 10.0f;

    // Use this for initialization
    void Start () {
        keywords.Add("shoot", () => {
            ThrowNewBall();
        });
        keywords.Add("throw", () => {
            ThrowNewBall();
        });
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction)) {
            keywordAction.Invoke();
        }
    }

    private void ThrowNewBall()
    {
        var ball = (Rigidbody)Instantiate(prefab, transform.position, transform.rotation);
        ball.velocity = (transform.forward + transform.up * 0.8f) * speed;
    }

}
