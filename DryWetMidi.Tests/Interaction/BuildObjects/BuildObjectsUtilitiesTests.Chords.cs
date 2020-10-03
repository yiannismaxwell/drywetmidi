using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class BuildObjectsUtilitiesTests
    {
        #region Test methods

        [Test]
        public void BuildChords_FromNotes_SingleNote()
        {
            CheckBuildingChords(
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50)),
                });
        }

        [Test]
        public void BuildChords_FromNotes_MultipleNotes_SameTime()
        {
            CheckBuildingChords(
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50),
                    new Note((SevenBitNumber)70),
                    new Note((SevenBitNumber)90, 100, 0),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50),
                        new Note((SevenBitNumber)70),
                        new Note((SevenBitNumber)90, 100, 0)),
                });
        }

        [Test]
        public void BuildChords_FromNotes_MultipleNotes_ExceedingNotesTolerance()
        {
            CheckBuildingChords(
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50),
                    new Note((SevenBitNumber)70),
                    new Note((SevenBitNumber)90, 100, 20),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50),
                        new Note((SevenBitNumber)70)),
                    new Chord(
                        new Note((SevenBitNumber)90, 100, 20)),
                });
        }

        [Test]
        public void BuildChords_FromNotes_MultipleNotes_DifferentChannels()
        {
            CheckBuildingChords(
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50),
                    new Note((SevenBitNumber)70),
                    new Note((SevenBitNumber)90) { Channel = (FourBitNumber)1 },
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50),
                        new Note((SevenBitNumber)70)),
                    new Chord(
                        new Note((SevenBitNumber)90) { Channel = (FourBitNumber)1 }),
                });
        }

        #endregion

        #region Private methods

        private void CheckBuildingChords(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects,
            long notesTolerance = 0)
        {
            CheckObjectsBuilding(
                inputObjects,
                outputObjects,
                new ObjectsBuildingSettings
                {
                    BuildChords = true,
                    ChordBuilderSettings = new ChordBuilderSettings
                    {
                        NotesTolerance = notesTolerance
                    }
                });
        }

        #endregion
    }
}
