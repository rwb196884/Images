using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IM = MetadataExtractor;

namespace Rwb.Images
{
    internal class MoveSuggestion
    {
        public FileInfo File { get; set; }
        public DateTime ImageDate { get; set; }
        public string NewName { get; set; }

        public void Do()
        {
            string newPath = Path.Combine(File.Directory.FullName, NewName);
            System.IO.File.Move(File.FullName, newPath);
            System.IO.File.SetCreationTime(newPath, ImageDate);
        }
    }

    internal delegate void RenameProgressEvent(object sender, ProgressEventArgs args);

    class FilenameRewrite
    {
        public Regex Source { get; set; }
        public int? MatchYear { get; set; }
        public int? MatchMonth { get; set; }
        public int? MatchDay { get; set; }
        public int? MatchHour { get; set; }
        public int? MatchMinute { get; set; }
        public int? MatchSecond { get; set; }

        public FilenameRewrite(string regex, int? year, int? month, int? day, int? hour, int? minute, int? second)
        {
            Source = new Regex(regex, RegexOptions.Compiled);
            MatchYear = year;
            MatchMonth = month;
            MatchDay = day;
            MatchHour = hour;
            MatchMinute = minute;
            MatchSecond = second;
        }

        public FilenameRewrite(string regex) : this(regex, 1, 2, 3, 4, 5, 6) { }

        public bool Test(FileInfo f)
        {
            return Source.IsMatch(f.Name);
        }

        public DateTime GetImageDate(FileInfo f)
        {
            DateTime t = DateTime.Now;
            Match m = Source.Match(f.Name);
            return new DateTime(
                MatchYear.HasValue ? int.Parse(m.Groups[MatchYear.Value].Value) : t.Year,
                MatchMonth.HasValue ? int.Parse(m.Groups[MatchMonth.Value].Value) : t.Month,
                MatchDay.HasValue ? int.Parse(m.Groups[MatchDay.Value].Value) : t.Day,
                MatchHour.HasValue ? int.Parse(m.Groups[MatchHour.Value].Value) : 0,
                MatchMinute.HasValue ? int.Parse(m.Groups[MatchMinute.Value].Value) : 0,
                MatchSecond.HasValue ? int.Parse(m.Groups[MatchSecond.Value].Value) : 0);
        }
    }


    internal class Rename
    {
        public event ProgressEvent OnProgress;
        private int _Files;
        private int _Skipped;
        public int Files { get { return _Files; } }

        private readonly string[] _Extensions = new string[] { ".jpg", ".jpeg", ".png" };

        private readonly Dictionary<string, int> _OtherExtensions;

        private readonly DirectoryInfo _Root;
        private readonly List<FilenameRewrite> _Rewriters;

        public List<MoveSuggestion> Moves { get; private set; }

        public Rename(DirectoryInfo root)
        {
            _OtherExtensions = new Dictionary<string, int>();
            _Root = root;
            _Rewriters = new List<FilenameRewrite>()
            {
                // Order is important!
                new FilenameRewrite(@"^(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})"),
                new FilenameRewrite(@"^IMG_(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})"),
                new FilenameRewrite(@"^PXL_(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})"),
                new FilenameRewrite(@"^(\d{4})-(\d{2})-(\d{2})_(\d{2})-(\d{2})-(\d{2})"),
                new FilenameRewrite(@"^IMG", null, null, null, null, null, null),
                new FilenameRewrite(@"^IMGA", null, null, null, null, null, null)
            };
            Moves = new List<MoveSuggestion>();
        }

        public void Detect()
        {
            _Files = 0;
            _Skipped = 0;
            Process(_Root);
        }

        private void Process(DirectoryInfo dir)
        {
            foreach (FileInfo f in dir.GetFiles())
            {
                Process(f);
            }

            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                Process(d);
            }
        }

        private void Process(FileInfo file)
        {
            _Files++;
            if (!_Extensions.Contains(file.Extension.ToLower()))
            {
                if (_OtherExtensions.ContainsKey(file.Extension.ToLower()))
                {
                    _OtherExtensions[file.Extension.ToLower()]++;
                }
                else
                {
                    _OtherExtensions.Add(file.Extension.ToLower(), 1);
                }
                return;
            }

            FilenameRewrite rw = _Rewriters.FirstOrDefault(z => z.Test(file));
            if (rw == null)
            {
                return;
            }

            List<IM.Tag> d = new List<IM.Tag>();
            try
            {
                d = IM.ImageMetadataReader.ReadMetadata(file.FullName).SelectMany(z => z.Tags)
                                .Where(z => z.Name.Contains("Date"))
                                .ToList();
            }
            catch (Exception e)
            {

            }

            if (d.Any())
            {
                DateTime? t = GetImageDate(d);

                if (t.HasValue)
                {
                    Move(file, t.Value);
                    return;
                }
            }

            Move(file, rw.GetImageDate(file));
        }

        private static DateTime? GetImageDate(IEnumerable<IM.Tag> tags)
        {
            IEnumerable<DateTime?> dates = tags.Select(z =>
            {
                bool ok = DateTime.TryParseExact(z.Description, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);
                if (ok)
                {
                    return result as DateTime?;
                }

                ok = DateTime.TryParseExact(z.Description, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result2);
                if (ok)
                {
                    return result2 as DateTime?;
                }
                return null;
            })
               .Where(z => z.HasValue);

            return dates.GroupBy(z => z).OrderByDescending(z => z.Count()).FirstOrDefault()?.Key;
        }

        private void Move(FileInfo f, DateTime t)
        {
            string newName = $"{t.Year:0000}-{t.Month:00}-{t.Day:00}_{t.Hour:00}-{t.Minute:00}{f.Extension.ToLower()}";
            string newPath = Path.Combine(f.Directory.FullName, newName);
            int n = 2;
            while (System.IO.File.Exists(newPath))
            {
                newName = $"{t.Year:0000}-{t.Month:00}-{t.Day:00}_{t.Hour:00}-{t.Minute:00}_{n}{f.Extension.ToLower()}";
                newPath = Path.Combine(f.Directory.FullName, newName);
                n++;
            }

            if (newName != f.Name)
            {
                Moves.Add(new MoveSuggestion()
                {
                    File = f,
                    ImageDate = t,
                    NewName = newName
                });
            }
            else
            {
                _Skipped++;
            }

            if (OnProgress != null)
            {
                OnProgress(this, new ProgressEventArgs() { Message = $"{_Files} files. {Moves.Count} to move, {_Skipped} to leave." });
            }
        }
    }
}
