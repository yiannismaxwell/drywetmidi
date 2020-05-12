﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    // TODO: add TimeChanged to ITimedObject
    // TODO: add LengthChanged to ILengthedObject
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested classes

        private sealed class PlaybackSource : IEnumerable<ITimedObject>, IPlaybackSource
        {
            #region Events

            public event EventHandler<ICollection<ITimedObject>> ObjectsAdded;
            public event EventHandler<ICollection<ITimedObject>> ObjectsRemoved;
            public event EventHandler<ICollection<ITimedObject>> ObjectsTimesChanged;
            public event EventHandler CollectionModified;

            #endregion

            #region Fields

            private readonly List<ITimedObject> _timedObjects = new List<ITimedObject>();

            #endregion

            #region Methods

            public void Add(ITimedObject timedObject)
            {
                _timedObjects.Add(timedObject);
                OnObjectsAdded(timedObject);
            }

            public void Add(params ITimedObject[] timedObjects)
            {
                _timedObjects.AddRange(timedObjects);
                OnObjectsAdded(timedObjects);
            }

            public void Remove(ITimedObject timedObject)
            {
                _timedObjects.Remove(timedObject);
                OnObjectsRemoved(timedObject);
            }

            public void FireObjectsTimesChanged(ITimedObject timedObject)
            {
                OnObjectsTimesChanged(timedObject);
            }

            public void Clear()
            {
                _timedObjects.Clear();
                OnCollectionModified();
            }

            private void OnObjectsAdded(params ITimedObject[] timedObjects)
            {
                ObjectsAdded?.Invoke(this, timedObjects);
            }

            private void OnObjectsRemoved(params ITimedObject[] timedObjects)
            {
                ObjectsRemoved?.Invoke(this, timedObjects);
            }

            private void OnObjectsTimesChanged(params ITimedObject[] timedObjects)
            {
                ObjectsTimesChanged?.Invoke(this, timedObjects);
            }

            private void OnCollectionModified()
            {
                CollectionModified?.Invoke(this, EventArgs.Empty);
            }

            #endregion

            #region IEnumerable<ITimedObject>

            public IEnumerator<ITimedObject> GetEnumerator()
            {
                return _timedObjects.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        private sealed class SourceModifier
        {
            #region Fields

            private readonly IEnumerator<(TimeSpan Time, Action Action)> _modifiersEnumerator;
            private readonly Timer _timer = new Timer();

            #endregion

            #region Constructor

            public SourceModifier(params (TimeSpan Time, Action Action)[] modifiers)
            {
                _modifiersEnumerator = modifiers.OrderBy(m => m.Time).GetEnumerator();
                _timer.Elapsed += OnTimerElapsed;
                InitializeTimer();
            }

            #endregion

            #region Methods

            private void OnTimerElapsed(object sender, ElapsedEventArgs e)
            {
                _modifiersEnumerator.Current.Action();
                InitializeTimer();
            }

            private void InitializeTimer()
            {
                _timer.Stop();

                if (!_modifiersEnumerator.MoveNext())
                    return;

                _timer.Interval = _modifiersEnumerator.Current.Time.TotalMilliseconds - 200;
                _timer.Start();
            }

            #endregion
        }

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void Add_AtMiddle_AfterStop()
        {
            //  ======== 1s
            //
            // 0   add add|stop|   add                  add             
            // |====|===|=|    |====|============ 3      |              |  A    s = 0      l = 3      |
            //      |   |           |  2 ================|======== 5    |  B    s = 2      l = 3      > initial
            //      |   |           |          3 ========|======== 5    |  C    s = 3      l = 2      |
            //      |   |           |                    |              -------------------------------
            //      |   |           >====== 2.25         |              |  D    s = 1.5    l = 0.75   |
            //      |   |           |                    >==== 4.5      |  E    s = 4      l = 0.5    > added
            //      |   >=|    |= 1.125                                 |  F    s = 0.875  l = 0.25   |
            //      >== 0.75                                            |  G    s = 0.5    l = 0.25   |

            var aProperties = (NoteNumber: (SevenBitNumber)10, Time: TimeSpan.Zero, Length: TimeSpan.FromSeconds(3));
            var a = GetNote(aProperties);

            var bProperties = (NoteNumber: (SevenBitNumber)20, Time: TimeSpan.FromSeconds(2), Length: TimeSpan.FromSeconds(3));
            var b = GetNote(bProperties);

            var cProperties = (NoteNumber: (SevenBitNumber)30, Time: TimeSpan.FromSeconds(3), Length: TimeSpan.FromSeconds(2));
            var c = GetNote(cProperties);

            var dProperties = (NoteNumber: (SevenBitNumber)40, Time: TimeSpan.FromSeconds(1.5), Length: TimeSpan.FromSeconds(0.75));
            var d = GetNote(dProperties);

            var eProperties = (NoteNumber: (SevenBitNumber)50, Time: TimeSpan.FromSeconds(4), Length: TimeSpan.FromSeconds(0.5));
            var e = GetNote(eProperties);

            var fProperties = (NoteNumber: (SevenBitNumber)60, Time: TimeSpan.FromSeconds(0.875), Length: TimeSpan.FromSeconds(0.25));
            var f = GetNote(fProperties);

            var gProperties = (NoteNumber: (SevenBitNumber)70, Time: TimeSpan.FromSeconds(0.5), Length: TimeSpan.FromSeconds(0.25));
            var g = GetNote(gProperties);

            var source = new PlaybackSource { a, b, c };
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            CheckPlaybackSource(
                source,
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(aProperties.NoteNumber, Note.DefaultVelocity), aProperties.Time),
                    // new ReceivedEvent(new NoteOnEvent(gProperties.NoteNumber, Note.DefaultVelocity), gProperties.Time),
                    // new ReceivedEvent(new NoteOffEvent(gProperties.NoteNumber, SevenBitNumber.MinValue), gProperties.Time + gProperties.Length),
                    // new ReceivedEvent(new NoteOnEvent(fProperties.NoteNumber, Note.DefaultVelocity), fProperties.Time),
                    new ReceivedEvent(new NoteOffEvent(fProperties.NoteNumber, SevenBitNumber.MinValue), fProperties.Time + fProperties.Length),
                    new ReceivedEvent(new NoteOnEvent(dProperties.NoteNumber, Note.DefaultVelocity), dProperties.Time),
                    new ReceivedEvent(new NoteOnEvent(bProperties.NoteNumber, Note.DefaultVelocity), bProperties.Time),
                    new ReceivedEvent(new NoteOffEvent(dProperties.NoteNumber, SevenBitNumber.MinValue), dProperties.Time + dProperties.Length),
                    new ReceivedEvent(new NoteOffEvent(aProperties.NoteNumber, SevenBitNumber.MinValue), aProperties.Time + aProperties.Length),
                    new ReceivedEvent(new NoteOnEvent(cProperties.NoteNumber, Note.DefaultVelocity), cProperties.Time),
                    new ReceivedEvent(new NoteOnEvent(eProperties.NoteNumber, Note.DefaultVelocity), eProperties.Time),
                    new ReceivedEvent(new NoteOffEvent(eProperties.NoteNumber, SevenBitNumber.MinValue), eProperties.Time + eProperties.Length),
                    new ReceivedEvent(new NoteOffEvent(bProperties.NoteNumber, SevenBitNumber.MinValue), bProperties.Time + bProperties.Length),
                    new ReceivedEvent(new NoteOffEvent(cProperties.NoteNumber, SevenBitNumber.MinValue), cProperties.Time + cProperties.Length),
                },
                setupPlayback: NoPlaybackAction,
                beforePlaybackStarted: NoPlaybackAction,
                afterPlaybackStarted: NoPlaybackAction,
                stopAfter: TimeSpan.FromSeconds(1),
                stopPeriod: stopPeriod,
                afterPlaybackStopped: (context, playback) => source.Add(d, e, f, g),
                afterPlaybackResumed: NoPlaybackAction,
                afterPlaybackFinished: NoPlaybackAction);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_AtMiddle_BeforePlaying()
        {
            //  ======== 1s
            //
            // 0   add add|stop|   add                  add             
            // |====|===|=|    |====|============ 3      |              |  A    s = 0      l = 3      |
            //      |   |           |  2 ================|======== 5    |  B    s = 2      l = 3      > initial
            //      |   |           |          3 ========|======== 5    |  C    s = 3      l = 2      |
            //      |   |           |                    |              -------------------------------
            //      |   |           >====== 2.25         |              |  D    s = 1.5    l = 0.75   |
            //      |   |           |                    >==== 4.5      |  E    s = 4      l = 0.5    > added
            //      |   >=|    |= 1.125                                 |  F    s = 0.875  l = 0.25   |
            //      >== 0.75                                            |  G    s = 0.5    l = 0.25   |

            var aProperties = (NoteNumber: (SevenBitNumber)10, Time: TimeSpan.Zero, Length: TimeSpan.FromSeconds(3));
            var a = GetNote(aProperties);

            var bProperties = (NoteNumber: (SevenBitNumber)20, Time: TimeSpan.FromSeconds(2), Length: TimeSpan.FromSeconds(3));
            var b = GetNote(bProperties);

            var cProperties = (NoteNumber: (SevenBitNumber)30, Time: TimeSpan.FromSeconds(3), Length: TimeSpan.FromSeconds(2));
            var c = GetNote(cProperties);

            var dProperties = (NoteNumber: (SevenBitNumber)40, Time: TimeSpan.FromSeconds(1.5), Length: TimeSpan.FromSeconds(0.75));
            var d = GetNote(dProperties);

            var eProperties = (NoteNumber: (SevenBitNumber)50, Time: TimeSpan.FromSeconds(4), Length: TimeSpan.FromSeconds(0.5));
            var e = GetNote(eProperties);

            var fProperties = (NoteNumber: (SevenBitNumber)60, Time: TimeSpan.FromSeconds(0.875), Length: TimeSpan.FromSeconds(0.25));
            var f = GetNote(fProperties);

            var gProperties = (NoteNumber: (SevenBitNumber)70, Time: TimeSpan.FromSeconds(0.5), Length: TimeSpan.FromSeconds(0.25));
            var g = GetNote(gProperties);

            var source = new PlaybackSource { a, b, c };
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            CheckPlaybackSource(
                source,
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(aProperties.NoteNumber, Note.DefaultVelocity), aProperties.Time),
                    new ReceivedEvent(new NoteOnEvent(gProperties.NoteNumber, Note.DefaultVelocity), gProperties.Time),
                    new ReceivedEvent(new NoteOffEvent(gProperties.NoteNumber, SevenBitNumber.MinValue), gProperties.Time + gProperties.Length),
                    new ReceivedEvent(new NoteOnEvent(fProperties.NoteNumber, Note.DefaultVelocity), fProperties.Time),
                    new ReceivedEvent(new NoteOffEvent(fProperties.NoteNumber, SevenBitNumber.MinValue), fProperties.Time + fProperties.Length),
                    new ReceivedEvent(new NoteOnEvent(dProperties.NoteNumber, Note.DefaultVelocity), dProperties.Time),
                    new ReceivedEvent(new NoteOnEvent(bProperties.NoteNumber, Note.DefaultVelocity), bProperties.Time),
                    new ReceivedEvent(new NoteOffEvent(dProperties.NoteNumber, SevenBitNumber.MinValue), dProperties.Time + dProperties.Length),
                    new ReceivedEvent(new NoteOffEvent(aProperties.NoteNumber, SevenBitNumber.MinValue), aProperties.Time + aProperties.Length),
                    new ReceivedEvent(new NoteOnEvent(cProperties.NoteNumber, Note.DefaultVelocity), cProperties.Time),
                    new ReceivedEvent(new NoteOnEvent(eProperties.NoteNumber, Note.DefaultVelocity), eProperties.Time),
                    new ReceivedEvent(new NoteOffEvent(eProperties.NoteNumber, SevenBitNumber.MinValue), eProperties.Time + eProperties.Length),
                    new ReceivedEvent(new NoteOffEvent(bProperties.NoteNumber, SevenBitNumber.MinValue), bProperties.Time + bProperties.Length),
                    new ReceivedEvent(new NoteOffEvent(cProperties.NoteNumber, SevenBitNumber.MinValue), cProperties.Time + cProperties.Length),
                },
                setupPlayback: NoPlaybackAction,
                beforePlaybackStarted: (context, playback) => source.Add(d, e, f, g),
                afterPlaybackStarted: NoPlaybackAction,
                stopAfter: TimeSpan.FromSeconds(1),
                stopPeriod: stopPeriod,
                afterPlaybackStopped: NoPlaybackAction,
                afterPlaybackResumed: NoPlaybackAction,
                afterPlaybackFinished: NoPlaybackAction);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_AtMiddle_Ongoing()
        {
            //  ======== 1s
            //
            // 0   add add   add                  add             
            // |====|===|=====|============ 3      |              |  A    s = 0      l = 3      |
            //      |   |     |  2 ================|======== 5    |  B    s = 2      l = 3      > initial
            //      |   |     |          3 ========|======== 5    |  C    s = 3      l = 2      |
            //      |   |     |                    |              -------------------------------                              
            //      |   |     >====== 2.25         |              |  D    s = 1.5    l = 0.75   |
            //      |   |     |                    >==== 4.5      |  E    s = 4      l = 0.5    > added
            //      |   >== 1.125                                 |  F    s = 0.875  l = 0.25   |
            //      >== 0.75                                      |  G    s = 0.5    l = 0.25   |

            var aProperties = (NoteNumber: (SevenBitNumber)10, Time: TimeSpan.Zero, Length: TimeSpan.FromSeconds(3));
            var a = GetNote(aProperties);

            var bProperties = (NoteNumber: (SevenBitNumber)20, Time: TimeSpan.FromSeconds(2), Length: TimeSpan.FromSeconds(3));
            var b = GetNote(bProperties);

            var cProperties = (NoteNumber: (SevenBitNumber)30, Time: TimeSpan.FromSeconds(3), Length: TimeSpan.FromSeconds(2));
            var c = GetNote(cProperties);

            var dProperties = (NoteNumber: (SevenBitNumber)40, Time: TimeSpan.FromSeconds(1.5), Length: TimeSpan.FromSeconds(0.75));
            var d = GetNote(dProperties);

            var eProperties = (NoteNumber: (SevenBitNumber)50, Time: TimeSpan.FromSeconds(4), Length: TimeSpan.FromSeconds(0.5));
            var e = GetNote(eProperties);

            var fProperties = (NoteNumber: (SevenBitNumber)60, Time: TimeSpan.FromSeconds(0.875), Length: TimeSpan.FromSeconds(0.25));
            var f = GetNote(fProperties);

            var gProperties = (NoteNumber: (SevenBitNumber)70, Time: TimeSpan.FromSeconds(0.5), Length: TimeSpan.FromSeconds(0.25));
            var g = GetNote(gProperties);

            var source = new PlaybackSource { a, b, c };
            var sourceModifier = new SourceModifier(
                (dProperties.Time, () => source.Add(d)),
                (eProperties.Time, () => source.Add(e)),
                (fProperties.Time, () => source.Add(f)),
                (gProperties.Time, () => source.Add(g)));

            CheckPlaybackSource(
                source,
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(aProperties.NoteNumber, Note.DefaultVelocity), aProperties.Time),
                    new ReceivedEvent(new NoteOnEvent(gProperties.NoteNumber, Note.DefaultVelocity), gProperties.Time),
                    new ReceivedEvent(new NoteOffEvent(gProperties.NoteNumber, SevenBitNumber.MinValue), gProperties.Time + gProperties.Length),
                    new ReceivedEvent(new NoteOnEvent(fProperties.NoteNumber, Note.DefaultVelocity), fProperties.Time),
                    new ReceivedEvent(new NoteOffEvent(fProperties.NoteNumber, SevenBitNumber.MinValue), fProperties.Time + fProperties.Length),
                    new ReceivedEvent(new NoteOnEvent(dProperties.NoteNumber, Note.DefaultVelocity), dProperties.Time),
                    new ReceivedEvent(new NoteOnEvent(bProperties.NoteNumber, Note.DefaultVelocity), bProperties.Time),
                    new ReceivedEvent(new NoteOffEvent(dProperties.NoteNumber, SevenBitNumber.MinValue), dProperties.Time + dProperties.Length),
                    new ReceivedEvent(new NoteOffEvent(aProperties.NoteNumber, SevenBitNumber.MinValue), aProperties.Time + aProperties.Length),
                    new ReceivedEvent(new NoteOnEvent(cProperties.NoteNumber, Note.DefaultVelocity), cProperties.Time),
                    new ReceivedEvent(new NoteOnEvent(eProperties.NoteNumber, Note.DefaultVelocity), eProperties.Time),
                    new ReceivedEvent(new NoteOffEvent(eProperties.NoteNumber, SevenBitNumber.MinValue), eProperties.Time + eProperties.Length),
                    new ReceivedEvent(new NoteOffEvent(bProperties.NoteNumber, SevenBitNumber.MinValue), bProperties.Time + bProperties.Length),
                    new ReceivedEvent(new NoteOffEvent(cProperties.NoteNumber, SevenBitNumber.MinValue), cProperties.Time + cProperties.Length),
                },
                setupPlayback: NoPlaybackAction,
                beforePlaybackStarted: NoPlaybackAction,
                afterPlaybackStarted: NoPlaybackAction,
                stopAfter: TimeSpan.Zero,
                stopPeriod: TimeSpan.Zero,
                afterPlaybackStopped: NoPlaybackAction,
                afterPlaybackResumed: NoPlaybackAction,
                afterPlaybackFinished: NoPlaybackAction);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_AtStart_AfterStop()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_AtStart_BeforePlaying()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_AtStart_Ongoing()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_AtEnd_AfterStop()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_AtEnd_BeforePlaying()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_AtEnd_Ongoing()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_AfterCurrent_BeforePlaying()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_AfterCurrent_AtEnd_Ongoing()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_BeforeCurrent_BeforePlaying()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Add_BeforeCurrent_AtEnd_Ongoing()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Remove_AtMiddle_AfterStop()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Remove_AtMiddle_Ongoing()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Remove_AtStart_AfterStop()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Remove_AtStart_Ongoing()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Remove_AtEnd_AfterStop()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Remove_AtEnd_Ongoing()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Remove_Current_AfterStop()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void Remove_Current_Ongoing()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TimesChanged_AfterStop()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TimesChanged_Ongoing()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CurrentObjectTimeChanged_AfterStop()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CurrentObjectTimeChanged_Ongoing()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CollectionModified_AfterStop()
        {
            throw new NotImplementedException();
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CollectionModified_Ongoing()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private methods

        private static Note GetNote((SevenBitNumber NoteNumber, TimeSpan Time, TimeSpan Length) properties)
        {
            var noteMethods = new NoteMethods(TempoMap.Default);
            return noteMethods.Create(properties.NoteNumber, (MetricTimeSpan)properties.Time, (MetricTimeSpan)properties.Length);
        }

        #endregion
    }
}