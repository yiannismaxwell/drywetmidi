namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class TimedEventsBagsManager : ObjectsBagsManager<TimedEventsBag>
    {
        #region Methods

        public override bool TryAddObject(ITimedObject timedObject)
        {
            var bag = GetNewBag();
            _objectsBags.Add(bag);
            return bag.TryAddObject(timedObject);
        }

        #endregion
    }
}
