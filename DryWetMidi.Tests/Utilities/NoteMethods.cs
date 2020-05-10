using System;
using System.Collections;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    public sealed class NoteMethods : LengthedObjectMethods<Note>
    {
        #region Nested classes

        private sealed class NoteComparer : IComparer
        {
            #region IComparer

            public int Compare(object x, object y)
            {
                var xNote = (Note)x;
                var yNote = (Note)y;

                return NoteEquality.AreEqual(xNote, yNote) ? 0 : -1;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly Random _random = new Random();
        private readonly TempoMap _tempoMap;

        #endregion

        #region Constructor

        public NoteMethods()
        {
        }

        public NoteMethods(TempoMap tempoMap)
        {
            _tempoMap = tempoMap;
        }

        #endregion

        #region Methods

        public Note Create(SevenBitNumber noteNumber, ITimeSpan time, ITimeSpan length)
        {
            var convertedTime = TimeConverter.ConvertFrom(time, _tempoMap);
            return new Note(
                noteNumber,
                LengthConverter.ConvertFrom(length, convertedTime, _tempoMap),
                convertedTime);
        }

        #endregion

        #region Overrides

        protected override IComparer Comparer { get; } = new NoteComparer();

        public override void SetTime(Note obj, long time)
        {
            obj.Time = time;
        }

        public override void SetLength(Note obj, long length)
        {
            obj.Length = length;
        }

        public override Note Create(long time, long length)
        {
            return new Note((SevenBitNumber)_random.Next(SevenBitNumber.MaxValue), length, time);
        }

        public override Note Clone(Note obj)
        {
            return obj.Clone();
        }

        #endregion
    }
}
