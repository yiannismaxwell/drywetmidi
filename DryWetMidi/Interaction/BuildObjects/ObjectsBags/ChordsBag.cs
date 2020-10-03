﻿using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class ChordsBag : ObjectsBag
    {
        #region Fields

        private readonly List<NotesBag> _notesBags = new List<NotesBag>();

        private long _chordStart = -1;

        #endregion

        #region Properties

        public override bool IsCompleted
        {
            get
            {
                if (!_notesBags.Any())
                    return _timedObjects.Any();

                return _notesBags.All(b => b.IsCompleted);
            }
        }

        #endregion

        #region Methods

        public override IEnumerable<ITimedObject> GetObjects()
        {
            var result = base.GetObjects();
            return _notesBags.Any()
                ? result.Concat(new[] { new Chord(_notesBags.SelectMany(b => b.GetObjects()).OfType<Note>()) })
                : result;
        }

        public override IEnumerable<ITimedObject> GetRawObjects()
        {
            return _notesBags.SelectMany(b => b.GetRawObjects());
        }

        public override bool TryAddObject(ITimedObject timedObject, ObjectsBuildingSettings settings)
        {
            if (IsCompleted)
                return false;

            return TryAddTimedEvent(timedObject as TimedEvent, settings) ||
                   TryAddNote(timedObject as Note, settings) ||
                   TryAddChord(timedObject as Chord);
        }

        private bool TryAddTimedEvent(TimedEvent timedEvent, ObjectsBuildingSettings settings)
        {
            if (timedEvent == null)
                return false;

            var handlingBag = _notesBags.FirstOrDefault(b => b.TryAddObject(timedEvent, settings));
            if (handlingBag != null)
                return true;

            return TryAddObjectToNewNoteBag(timedEvent, settings);
        }

        private bool TryAddNote(Note note, ObjectsBuildingSettings settings)
        {
            if (note == null)
                return false;

            return TryAddObjectToNewNoteBag(note, settings);
        }

        private bool TryAddChord(Chord chord)
        {
            if (chord == null)
                return false;

            _timedObjects.Add(chord);
            return true;
        }

        private bool TryAddObjectToNewNoteBag(ITimedObject timedObject, ObjectsBuildingSettings settings)
        {
            var bag = new NotesBag();
            if (!bag.TryAddObject(timedObject, settings))
                return false;

            var newNoteTime = bag.Time;
            if (_chordStart < 0)
            {
                _notesBags.Add(bag);
                _chordStart = newNoteTime;
                return true;
            }
            else
            {
                if (newNoteTime - _chordStart > settings.ChordBuilderSettings.NotesTolerance)
                    return false;

                _notesBags.Add(bag);
                return true;
            }
        }

        #endregion
    }
}
