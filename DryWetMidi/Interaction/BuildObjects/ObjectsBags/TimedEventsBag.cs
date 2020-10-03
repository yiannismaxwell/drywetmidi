﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class TimedEventsBag : ObjectsBag
    {
        #region Properties

        public override bool IsCompleted
        {
            get { return _timedObjects.Any(); }
        }

        #endregion

        #region Methods

        public override IEnumerable<ITimedObject> GetRawObjects()
        {
            throw new InvalidOperationException("Raw objects aren't defined for timed events.");
        }

        public override bool TryAddObject(ITimedObject timedObject, ObjectsBuildingSettings settings)
        {
            if (IsCompleted)
                return false;

            return TryAddTimedEvent(timedObject as TimedEvent) ||
                   TryAddNote(timedObject as Note) ||
                   TryAddChord(timedObject as Chord);
        }

        private bool TryAddTimedEvent(TimedEvent timedEvent)
        {
            if (timedEvent == null)
                return false;

            _timedObjects.Add(timedEvent);
            return true;
        }

        private bool TryAddNote(Note note)
        {
            if (note == null)
                return false;

            return TryAddTimedEvent(note.GetTimedNoteOnEvent()) && TryAddTimedEvent(note.GetTimedNoteOffEvent());
        }

        private bool TryAddChord(Chord chord)
        {
            if (chord == null)
                return false;

            var result = true;

            foreach (var note in chord.Notes)
            {
                result &= TryAddNote(note);
            }

            return result;
        }

        #endregion
    }
}
