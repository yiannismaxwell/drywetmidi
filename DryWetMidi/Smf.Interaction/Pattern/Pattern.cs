﻿using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a musical pattern - set of notes with the specified times and lengths.
    /// </summary>
    public sealed class Pattern
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> with the specified actions.
        /// </summary>
        /// <param name="actions">Actions that pattern have to invoke on export to MIDI.</param>
        internal Pattern(IEnumerable<IPatternAction> actions)
        {
            Debug.Assert(actions != null);

            Actions = actions;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of actions that pattern have to invoke on export to MIDI.
        /// </summary>
        internal IEnumerable<IPatternAction> Actions { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Exports the current <see cref="Pattern"/> to track chunk.
        /// </summary>
        /// <param name="tempoMap">Tempo map to process pattern data according with.</param>
        /// <param name="channel">Channel of notes that will be generated by pattern.</param>
        /// <returns>The <see cref="TrackChunk"/> containing notes events generated by the current <see cref="Pattern"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is null.</exception>
        public TrackChunk ToTrackChunk(TempoMap tempoMap, FourBitNumber channel)
        {
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var context = new PatternContext(tempoMap, channel);
            var result = InvokeActions(0, context);

            //

            var trackChunk = new TrackChunk();

            using (var notesManager = trackChunk.ManageNotes())
            {
                notesManager.Notes.Add(result.Notes ?? Enumerable.Empty<Note>());
            }

            //

            return trackChunk;
        }

        /// <summary>
        /// Exports the current <see cref="Pattern"/> to MIDI file.
        /// </summary>
        /// <param name="tempoMap">Tempo map to process pattern data according with.</param>
        /// <param name="channel">Channel of notes that will be generated by pattern.</param>
        /// <returns>The <see cref="MidiFile"/> containing notes events generated by the current <see cref="Pattern"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is null.</exception>
        public MidiFile ToFile(TempoMap tempoMap, FourBitNumber channel)
        {
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var trackChunk = ToTrackChunk(tempoMap, channel);

            var midiFile = new MidiFile(trackChunk);
            midiFile.ReplaceTempoMap(tempoMap);

            return midiFile;
        }

        /// <summary>
        /// Exports the current <see cref="Pattern"/> to MIDI file using default tempo map.
        /// </summary>
        /// <param name="channel">Channel of notes that will be generated by pattern.</param>
        /// <returns>The <see cref="MidiFile"/> containing notes events generated by the current <see cref="Pattern"/>.</returns>
        public MidiFile ToFile(FourBitNumber channel)
        {
            return ToFile(TempoMap.Default, channel);
        }

        internal PatternActionResult InvokeActions(long time, PatternContext context)
        {
            var notes = new List<Note>();

            foreach (var action in Actions)
            {
                var actionResult = action.Invoke(time, context);

                var newTime = actionResult.Time;
                if (newTime != null)
                    time = newTime.Value;

                var addedNotes = actionResult.Notes;
                if (addedNotes != null)
                    notes.AddRange(addedNotes);
            }

            return new PatternActionResult(time, notes);
        }

        #endregion
    }
}