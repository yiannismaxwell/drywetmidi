﻿using System.Collections.Generic;

namespace Melanchall.DryMidi
{
    /// <summary>
    /// Provides a way to convert a collection of <see cref="Chunk"/> objects to
    /// another representation according to rules defined by implementation of this
    /// interface.
    /// </summary>
    internal interface IChunksConverter
    {
        /// <summary>
        /// Converts collection of <see cref="Chunk"/> objects to another representation.
        /// </summary>
        /// <param name="chunks">Chunks collection that need to be converted.</param>
        /// <returns>Another representation of the <paramref name="chunks"/> collection.</returns>
        IEnumerable<Chunk> Convert(IEnumerable<Chunk> chunks);
    }
}
