namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class TimedEventBag : ObjectBag
    {
        #region Properties

        public override bool IsCompleted
        {
            get { return true; }
        }

        #endregion

        #region Methods

        public override bool TryAddObject(ITimedObject timedObject)
        {
            return TryAddTimedEvent(timedObject as TimedEvent) ||
                   TryAddNote(timedObject as Note) ||
                   TryAddChord(timedObject as Chord) ||
                   TryAddRegisteredParameter(timedObject as RegisteredParameter);
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

        private bool TryAddRegisteredParameter(RegisteredParameter registeredParameter)
        {
            if (registeredParameter == null)
                return false;

            var result = true;

            foreach (var timedEvent in registeredParameter.GetTimedEvents())
            {
                result &= TryAddTimedEvent(timedEvent);
            }

            return result;
        }

        #endregion
    }
}
