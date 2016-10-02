using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utilities {
    // singleton holding global variables
    public sealed class Globals {
        // create singleton and lock it as a singleton
        private Globals() { }
        private static readonly Globals instance = new Globals();

        // player i on autopilot?
        private bool[] _autos = new bool[2];
        public static bool[] autos { get { return instance._autos; } }

        // elapsed time since start
        private int _totTime;
        public static int time
        {
            get { return instance._totTime; }
            set { instance._totTime = value; }
        }

        // audio clip
        private AudioClip _clip;
        public static AudioClip audioClip
        {
            get { return instance._clip; }
            set { instance._clip = value; }
        }

        // audio source
        private AudioSource _source;
        public static AudioSource audioSource
        {
            get { return instance._source; }
            set { instance._source = value; }
        }

        // locations of all targets
        private Vector3[,] _tLocs = { { new Vector3(-8, -4), new Vector3(-6, -4), new Vector3(-4, -4), new Vector3(-2, -4) },
                                     { new Vector3(2, -4), new Vector3(4, -4), new Vector3(6, -4), new Vector3(8, -4) }};
        public static Vector3[,] targetLocations
        {
            get { return instance._tLocs; }
        }

        // half of the delay between beat appearance and beat landing, in seconds, as a float
        // this is needed more frequently than the full time in ms as an int, so save it this way
        // TODO is it really? Recheck this.
        private float _hlt = 1.25f; // defaults for testing purposes
        public static float halfLeadTimeSec
        {
            get { return instance._hlt; }
        }
        // getter property for full lead time as int in ms
        public static int leadTimeMs
        {
            set { instance._hlt = value / 2000f; }
            get { return (int)(instance._hlt * 2000); }
        }

        // permitted leeway for accurate hits, in vertical distance
        private float _lw = 1; // default for testing purposes
        public static float leeway
        {
            get { return instance._lw; }
            set { instance._lw = value; }
        }

        // list of all timestamps in the game
        private List<int> _tss = Preprocessor.makeMetronome(20, 500, 2500); // default value is metronome
        public static List<int> timestamps
        {
            get { return instance._tss; }
            set { instance._tss = value; }
        }

        // scores: p1hit, p2hit, p1miss, p2miss,
        private int[] _scr = { 0, 0, 0, 0};
        public static int[] scores { get { return instance._scr; } }
    }
}
