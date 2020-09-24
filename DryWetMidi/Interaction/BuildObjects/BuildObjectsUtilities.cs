using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public static class BuildObjectsUtilities
    {
        #region Methods

        public static IEnumerable<ITimedObject> BuildObjects(this IEnumerable<ITimedObject> timedObjects, ObjectsBuildingSettings settings)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(settings), settings);

            foreach (var timedObject in timedObjects)
            {
                if (timedObject == null)
                {
                    // TODO: policy
                    continue;
                }
            }

            return null;
        }

        #endregion
    }
}
