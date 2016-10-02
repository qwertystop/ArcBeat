#undef UNITTESTING
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    // simple class holding properties of a beat - all timestamps plus directions
    public class BeatProps
    {
        private LinkedList<Beat.Direction> dirs = new LinkedList<Beat.Direction>();
        private LinkedList<int> timestamps = new LinkedList<int>(); // in ms

        // every time a timestamp is added, match it with a random direction
        public void addTimestamp(int tst) {
#if UNITTESTING
            // these two lines for when testing - no UnityEngine to work from
            System.Random testrandom = new System.Random();
            dirs.AddLast((Beat.Direction)testrandom.Next(0, 4));
#else
            // this line when running the actual game - the UnityEngine random is thread-safe
            dirs.AddLast((Beat.Direction)UnityEngine.Random.Range(0, 4));
#endif
            timestamps.AddLast(tst);
        }

        public Boolean notEmpty {
            get { return timestamps.Count != 0; }
        }

        public int timestamp { get { return timestamps.First.Value; } }

        public Beat.Direction direction { get { return dirs.First.Value; } }

        // drop the first element of both lists
        public BeatProps pop() {
            dirs.RemoveFirst();
            timestamps.RemoveFirst();
            return this;
        }
    }
}
