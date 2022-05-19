﻿using Melanchall.DryWetMidi.Core;

namespace ChartTools.IO.Configuration
{
    /// <summary>
    /// Configuration object to direct the reading of a file
    /// </summary>
    /// <inheritdoc cref="CommonConfiguration" path="/remarks"/>
    public class ReadingConfiguration : CommonConfiguration
    {
        public UnopenedTrackObjectPolicy UnopenedTrackObjectPolicy { get; set; }
        public UnclosedTrackObjectPolicy UnclosedTracjObjectPolicy { get; set; }
        public UnknownSectionPolicy UnknownSectionPolicy { get; set; }
        /// <summary>
        /// Configuration object to customize how DryWetMidi reads Midi file before being parsed
        /// </summary>
        /// <remarks>Setting to <see landword="null"/> will use default settings</remarks>
        public ReadingSettings? MidiFirstPassReadingSettings { get; set; }
    }
}
