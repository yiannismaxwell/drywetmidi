using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal interface IPostBuilder
    {
        #region Methods

        IEnumerable<ITimedObject> BuildObjects(
            IEnumerable<ITimedObject> inputTimedObjects,
            IEnumerable<ITimedObject> resultTimedObjects,
            ObjectsBuildingSettings settings);

        #endregion
    }
}
