﻿using ChartTools.IO.Chart.Entries;

using System;

namespace ChartTools.IO.Chart.Parsers
{
    internal class MetadataParser : ChartPartParser
    {
        private Metadata? preResult, result;
        public override Metadata? Result => result;

        protected override void HandleLine(string line)
        {
            ChartEntry entry;
            try { entry = new(line); }
            catch (Exception e) { throw ChartParser.GetLineException(line, e); }

            string data = entry.Data.Trim('"');

            switch (entry.Header)
            {
                case "Name":
                    preResult!.Title = data;
                    break;
                case "Artist":
                    preResult!.Artist = data;
                    break;
                case "Charter":
                    preResult!.Charter = new() { Name = data };
                    break;
                case "Album":
                    preResult!.Album = data;
                    break;
                case "Year":
                    try { preResult!.Year = ushort.Parse(data.TrimStart(',')); }
                    catch (Exception e) { throw ChartParser.GetLineException(line, e); }
                    break;
                case "Offset":
                    try { preResult!.AudioOffset = (int)(float.Parse(entry.Data) * 1000); }
                    catch (Exception e) { throw ChartParser.GetLineException(line, e); }
                    break;
                case "Resolution":
                    try { preResult!.Resolution = ushort.Parse(data); }
                    catch (Exception e) { throw ChartParser.GetLineException(line, e); }
                    break;
                case "Difficulty":
                    try { preResult!.Difficulty = sbyte.Parse(data); }
                    catch (Exception e) { throw ChartParser.GetLineException(line, e); }
                    break;
                case "PreviewStart":
                    try { preResult!.PreviewStart = uint.Parse(data); }
                    catch (Exception e) { throw ChartParser.GetLineException(line, e); }
                    break;
                case "PreviewEnd":
                    try { preResult!.PreviewEnd = uint.Parse(data); }
                    catch (Exception e) { throw ChartParser.GetLineException(line, e); }
                    break;
                case "Genre":
                    preResult!.Genre = data;
                    break;
                case "MediaType":
                    preResult!.MediaType = data;
                    break;
                case "MusicStream":
                    preResult!.Streams.Music = data;
                    break;
                case "GuitarStream":
                    preResult!.Streams.Guitar = data;
                    break;
                case "BassStream":
                    preResult!.Streams.Bass = data;
                    break;
                case "RhythmStream":
                    preResult!.Streams ??= new();
                    preResult.Streams.Rhythm = data;
                    break;
                case "KeysStream":
                    preResult!.Streams ??= new();
                    preResult.Streams.Keys = data;
                    break;
                case "DrumStream":
                    preResult!.Streams ??= new();
                    preResult.Streams.Drum = data;
                    break;
                case "Drum2Stream":
                    preResult!.Streams ??= new();
                    preResult.Streams.Drum2 = data;
                    break;
                case "Drum3Stream":
                    preResult!.Streams ??= new();
                    preResult.Streams.Drum3 = data;
                    break;
                case "Drum4Stream":
                    preResult!.Streams ??= new();
                    preResult.Streams.Drum4 = data;
                    break;
                case "VocalStream":
                    preResult!.Streams ??= new();
                    preResult.Streams.Vocal = data;
                    break;
                case "CrowdStream":
                    preResult!.Streams ??= new();
                    preResult.Streams.Crowd = data;
                    break;
                default:
                    preResult!.UnidentifiedData.Add(new() { Key = entry.Header, Data = entry.Data, Origin = FileFormat.Chart });
                    break;
            }
        }

        protected override void PrepareParse() => preResult = new();
        protected override void FinaliseParse() => result = preResult;

        public override void ApplyResultToSong(Song song) => song.Metadata = result;
    }
}
