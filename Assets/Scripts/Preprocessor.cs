#undef UNITTESTING
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    // preprocessor utilities
    public static class Preprocessor {
        // returns audio file
        public static FileInfo processOsuFile(FileInfo osufile, Text loadingText) {
            loadingText.text = "Reading .osu file . . .";
            sections section = sections.UNKNOWN; // section of the .osu file
            FileInfo audiofile = null; // the audio file referenced by the .osu file
            int audioLead = 0;
            // data used in calculating timestamps
            float sliderMultiplier = 1.4f;// default
            List<KeyValuePair<int, float>> beatLengthsByTimes = new List<KeyValuePair<int, float>>();
            List<string> hitObjects = new List<string>();
            List<int> hitObjectTimes;
            // read file
            foreach (string line in File.ReadAllLines(osufile.FullName))
            {
                if (line.StartsWith("[")) // section header, change section
                {
                    switch (line)
                    {
                        case "[General]":
                            section = sections.GENERAL;
                            break;
                        case "[Difficulty]":
                            section = sections.DIFFICULTY;
                            break;
                        case "[TimingPoints]":
                            section = sections.TIMING_POINTS;
                            break;
                        case "[HitObjects]":
                            section = sections.HIT_OBJECTS;
                            loadingText.text = "Reading hitObject times . . ."; // this persists for the whole section
                            break;
                        default:
                            section = sections.UNKNOWN;
                            break;
                    }
                } else if (line != "") // not a section header, what to look for depends on section
                {// but ignore the blank lines, of course
                    switch (section)
                    {
                        case sections.GENERAL:
                            if (line.StartsWith("AudioFilename: "))
                            {
                                loadingText.text = "Reading audio file location . . .";
                                string loc = osufile.DirectoryName + "/" + line.Substring(15);
                                FileInfo[] files = osufile.Directory.GetFiles(loc);
                                if (1 != files.Length)
                                {
                                    Debug.LogError(".osu file does not unambiguously name a single present audio file");
                                }
                                audiofile = new FileInfo(loc);
                            } else if (line.StartsWith("AudioLeadIn: "))
                            {
                                loadingText.text = "Reading audio leadin";
                                string lead = line.Substring(13);
                                audioLead = Int32.Parse(lead);
                            }
                            break;
                        case sections.DIFFICULTY:
                            if (line.StartsWith("SliderMultiplier:"))
                            {
                                sliderMultiplier = float.Parse(line.Substring(17));
                            }
                            break;
                        case sections.TIMING_POINTS:
                            string[] parts = line.Split(',');
                            int time = (int)double.Parse(parts[0]);// spec says int, but program can produce non-int, so need to be able to read double
                            float msPerBeat = float.Parse(parts[1]);
                            if (msPerBeat < 0) // negative here means inherited timing point, which works differently
                            {// no need to ensure that it's not the first - if inherited is first the source file is broken anyway.
                             // take the written value, round down, divide by -100f, multiply by the msPerBeat of the previous point
                                beatLengthsByTimes.Add(
                                    new KeyValuePair<int, float>(time, ((int)msPerBeat / -100f) * beatLengthsByTimes.FindLast(x => true).Value));
                            } else
                            {// not inherited, much simpler
                                beatLengthsByTimes.Add(new KeyValuePair<int, float>(time, msPerBeat));
                            }
                            break;
                        case sections.HIT_OBJECTS:
                            // not changing text because it persists for the whole section
                            // add the hitObjects to the list, will process them for times after reading the whole file
                            hitObjects.Add(line);
                            break;
                        default: // same as case 0
                            loadingText.text = "Reading .osu file . . .";
                            break;
                    }
                }
            }
            loadingText.text = "";
            // file read, work with data
            hitObjectTimes = getTimesFromHitObjects(hitObjects, sliderMultiplier, beatLengthsByTimes);
            Globals.time = -Globals.leadTimeMs - audioLead;
            Globals.timestamps = hitObjectTimes;
            return audiofile;
        }

        private static List<int> getTimesFromHitObjects(List<string> lines, float sliderMultiplier, List<KeyValuePair<int, float>> beatLengthsByTimes) {
            List<int> times = new List<int>();
            // must be set before looping so it doesn't reset, since value may need to carry between loops
            float beatLength = 1; // depends on current timing section but we need a default
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                // no matter what, if the file is valid, it has at least one time
                int lastObjectTime = Int32.Parse(parts[2]);
                times.Add(lastObjectTime);
                if ((Int32.Parse(parts[3]) & 2) != 0) // that field is a bitmap for hitObject type;
                {// only sliders have more times since I'm not modeling spinners 
                 // get the simple values ready first
                    int numRepeats = Int32.Parse(parts[6]);
                    int pixelLength = Int32.Parse(parts[7]);

                    // determine what timing point we're in for current beatLength
                    beatLengthsByTimes.Sort((x, y) => x.Key - y.Key); // make sure it's in order, because the spec doesn't technically guarantee it
                    while (beatLengthsByTimes.Count != 0 && beatLengthsByTimes[0].Key < lastObjectTime)
                    {// either in this timing section or a later one
                        beatLength = beatLengthsByTimes[0].Value; // so set the current beatLength to that,
                        beatLengthsByTimes.RemoveAt(0);// remove the timing point from the list, and repeat as necessary
                    }
                    //TODO pretty sure duration is wrong
                    float sliderTime = (int)(beatLength * (pixelLength / sliderMultiplier) / 100f); // duration of one iteration of slider

                    // now add the times for each repetition of the slider
                    int startTime = lastObjectTime + (int)sliderTime;
                    List<int> s_times = Utilities.Preprocessor.makeMetronome(numRepeats, (int)sliderTime, startTime);
                    times.AddRange(s_times);
                }
            }
            return times;
        }
        

        /* given:
            the number of ticks
            the interval between ticks (in ms)
            the starting time
        produce a list of timestamps for beats on those ticks
        */
        public static List<int> makeMetronome(int length, int interval, int start) {
            List<int> ts = new List<int>(length);
            for (int i = 0; i < length; ++i)
            {
                ts.Add((i * interval) + start);
            }
            return ts;
        }

        /* given:
            a sorted (increasing order) array of unique timestamps for beats (in ms)
        produce a LinkedList of BeatProps, each one timestamped for bounces as far as possible
        while maintaining that each landing is within +/- 10% of Globals.leadTimeInt
        */
        public static LinkedList<BeatProps> choreograph(List<int> timestamps) {
            LinkedList<BeatProps> bts = new LinkedList<BeatProps>();
            int minlead = (int)(Globals.leadTimeMs * 0.9);
            int maxlead = (int)(Globals.leadTimeMs * 1.1);

            // as long as there are unassigned timestamps
            while (timestamps.Count != 0)
            {
                // add the first timestamp to the list
                // this will be the first timestamp of the new beat
                int curTime = timestamps[0];
                bts.AddLast(new BeatProps());
                bts.Last.Value.addTimestamp(curTime);
                timestamps.RemoveAt(0);

                // now start figuring out the later timestamps of that beat
                List<int> potentials;
                // as long as there are more unassigned timestamps in range
                while ((potentials = timestamps.FindAll(x => (x >= curTime + minlead) && (x <= curTime + maxlead))).Count != 0)
                {// add one to the list for that beat at random
#if UNITTESTING
                    // these two lines for when testing - no UnityEngine to work from
                    System.Random testingrand = new System.Random();
                    int nextIndex = testingrand.Next(0, potentials.Count);
#else
                    // this line when running the actual game - the UnityEngine random is thread-safe
                    int nextIndex = UnityEngine.Random.Range(0, potentials.Count);
#endif
                    int nextTime = potentials[nextIndex];
                    bts.Last.Value.addTimestamp(nextTime);
                    // remove it from the unassigned times
                    timestamps.Remove(nextTime);
                    // and work from there to find the next one
                    curTime = nextTime;
                }
                // then move on to the first timestamp still unassigned for a new beat
            }
            return bts;
        }
    }

    enum sections {
        UNKNOWN,
        GENERAL,
        DIFFICULTY,
        TIMING_POINTS,
        HIT_OBJECTS
    }
}