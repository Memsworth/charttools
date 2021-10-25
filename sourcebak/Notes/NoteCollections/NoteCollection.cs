﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChartTools
{
    /// <summary>
    /// Set of notes played simultaneously
    /// </summary>
    public class NoteCollection<TNote, TLane> : ICollection<TNote> where TNote : Note<TLane>, new() where TLane : struct
    {
        protected virtual ICollection<TNote> Notes { get; }

        public int Count => Notes.Count;
        public bool IsReadOnly => false;

        public NoteCollection() => Notes = new List<TNote>();


        /// <summary>
        /// Adds a note to the <see cref="NoteCollection{TNote}"/>.
        /// </summary>
        /// <remarks>Adding a note that already exists will overwrite the existing note.
        ///     <para>If <see cref="OpenExclusivity"/> is <see langword="true"/>, combining an open note with other notes will remove the current ones.</para>
        /// </remarks>
        /// <param name="item">Item to add</param>
        //public override void Add(TNote item)
        //{
        //    if (item is null)
        //        throw GetNullNoteException(nameof(item));

        //    if (OpenExclusivity && (item.NoteIndex == 0 || Count > 0 && this[0].NoteIndex == 0))
        //        Clear();

        //    base.Add(item);
        //}
        //public bool Remove(TLane lane)
        //{
        //    TNote? n = this[lane];
        //    return n is not null && Remove(n);
        //}

        public virtual void Add(TLane lane) => Notes.Add(new() { Lane = lane });
        /// <summary>Adds a note to the collection.</summary>
        /// <exception cref="ArgumentNullException"/>
        public void Add(TNote item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            Notes.Add(item);
        }
        public virtual bool Contains(TLane lane) => Notes.Any(n => n.Lane.Equals(lane));
        public void Clear() => Notes.Clear();
        /// <summary>
        /// Determines if a note is contained in the collection.
        /// <exception cref="ArgumentNullException"/>
        public bool Contains(TNote item) => item is null ? throw new ArgumentNullException(nameof(item)) : Notes.Contains(item);
        public void CopyTo(TNote[] array, int arrayIndex) => Notes.CopyTo(array, arrayIndex);
        /// <summary>
        /// Removes a note from the collection.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public bool Remove(TNote item) => item is null ? throw new ArgumentNullException(nameof(item)) : Notes.Remove(item);
        public virtual bool Remove(TLane lane)
        {
            var note = Notes.FirstOrDefault(n => n.Lane.Equals(lane));
            return note is not null && Notes.Remove(note);
        }
        public IEnumerator<TNote> GetEnumerator() => Notes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the note matching a lane.
        /// </summary>
        public virtual TNote? this[TLane lane] => Notes.FirstOrDefault(n => n.Lane.Equals(lane));
    }
}
