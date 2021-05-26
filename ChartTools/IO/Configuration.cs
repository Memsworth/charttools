﻿namespace ChartTools.IO
{
    /// <summary>
    /// Difficulty of the <see cref="Track"/> to serve as a source of for track objects common to all difficulties to use for all tracks in the same <see cref="Instrument"/>
    /// 
    /// <para>Common track objects are:<list type="bullet">
    /// <item>Local events</item>
    /// <item>Star power phrases</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <remarks>Can be casted from <see cref="Difficulty"/>.</remarks>
    public enum CommonObjectsSource : byte
    {
        Easy, Medium, Hard, Expert,
        /// <summary>
        /// Each <see cref="Track"/> will contain a combinaition of all unique common track objects in the same <see cref="Instrument"/>
        /// </summary>
        Merge,
        Seperate
    }

    /// <summary>
    /// Defines how to handle "solo" local events in tracks 
    /// </summary>
    public enum SoloNoStarPowerRule : byte
    {
        /// <summary>
        /// Local events are interpreted as is
        /// </summary>
        Ignore,
        /// <summary>
        /// If a track has "solo" or "soloend" local events and no star power, convert the events into star power as interpreted by Clone Hero
        /// </summary>
        Convert
    }

    /// <summary>
    /// Configuration object to direct the reading of a file
    /// </summary>
    public class ReadingConfiguration
    {
        /// <inheritdoc cref="IO.SoloNoStarPowerRule"/>
        public SoloNoStarPowerRule SoloNoStarPowerRule { get; set; } = SoloNoStarPowerRule.Convert;
    }

    public class WritingConfiguration
    {
        /// <inheritdoc cref="IO.SoloNoStarPowerRule">
        public SoloNoStarPowerRule SoloNoStarPowerRule { get; set; } = SoloNoStarPowerRule.Convert;
        /// <inheritdoc cref="CommonObjectsSource">
        public CommonObjectsSource EventSource { get; set; } = CommonObjectsSource.Merge;
        /// <inheritdoc cref="CommonObjectsSource"/>
        public CommonObjectsSource StarPowerSource { get; set; } = CommonObjectsSource.Merge;
    }
}