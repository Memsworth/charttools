﻿using System;

namespace ChartTools.Lyrics
{
    public struct VocalsPitch : IEquatable<VocalsPitch>, IEquatable<VocalsPitches>
    {
        /// <summary>
        /// Pitch value
        /// </summary>
        public VocalsPitches Pitch { get; }
        /// <summary>
        /// Key excluding the octave
        /// </summary>
        public VocalsKey Key => (VocalsKey)((int)Pitch & 0x0F);
        /// <summary>
        /// Octave number
        /// </summary>
        public byte Octave => (byte)(((int)Pitch & 0xF0) >> 4);


        public VocalsPitch(VocalsPitches pitch) => Pitch = pitch;

        #region Equals
        public bool Equals(VocalsPitch other) => Pitch == other.Pitch;
        public bool Equals(VocalsPitches other) => Pitch == other;
        public override bool Equals(object? obj) => obj is VocalsPitch pitch && Equals(pitch);
        #endregion

        #region Operators
        public static implicit operator VocalsPitch(VocalsPitches pitch) => new(pitch);
        public static bool operator ==(VocalsPitch left, VocalsPitch right) => left.Equals(right);
        public static bool operator !=(VocalsPitch left, VocalsPitch right) => !(left == right);
        public static bool operator <(VocalsPitch left, VocalsPitch right) => left.Pitch < right.Pitch;
        public static bool operator >(VocalsPitch left, VocalsPitch right) => left.Pitch > right.Pitch;
        #endregion

        public override int GetHashCode() => (int)Pitch;
    }
}
