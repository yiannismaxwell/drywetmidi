using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    internal abstract class SequentialObjectsBuilder<TBag> : ISequentialObjectsBuilder
        where TBag : ObjectsBag, new()
    {
        #region Fields

        private readonly ObjectsBuildingSettings _settings;

        private readonly List<ObjectsBag> _objectsBags;
        private readonly List<TBag> _uncompletedBags = new List<TBag>();

        #endregion

        #region Constructors

        public SequentialObjectsBuilder(List<ObjectsBag> objectsBags, ObjectsBuildingSettings settings)
        {
            _objectsBags = objectsBags;
            _settings = settings;
        }

        #endregion

        #region Methods

        public bool TryAddObject(ITimedObject timedObject)
        {
            var handlingBag = _uncompletedBags.FirstOrDefault(b => b.TryAddObject(timedObject, _settings));
            if (handlingBag != null)
            {
                if (!handlingBag.CanObjectsBeAdded)
                    _uncompletedBags.Remove(handlingBag);

                return true;
            }

            //

            var bag = new TBag();
            var result = bag.TryAddObject(timedObject, _settings);
            if (result)
            {
                _objectsBags.Add(bag);

                if (bag.CanObjectsBeAdded)
                    _uncompletedBags.Add(bag);
            }

            //

            return result;
        }

        #endregion
    }
}
