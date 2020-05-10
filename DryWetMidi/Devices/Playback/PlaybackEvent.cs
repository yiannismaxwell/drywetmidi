using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class PlaybackEvent
    {
        #region Constructor

        public PlaybackEvent(ITimedObject sourceObject, MidiEvent midiEvent, TimeSpan time, long rawTime)
        {
            SourceObject = sourceObject;
            Event = midiEvent;
            Time = time;
            RawTime = rawTime;
        }

        #endregion

        #region Properties

        public ITimedObject SourceObject { get; }

        public MidiEvent Event { get; }

        public TimeSpan Time { get; }

        public long RawTime { get; }

        public PlaybackEventMetadata Metadata { get; } = new PlaybackEventMetadata();

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Event [{Event}] at [{Time}]";
        }

        #endregion
    }
}
