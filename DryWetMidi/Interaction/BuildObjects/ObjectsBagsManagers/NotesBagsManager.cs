namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class NotesBagsManager : ObjectsBagsManager<NotesBag>
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
