using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class BuildObjectsUtilitiesTests
    {
        #region Nested classes

        private sealed class TimedObjectComparer : IComparer
        {
            #region IComparer

            public int Compare(object x, object y)
            {
                var timedObject1 = x as ITimedObject;
                var timedObject2 = y as ITimedObject;

                if (ReferenceEquals(timedObject1, timedObject2))
                    return 1;

                if (ReferenceEquals(timedObject1, null))
                    return -1;

                if (ReferenceEquals(timedObject2, null))
                    return 1;

                var timesDifference = timedObject1.Time - timedObject2.Time;
                if (timesDifference != 0)
                    return Math.Sign(timesDifference);

                return TimedObjectEquality.AreEqual(timedObject1, timedObject2, false) ? 0 : -1;
            }

            #endregion
        }

        #endregion

        #region Setup

        [OneTimeSetUp]
        public void SetUp()
        {
            TestContext.AddFormatter<ITimedObject>(obj =>
            {
                var timedObject = (ITimedObject)obj;
                var lengthedObject = obj as ILengthedObject;
                return lengthedObject != null
                    ? $"{obj} (T = {lengthedObject.Time}, L = {lengthedObject.Length})"
                    : $"{obj} (T = {timedObject.Time})";
            });
        }

        #endregion

        #region Test methods

        [Test]
        public void BuildObjects_TimedEventsAndNotes_Empty()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: Enumerable.Empty<ITimedObject>(),
                outputObjects: Enumerable.Empty<ITimedObject>());
        }

        [Test]
        public void BuildObjects_TimedEventsAndNotes_FromTimedEvents_Mixed()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new NoteOnEvent(), 20),
                    new TimedEvent(new NoteOffEvent(), 50),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note(SevenBitNumber.MinValue, 30, 20) { Velocity = SevenBitNumber.MinValue }
                });
        }

        [Test]
        public void BuildObjects_TimedEventsAndNotes_FromTimedEvents_OnlyNoteEvents()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent(), 20),
                    new TimedEvent(new NoteOffEvent(), 50),
                },
                outputObjects: new ITimedObject[]
                {
                    new Note(SevenBitNumber.MinValue, 30, 20) { Velocity = SevenBitNumber.MinValue }
                });
        }

        [Test]
        public void BuildObjects_TimedEventsAndNotes_FromTimedEvents_OnlyNonNoteEvents()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                });
        }

        [Test]
        public void BuildObjects_TimedEventsAndNotes_AllProcessed()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new SetTempoEvent(1234), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { Channel = (FourBitNumber)1 }, 10),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)70) { Channel = (FourBitNumber)1 }, 20),
                    new TimedEvent(new PitchBendEvent(123), 30),
                    new TimedEvent(new MarkerEvent("Marker"), 40),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 }, 40),
                    new TimedEvent(new MarkerEvent("Marker 2"), 50),
                    new TimedEvent(new TextEvent("Text"), 60),
                    new TimedEvent(new TextEvent("Text 2"), 70),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)1) { Channel = (FourBitNumber)10 }, 70),
                    new TimedEvent(new CuePointEvent("Point"), 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 }, 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)1, (SevenBitNumber)0) { Channel = (FourBitNumber)1 }, 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { Channel = (FourBitNumber)10 }, 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { Channel = (FourBitNumber)1 }, 100),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new SetTempoEvent(1234), 0),
                    new Note((SevenBitNumber)1, 80, 10) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)100 },
                    new Note((SevenBitNumber)2, 80, 20) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)70 },
                    new TimedEvent(new PitchBendEvent(123), 30),
                    new TimedEvent(new MarkerEvent("Marker"), 40),
                    new Note((SevenBitNumber)3, 40, 40) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)1, OffVelocity = (SevenBitNumber)1 },
                    new TimedEvent(new MarkerEvent("Marker 2"), 50),
                    new TimedEvent(new TextEvent("Text"), 60),
                    new TimedEvent(new TextEvent("Text 2"), 70),
                    new Note((SevenBitNumber)2, 20, 70) { Channel = (FourBitNumber)10, Velocity = (SevenBitNumber)1 },
                    new TimedEvent(new CuePointEvent("Point"), 80),
                });
        }

        [Test]
        public void BuildObjects_TimedEventsAndNotes_NotAllProcessed()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new SetTempoEvent(1234), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { Channel = (FourBitNumber)1 }, 10),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)70) { Channel = (FourBitNumber)1 }, 20),
                    new TimedEvent(new PitchBendEvent(123), 30),
                    new TimedEvent(new MarkerEvent("Marker"), 40),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 }, 40),
                    new TimedEvent(new MarkerEvent("Marker 2"), 50),
                    new TimedEvent(new TextEvent("Text"), 60),
                    new TimedEvent(new TextEvent("Text 2"), 70),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)1) { Channel = (FourBitNumber)10 }, 70),
                    new TimedEvent(new CuePointEvent("Point"), 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 }, 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { Channel = (FourBitNumber)10 }, 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { Channel = (FourBitNumber)1 }, 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)78, (SevenBitNumber)0) { Channel = (FourBitNumber)11 }, 100),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new SetTempoEvent(1234), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { Channel = (FourBitNumber)1 }, 10),
                    new Note((SevenBitNumber)2, 70, 20) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)70 },
                    new TimedEvent(new PitchBendEvent(123), 30),
                    new TimedEvent(new MarkerEvent("Marker"), 40),
                    new Note((SevenBitNumber)3, 40, 40) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)1, OffVelocity = (SevenBitNumber)1 },
                    new TimedEvent(new MarkerEvent("Marker 2"), 50),
                    new TimedEvent(new TextEvent("Text"), 60),
                    new TimedEvent(new TextEvent("Text 2"), 70),
                    new Note((SevenBitNumber)2, 10, 70) { Channel = (FourBitNumber)10, Velocity = (SevenBitNumber)1 },
                    new TimedEvent(new CuePointEvent("Point"), 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)78, (SevenBitNumber)0) { Channel = (FourBitNumber)11 }, 100),
                });
        }

        [Test]
        public void BuildObjects_TimedEventsAndNotes_SameNotesInTail()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100), 10),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)70), 20),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)1), 20),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)0), 20),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0), 30),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)1, (SevenBitNumber)0), 40),
                },
                outputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)1, 30, 10) { Velocity = (SevenBitNumber)100 },
                    new Note((SevenBitNumber)2, 0, 20) { Velocity = (SevenBitNumber)70, OffVelocity = (SevenBitNumber)1 },
                    new Note((SevenBitNumber)2, 10, 20) { Velocity = (SevenBitNumber)0 },
                });
        }

        #endregion

        #region Private methods

        private void CheckBuildingTimedEventsAndNotes(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects)
        {
            CheckObjectsBuilding(
                inputObjects,
                outputObjects,
                new ObjectsBuildingSettings
                {
                    BuildTimedEvents = true,
                    BuildNotes = true
                });
        }

        private void CheckObjectsBuilding(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects,
            ObjectsBuildingSettings settings)
        {
            var actualObjects = inputObjects
                .BuildObjects(settings)
                .ToList();

            CollectionAssert.AreEqual(outputObjects, actualObjects, new TimedObjectComparer());
        }

        #endregion
    }
}
