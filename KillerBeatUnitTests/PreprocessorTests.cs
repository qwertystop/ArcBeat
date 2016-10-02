using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;
using System.Collections.Generic;

namespace KillerBeatUnitTests {
    [TestClass]
    public class PreprocessorTests {
        [TestMethod]
        public void TestMetronome() {
            // arrange
            List<int> expected = new List<int>() {5000, 6000, 7000, 8000, 9000, 10000, 11000, 12000, 13000, 14000,
                                                15000, 16000, 17000, 18000, 19000, 20000, 21000, 22000, 23000, 24000 };

            // act
            List<int> actual = Preprocessor.makeMetronome(20, 1000, 5000);

            // assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestChoreograph() {
            // arrange
            LinkedList<BeatProps> expected = new LinkedList<BeatProps>();
            expected.AddLast(new BeatProps());
            expected.Last.Value.addTimestamp(5000);
            expected.Last.Value.addTimestamp(10000);
            expected.Last.Value.addTimestamp(15000);
            expected.Last.Value.addTimestamp(20000);
            expected.AddLast(new BeatProps());
            expected.Last.Value.addTimestamp(6000);
            expected.Last.Value.addTimestamp(11000);
            expected.Last.Value.addTimestamp(16000);
            expected.Last.Value.addTimestamp(21000);
            expected.AddLast(new BeatProps());
            expected.Last.Value.addTimestamp(7000);
            expected.Last.Value.addTimestamp(12000);
            expected.Last.Value.addTimestamp(17000);
            expected.Last.Value.addTimestamp(22000);
            expected.AddLast(new BeatProps());
            expected.Last.Value.addTimestamp(8000);
            expected.Last.Value.addTimestamp(13000);
            expected.Last.Value.addTimestamp(18000);
            expected.Last.Value.addTimestamp(23000);
            expected.AddLast(new BeatProps());
            expected.Last.Value.addTimestamp(9000);
            expected.Last.Value.addTimestamp(14000);
            expected.Last.Value.addTimestamp(19000);
            expected.Last.Value.addTimestamp(24000);

            // act
            LinkedList<BeatProps> actual = Preprocessor.choreograph(Preprocessor.makeMetronome(20, 1000, 5000));

            // assert
            Assert.AreEqual(expected.Count, actual.Count);
            while (expected.Count != 0 && actual.Count != 0)
            {
                BeatProps e = expected.First.Value;
                BeatProps a = actual.First.Value;
                while (a.notEmpty && e.notEmpty)
                {
                    Assert.AreEqual(e.timestamp, a.timestamp);
                    e.pop();
                    a.pop();
                }
                expected.RemoveFirst();
                actual.RemoveFirst();
            }
        }
    }
}
