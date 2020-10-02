namespace Melanchall.DryWetMidi.Interaction
{
    internal interface IObjectsBagsManager
    {
        #region Methods

        bool TryAddObject(ITimedObject timedObject);

        #endregion
    }
}
