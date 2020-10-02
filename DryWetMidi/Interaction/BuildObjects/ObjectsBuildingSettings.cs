namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ObjectsBuildingSettings
    {
        #region Properties

        public bool BuildTimedEvents { get; set; }

        public bool BuildNotes { get; set; }

        public bool BuildChords { get; set; }

        public bool BuildRegisteredParameters { get; set; }

        public bool BuildRests { get; set; }

        public RestBuildingSettings RestBuildingSettings { get; set; }

        #endregion
    }
}
