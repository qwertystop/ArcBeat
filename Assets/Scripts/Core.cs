using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Core : MonoBehaviour {
    public Player[] players;
    public AudioSource audioSource;
    public LinkedList<Utilities.BeatProps> beats;
    private LinkedList<Utilities.BeatProps> earlyBeats = new LinkedList<Utilities.BeatProps>();
    public Text[] scoreDisps = new Text[9];
    public Text notice;

    private bool musicStarted = false;

    private static double offset;
    public static double dspWithOffset {get { return AudioSettings.dspTime - offset; } }

	// Use this for initialization
	void Start () {
        beats = Utilities.Preprocessor.choreograph(Utilities.Globals.timestamps);
        audioSource.clip = Utilities.Globals.audioClip;
        Utilities.Globals.audioSource = audioSource; // for outside access
        // beats that need to be made before the music starts to arrive on time
        while (beats.Count != 0 && beats.First.Value.timestamp - Utilities.Globals.leadTimeMs < (-Utilities.Globals.time))
        {
            earlyBeats.AddLast(beats.First.Value);
            beats.RemoveFirst();
        }

        StartCoroutine(makeEarlyBeats());
        StartCoroutine(waitForMusic());
        enabled = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (!musicStarted)
        { // until the music starts use the default clock
            Utilities.Globals.time += (int)(Time.deltaTime * 1000);
        } // then stop keeping a separate time variable

        for (int i = 0; i < 4; i++)
        {// the four basic scores
            scoreDisps[i].text = Utilities.Globals.scores[i].ToString();
        }
        // now the sums
        // between-player totals
        int hittot = Utilities.Globals.scores[0] + Utilities.Globals.scores[1];
        int misstot = Utilities.Globals.scores[2] + Utilities.Globals.scores[3];
        int total = hittot - misstot;
        scoreDisps[4].text = hittot.ToString();
        scoreDisps[5].text = misstot.ToString();
        // hits / total
        scoreDisps[6].text = Utilities.Globals.scores[0].ToString() + " / " + (Utilities.Globals.scores[0] - Utilities.Globals.scores[2]).ToString();
        scoreDisps[7].text = Utilities.Globals.scores[1].ToString() + " / " + (Utilities.Globals.scores[1] - Utilities.Globals.scores[3]).ToString();
        scoreDisps[8].text = hittot + " / " + total;

        // set central text
        if (GameObject.FindGameObjectWithTag("beat") == null)
        {// if there are no beats
            if (beats.Count == 0)
            {// if there are no more to come
                notice.text = "( end )";
            } else
            {// there are more beats to come but there's a short pause
                notice.text = "( break )";
            }
        } else
        {
            notice.text = "";
        }
    }

    private System.Collections.IEnumerator waitForMusic() {
        while (Utilities.Globals.time < 0) { yield return 0; }
        audioSource.Play();
        offset = AudioSettings.dspTime; // doesn't start at 0 for some reason?
        musicStarted = true;
        StartCoroutine(makeBeats());
    }

    private System.Collections.IEnumerator makeBeats() {
        int which;
        while (beats.Count != 0)
        {
            // wait until it's time for the next beat to show up
            while (dspWithOffset < (beats.First.Value.timestamp - Utilities.Globals.leadTimeMs) / 1000.0) { yield return 0; }
            // then instantiate it over random player and remove it from the list
            which = Random.Range(0, 2);
            players[which].makeBeat(beats.First.Value, which);
            beats.RemoveFirst();
            yield return 0;
        }
    }

    private System.Collections.IEnumerator makeEarlyBeats() {
        int which;
        while (earlyBeats.Count != 0)
        {
            // wait until it's time for the next beat to show up
            while (Utilities.Globals.time < (earlyBeats.First.Value.timestamp - Utilities.Globals.leadTimeMs)) { yield return 0; }
            // then instantiate it over random player and remove it from the list
            which = Random.Range(0, 2);
            players[which].makeBeat(earlyBeats.First.Value, which);
            earlyBeats.RemoveFirst();
            yield return 0;
        }
    }
}
