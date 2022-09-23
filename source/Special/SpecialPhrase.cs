﻿namespace ChartTools
{
    public abstract class SpecialPhrase : TrackObjectBase, ILongTrackObject
    {
        /// <summary>
        /// Numerical value of the phrase type
        /// </summary>
        public byte TypeCode { get; set; }
        public uint Length { get; set; }

        /// <summary>
        /// Base constructor of special phrases.
        /// </summary>
        /// <param name="position">Position of the phrase</param>
        /// <param name="typeCode">Effect of the phrase</param>
        /// <param name="length">Duration in ticks</param>
        public SpecialPhrase(uint position, byte typeCode, uint length = 0) : base(position)
        {
            TypeCode = typeCode;
            Length = length;
        }
    }
}