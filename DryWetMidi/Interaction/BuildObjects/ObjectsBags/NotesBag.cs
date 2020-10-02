using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class NotesBag : ObjectsBag
    {
        #region Fields

        private TimedEvent _timedNoteOnEvent;
        private TimedEvent _timedNoteOffEvent;
        private NoteId _noteId;

        #endregion

        #region Properties

        public override bool IsCompleted
        {
            get
            {
                if (_timedNoteOnEvent == null && _timedNoteOffEvent == null)
                    return _timedObjects.Any();

                return _timedNoteOnEvent != null && _timedNoteOffEvent != null;
            }
        }

        #endregion

        #region Methods

        public override IEnumerable<ITimedObject> GetRawObjects()
        {
            if (_timedNoteOnEvent != null && _timedNoteOffEvent != null)
                return Enumerable.Empty<ITimedObject>();

            return new[] { _timedNoteOnEvent, _timedNoteOffEvent }.Where(e => e != null);
        }

        public override bool TryAddObject(ITimedObject timedObject)
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

            switch (timedEvent.Event.EventType)
            {
                case MidiEventType.NoteOn:
                    {
                        if (_timedNoteOnEvent != null)
                            return false;

                        _timedNoteOnEvent = timedEvent;
                        _noteId = ((NoteOnEvent)timedEvent.Event).GetNoteId();
                        break;
                    }
                case MidiEventType.NoteOff:
                    {
                        if (_timedNoteOnEvent == null || _timedNoteOffEvent != null)
                            return false;

                        var noteId = ((NoteOffEvent)timedEvent.Event).GetNoteId();
                        if (!noteId.Equals(_noteId))
                            return false;

                        _timedNoteOffEvent = timedEvent;
                        if (IsCompleted)
                            _timedObjects.Add(new Note(_timedNoteOnEvent, _timedNoteOffEvent));

                        break;
                    }
                default:
                    return false;
            }

            return true;
        }

        private bool TryAddNote(Note note)
        {
            if (note == null)
                return false;

            _timedObjects.Add(note);
            return true;
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
