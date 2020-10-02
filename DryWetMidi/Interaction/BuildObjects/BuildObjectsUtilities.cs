using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public static class BuildObjectsUtilities
    {
        #region Methods

        public static IEnumerable<ITimedObject> BuildObjects(
            this IEnumerable<ITimedObject> timedObjects,
            ObjectsBuildingSettings settings)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(settings), settings);

            return BuildObjects(timedObjects, settings, 0);
        }

        private static IEnumerable<ITimedObject> BuildObjects(
            this IEnumerable<ITimedObject> timedObjects,
            ObjectsBuildingSettings settings,
            int managersStartIndex)
        {
            var objectsBags = new List<ObjectsBag>();

            var objectsBagsManagers = new IObjectsBagsManager[]
            {
                settings.BuildNotes ? new ObjectsBagsManager<NotesBag>(objectsBags) : null,
                settings.BuildTimedEvents ? new ObjectsBagsManager<TimedEventsBag>(objectsBags) : null
            }
            .Where(m => m != null)
            .Skip(managersStartIndex)
            .ToArray();

            //

            foreach (var timedObject in timedObjects)
            {
                if (timedObject == null)
                {
                    // TODO: policy
                    continue;
                }

                var handlingManager = objectsBagsManagers.FirstOrDefault(m => m.TryAddObject(timedObject));
                if (handlingManager == null)
                {
                    // TODO: policy
                    continue;
                }
            }

            //

            var result = new List<ITimedObject>();

            foreach (var bag in objectsBags)
            {
                if (bag.IsCompleted)
                    result.AddRange(bag.GetObjects());
                else
                    result.AddRange(BuildObjects(bag.GetRawObjects(), settings, managersStartIndex + 1));
            }

            //

            return result.OrderBy(o => o.Time);
        }

        #endregion
    }
}
