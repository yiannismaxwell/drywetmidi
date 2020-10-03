using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class TimedEventsBuilder : SequentialObjectsBuilder<TimedEventsBag>
    {
        #region Constructors

        public TimedEventsBuilder(List<ObjectsBag> objectsBags)
            : base(objectsBags)
        {
        }

        #endregion
    }
}
