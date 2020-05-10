using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    public interface IPlaybackSource
    {
        #region Events

        event EventHandler<ICollection<ITimedObject>> ObjectsAdded;

        event EventHandler<ICollection<ITimedObject>> ObjectsRemoved;

        event EventHandler<ICollection<ITimedObject>> ObjectsTimesChanged;

        event EventHandler CollectionModified;

        #endregion
    }
}
