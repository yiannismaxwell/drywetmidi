using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    internal abstract class SequentialObjectsBuilder<TBag> : ISequentialObjectsBuilder
        where TBag : ObjectsBag, new()
    {
        #region Fields

        private readonly List<ObjectsBag> _objectsBags;
        private readonly List<TBag> _uncompletedBags = new List<TBag>();

        #endregion

        #region Constructors

        public SequentialObjectsBuilder(List<ObjectsBag> objectsBags)
        {
            _objectsBags = objectsBags;
        }

        #endregion

        #region Methods

        public bool TryAddObject(ITimedObject timedObject)
        {
            var handlingBag = _uncompletedBags.FirstOrDefault(b => b.TryAddObject(timedObject));
            if (handlingBag != null)
            {
                if (handlingBag.IsCompleted)
                    _uncompletedBags.Remove(handlingBag);

                return true;
            }

            //

            var bag = new TBag();
            var result = bag.TryAddObject(timedObject);
            if (result)
            {
                _objectsBags.Add(bag);

                if (!bag.IsCompleted)
                    _uncompletedBags.Add(bag);
            }

            //

            return result;
        }

        #endregion
    }
}
