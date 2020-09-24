using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal abstract class ObjectBag
    {
        #region Fields

        protected readonly List<ITimedObject> _timedObjects = new List<ITimedObject>();

        #endregion

        #region Properties

        public abstract bool IsCompleted { get; }

        #endregion

        #region Methods

        public IEnumerable<ITimedObject> GetObjects()
        {
            return _timedObjects;
        }

        public abstract bool TryAddObject(ITimedObject timedObject);

        #endregion
    }
}
