﻿namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class AddAnchorAction : IPatternAction
    {
        #region Constructor

        public AddAnchorAction(object anchor)
        {
            Anchor = anchor;
        }

        #endregion

        #region Properties

        public object Anchor { get; }

        #endregion

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            context.AnchorTime(Anchor, time);
            return new PatternActionResult(time);
        }

        #endregion
    }
}
