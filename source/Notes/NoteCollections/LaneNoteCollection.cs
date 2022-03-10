﻿using System;
using System.Linq;

namespace ChartTools
{
    public class LaneNoteCollection<TNote, TLane> : NoteCollection<TNote, TLane> where TNote : Note<TLane>, new() where TLane : struct, Enum
    {
        /// <summary>
        /// If <see langword="true"/>, trying to combine an open note with other notes will remove the current ones.
        /// </summary>
        public bool OpenExclusivity { get; }

        public LaneNoteCollection(bool openExclusivity)
        {
            OpenExclusivity = openExclusivity;
        }

        public override void Add(TLane lane)
        {
            if (Enum.IsDefined(lane))
            {
                base.Add(lane);

                if (OpenExclusivity)
                {
                    if (lane.Equals(0))
                        Notes.Clear();
                    else
                        base.Remove(lane);

                    base.Add(lane);
                }
            }
            else
                throw CommonExceptions.GetUndefinedException(lane);
        }
        public override bool Contains(TLane lane) => Enum.IsDefined(lane) ? base.Contains(lane) : throw CommonExceptions.GetUndefinedException(lane);
        public override bool Remove(TLane lane) => Enum.IsDefined(lane)? base.Remove(lane) : throw CommonExceptions.GetUndefinedException(lane);

        public override TNote? this[TLane lane] => Enum.IsDefined(lane) ? base[lane] : throw CommonExceptions.GetUndefinedException(lane);

        /// <summary>
        /// Adds a note to the <see cref="NoteCollection{TNote, TLane}"/>.
        /// </summary>
        /// <remarks>Adding a note that already exists will overwrite the existing note.
        ///     <para>If <see cref="OpenExclusivity"/> is <see langword="true"/>, combining an open note with other notes will remove the current ones.</para>
        /// </remarks>
        /// <param name="item">Item to add</param>
        public override void Add(TNote item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (OpenExclusivity && (item.NoteIndex == 0 || Count > 0 && this.First().NoteIndex == 0))
                Clear();

            base.Add(item);
        }
    }
}
