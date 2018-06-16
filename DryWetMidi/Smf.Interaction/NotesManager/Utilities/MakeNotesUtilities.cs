﻿using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class MakeNotesUtilities
    {
        #region Nested classes

        private sealed class NoteEventsDescriptor
        {
            #region Constructor

            public NoteEventsDescriptor(TimedEvent noteOnTimedEvent, IEnumerable<TimedEvent> eventsTail)
            {
                NoteOnTimedEvent = noteOnTimedEvent;
                EventsTail = eventsTail;
            }

            #endregion

            #region Properties

            public TimedEvent NoteOnTimedEvent { get; }

            public TimedEvent NoteOffTimedEvent { get; private set; }

            public IEnumerable<TimedEvent> EventsTail { get; }

            public bool IsNoteCompleted { get; private set; }

            #endregion

            #region Methods

            public void CompleteNote(TimedEvent noteOffTimedEvent)
            {
                NoteOffTimedEvent = noteOffTimedEvent;
                IsNoteCompleted = true;
            }

            public bool IsCorrespondingNoteOffEvent(NoteOffEvent noteOffEvent)
            {
                return NoteEventUtilities.IsNoteOnCorrespondToNoteOff((NoteOnEvent)NoteOnTimedEvent.Event,
                                                                      noteOffEvent) &&
                       !IsNoteCompleted;
            }

            public IEnumerable<ITimedObject> GetTimedObjects()
            {
                if (IsNoteCompleted)
                    yield return new Note(NoteOnTimedEvent, NoteOffTimedEvent);
                else
                    yield return NoteOnTimedEvent;

                foreach (var eventFromTail in EventsTail)
                {
                    yield return eventFromTail;
                }
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Iterates through the specified collection of <see cref="TimedEvent"/> returning
        /// <see cref="Note"/> for Note On/Note Off event pairs and original <see cref="TimedEvent"/>
        /// for all other events.
        /// </summary>
        /// <remarks>
        /// If there is no corresponding Note Off event for Note On (or if there is no correspinding
        /// Note On event for Note Off) the event will be returned as is.
        /// </remarks>
        /// <param name="timedEvents">Collection of <see cref="TimedEvent"/> to make notes.</param>
        /// <returns>Collection of <see cref="ITimedObject"/> where an element either <see cref="TimedEvent"/>
        /// or <see cref="Note"/>.</returns>
        public static IEnumerable<ITimedObject> MakeNotes(this IEnumerable<TimedEvent> timedEvents)
        {
            ThrowIfArgument.IsNull(nameof(timedEvents), timedEvents);

            var noteEventsDescriptors = new List<NoteEventsDescriptor>();
            List<TimedEvent> eventsTail = null;

            foreach (var timedEvent in timedEvents)
            {
                var midiEvent = timedEvent?.Event;

                var noteOnEvent = midiEvent as NoteOnEvent;
                if (noteOnEvent != null)
                {
                    noteEventsDescriptors.Add(new NoteEventsDescriptor(timedEvent, eventsTail = new List<TimedEvent>()));
                    continue;
                }

                var noteOffEvent = midiEvent as NoteOffEvent;
                if (noteOffEvent != null)
                {
                    var noteEventsDescriptor = noteEventsDescriptors.FirstOrDefault(d => d.IsCorrespondingNoteOffEvent(noteOffEvent));
                    if (noteEventsDescriptor != null)
                    {
                        noteEventsDescriptor.CompleteNote(timedEvent);
                        if (noteEventsDescriptors.First() != noteEventsDescriptor)
                            continue;

                        for (int i = 0; i < noteEventsDescriptors.Count; i++)
                        {
                            var descriptor = noteEventsDescriptors[i];
                            if (!descriptor.IsNoteCompleted)
                                break;

                            foreach (var timedObject in descriptor.GetTimedObjects())
                            {
                                yield return timedObject;
                            }

                            noteEventsDescriptors.RemoveAt(i);
                            i--;
                        }

                        if (!noteEventsDescriptors.Any())
                            eventsTail = null;

                        continue;
                    }
                }

                if (eventsTail != null)
                    eventsTail.Add(timedEvent);
                else
                    yield return timedEvent;
            }

            foreach (var timedObject in noteEventsDescriptors.SelectMany(d => d.GetTimedObjects()))
            {
                yield return timedObject;
            }
        }

        #endregion
    }
}
