using UnityEngine;
using System.Collections;

public class Beat : MonoBehaviour {
    public Sprite[] arrows = new Sprite[4]; // all arrow sprites
    public Utilities.BeatProps allProps;// all timestamps and directions for the life of this beat
    public int curTStamp;
    private Direction curDir;
    public int curPlayer;
    // to stop multi-hits
    private bool hittable = true;

    private string dirName
    {
        get
        {
            switch (curDir)
            {
                case Direction.LEFT:
                    return curPlayer == 0 ? "p1left" : "p2left";
                case Direction.DOWN:
                    return curPlayer == 0 ? "p1down" : "p2down";
                case Direction.UP:
                    return curPlayer == 0 ? "p1up" : "p2up";
                case Direction.RIGHT:
                    return curPlayer == 0 ? "p1right" : "p2right";
                default:
                    return "";
            }
        }
    }
    private string modifierKey
    {
        get { return curPlayer == 0 ? "p1self" : "p2self"; }
    }
    private Color color
    {
        get
        {
            switch (curDir)
            {
                case Direction.UNSET:
                    return Color.clear;
                case Direction.LEFT:
                    return Color.red;
                case Direction.DOWN:
                    return new Color(0.2f, 1, 0.2f, 1);
                case Direction.UP:
                    return Color.blue;
                case Direction.RIGHT:
                    return Color.yellow;
                default:
                    throw new System.ArgumentException("Direction does not exist");
            }
        }
    }



    // Use this for initialization
    void Start () {
        // can't be initialized here, data needs to be passed in from creator
        // except for the particle effect and tag
        GetComponent<ParticleSystem>().startLifetime = Utilities.Globals.halfLeadTimeSec * 2;
        gameObject.tag = "beat";
	}

    // initialization after manual field-setting by creator
    public void setUp() {
        curTStamp = allProps.timestamp;
        curDir = allProps.direction;
        allProps.pop();
        GetComponent<SpriteRenderer>().sprite = arrows[(int)curDir];// set sprite
        GetComponent<ParticleSystem>().startColor = color;
        // now set new path
        iTween.MoveBy(this.gameObject, iTween.Hash("y", -15,
                                           "space", Space.World,
                                           "time", Utilities.Globals.halfLeadTimeSec * 3,
                                           "easetype", iTween.EaseType.linear));
    }

    // Update is called once per frame
    void Update() {
        // check if hit
        if (hittable &&
            // if autoplay is on for this side
            ((Utilities.Globals.autos[curPlayer] && Mathf.Abs(GetComponentInParent<Transform>().transform.position.y + 4.0f) < 0.3) ||
            // if it isn't
            (Input.GetButtonDown(dirName) && Mathf.Abs(GetComponentInParent<Transform>().transform.position.y + 4.0f) < Utilities.Globals.leeway)))
        {
            hit(Input.GetButton(modifierKey));
            Utilities.Globals.scores[curPlayer] += 1;
        }
    }

    // do this when this beat is hit
    public void hit(bool toSelf) {
        // change to next location
        if (allProps.notEmpty)
        {
            iTween.Stop(gameObject); // stop current motion
            curTStamp = allProps.timestamp;
            curDir = allProps.direction;
            allProps.pop();
            GetComponent<SpriteRenderer>().sprite = arrows[(int)curDir];// set sprite
            GetComponent<ParticleSystem>().startColor = color;
            if (!toSelf)
            { // going to other player
                curPlayer = curPlayer ^ 1; // flip player by bitwise XOR
            }
            tweenToTarget();
            // stop multi-hits
            StartCoroutine(tempUnhittable());
        } else
        {// no bounces
            GetComponentInParent<SpriteRenderer>().sprite = null;
            GetComponentInParent<ParticleSystem>().Stop();
            iTween.Stop(gameObject);
            // to allow the particle effect to fade
            StartCoroutine(destroyAfterDelay());
        }
    }

    // move to a different layer temporarially to avoid multi-hits.
    IEnumerator tempUnhittable() {
        hittable = false;
        double endTime = (((curTStamp / 1000f) - Core.dspWithOffset) / 2f + Core.dspWithOffset);
        while (Core.dspWithOffset < endTime) { yield return 0; }
        hittable = true;
        yield return 0;
    }

    IEnumerator destroyAfterDelay() {
        hittable = false;
        double endTime = (Core.dspWithOffset + Utilities.Globals.halfLeadTimeSec * 2);
        while(Core.dspWithOffset < endTime) { yield return 0; }
        Destroy(this.gameObject);
        yield return 0;
    }

    private void tweenToTarget() {
        // now that the new target has been set, start moving there
        float halfTimeUntil = (float)((curTStamp / 1000f) - Core.dspWithOffset) / 2f;
        iTween.MoveBy(this.gameObject, iTween.Hash("y", 9,
                                           "space", Space.World,
                                           "time", halfTimeUntil,
                                           "easetype", iTween.EaseType.easeOutQuad));
        iTween.MoveBy(this.gameObject, iTween.Hash("y", -9,
                                           "space", Space.World,
                                           "time", halfTimeUntil,
                                           "delay", halfTimeUntil,
                                           "easetype", iTween.EaseType.easeInQuad));
        iTween.MoveBy(gameObject, iTween.Hash("y", -7,
            "Space", Space.World,
            "time", halfTimeUntil,
            "delay", halfTimeUntil * 2,
            "easetype", iTween.EaseType.easeOutQuad));
        iTween.MoveAdd(gameObject, iTween.Hash("x", Utilities.Globals.targetLocations[curPlayer, (int)curDir].x - gameObject.transform.position.x,
                                           "space", Space.World,
                                           "time", halfTimeUntil * 2,
                                           "easetype", iTween.EaseType.linear));
    }

    public enum Direction {
        // relative to the player, for a non-UNSET direction dir,
        // the matching target is at (int)dir * 2 - 3
        UNSET = -1,
        LEFT = 0,
        DOWN = 1,
        UP = 2,
        RIGHT = 3
    }
}
