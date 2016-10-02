using UnityEngine;
using System.Collections;

// if a missed beat has more to do, relaunch it from the bottom, else destroy it
// either way play the miss effect
public class Cleanup : MonoBehaviour {
    public ParticleSystem missflair;

    void Start() {

    }

    void OnTriggerEnter2D(Collider2D other) {
        Beat b = other.GetComponentInParent<Beat>();
        // add a miss
        Utilities.Globals.scores[b.curPlayer + 2] -= 1;
        if (b.allProps.notEmpty)
        {// relaunch the beat from just below the bottom-center
            other.gameObject.transform.position = new Vector3(0, -6);
            // to the player who missed it
            b.hit(true);
            missflair.Play();
        } else
        {// nothing left, get rid of the shell
            Destroy(other.gameObject);
            missflair.Play();
        }
    }
}
