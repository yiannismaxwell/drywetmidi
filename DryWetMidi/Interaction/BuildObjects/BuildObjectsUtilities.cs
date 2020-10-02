﻿using System.Collections.Generic;
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

            return BuildObjects(timedObjects.OrderBy(o => o.Time), settings, 0);
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

            return objectsBags
                .SelectMany(b => b.IsCompleted
                    ? b.GetObjects()
                    : BuildObjects(b.GetRawObjects(), settings, managersStartIndex + 1))
                .OrderBy(o => o.Time);
        }

        #endregion
    }
}
