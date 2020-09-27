using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal abstract class ObjectsBagsManager<TBag> where TBag : ObjectsBag, new()
    {
        #region Fields

        protected readonly List<ObjectsBag> _objectsBags = new List<ObjectsBag>();

        #endregion

        #region Methods

        public IEnumerable<ObjectsBag> GetObjectsBags()
        {
            return _objectsBags;
        }

        public abstract bool TryAddObject(ITimedObject timedObject);

        protected TBag GetNewBag()
        {
            return new TBag();
        }

        #endregion
    }
}
