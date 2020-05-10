using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    public interface ITimedObjectsCollectionChanged
    {
        #region Events

        event EventHandler<ICollection<ITimedObject>> ObjectsAdded;

        event EventHandler<ICollection<ITimedObject>> ObjectsRemoved;

        event EventHandler<ICollection<ITimedObject>> ObjectsTimesChanged;

        event EventHandler CollectionModified;

        #endregion
    }
}
