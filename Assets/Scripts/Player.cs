using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public Beat beat;

	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    void Update() {

    }

    // make a Beat from here, from the given BeatProps
    public void makeBeat(Utilities.BeatProps bp, int which) {
        Vector3 nbLoc = new Vector3(this.transform.position.x + (int)bp.direction * 2 - 3, this.transform.position.y);
        Beat nb = (Beat)Instantiate(beat, nbLoc, Quaternion.identity);
        nb.allProps = bp;
        nb.curPlayer = which;
        nb.setUp();
    }
}
