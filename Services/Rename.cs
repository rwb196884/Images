using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IM = MetadataExtractor;

namespace Rwb.Images
{
       internal class RenameProgressEventArgs
    {
        public string OldName { get; set; }
        public string NewName { get; set; }
    }

    internal delegate void RenameProgressEvent(object sender, RenameProgressEventArgs args);

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
            throw new NotImplementedException();
        }

        public Match Match(string thing)
        {
            throw new NotImplementedException();
        }
    }


    internal class Rename
    {
        public event ProgressEvent OnProgress;

        private readonly string[] _Extensions = new string[] { ".jpg", ".jpeg", ".png" };

        private readonly Dictionary<string, int> _OtherExtensions;

        private static readonly FilenameRewrite _re0 = new FilenameRewrite(@"^(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})");
        private static readonly FilenameRewrite _re1 = new FilenameRewrite(@"^IMG_(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})");
        private static readonly FilenameRewrite _re2 = new FilenameRewrite(@"^PXL_(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})");
        private static readonly FilenameRewrite _re3 = new FilenameRewrite(@"^(\d{4})-(\d{2})-(\d{2})_(\d{2})-(\d{2})-(\d{2})");

        private readonly DirectoryInfo _Root;

        public Rename(DirectoryInfo root)
        {
            _OtherExtensions = new Dictionary<string, int>();
            _Root = root;
        }

        public void Detect()
        {
            Process(_Root);
            if (OnProgress != null)
            {
                //OnProgress(this, new ProgressEventArgs() { Percent = 0, Message = $"Comparing {_Files} files..." });
            }
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

            Match m0 = _re0.Match(file.Name);
            Match m1 = _re1.Match(file.Name);
            Match m2 = _re2.Match(file.Name);
            Match m3 = _re3.Match(file.Name);
            if (!(file.Name.StartsWith("IMAG") || file.Name.StartsWith("IMG") || (new Match[] { m0, m1, m2, m3 }).Any(z => z.Success)))
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

            if (m0.Success)
            {
                DateTime t0 = new DateTime(int.Parse(m0.Groups[1].Value), int.Parse(m0.Groups[2].Value), int.Parse(m0.Groups[3].Value), int.Parse(m0.Groups[4].Value), int.Parse(m0.Groups[5].Value), int.Parse(m0.Groups[6].Value));
                Move(file, t0);
                return;
            }

            if (m1.Success)
            {
                DateTime t1 = new DateTime(int.Parse(m1.Groups[1].Value), int.Parse(m1.Groups[2].Value), int.Parse(m1.Groups[3].Value));
                Move(file, t1);
                return;
            }

            if (m2.Success)
            {
                DateTime t2 = new DateTime(int.Parse(m2.Groups[1].Value), int.Parse(m2.Groups[2].Value), int.Parse(m2.Groups[3].Value));
                Move(file, t2);
                return;
            }

            if (m3.Success)
            {
                DateTime t3 = new DateTime(int.Parse(m3.Groups[1].Value), int.Parse(m3.Groups[2].Value), int.Parse(m3.Groups[3].Value), int.Parse(m3.Groups[4].Value), int.Parse(m3.Groups[5].Value), int.Parse(m3.Groups[6].Value));
                Move(file, t3);
                return;
            }
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

        private static void Move(FileInfo f, DateTime t)
        {
            string newName = $"{t.Year:0000}-{t.Month:00}-{t.Day:00}_{t.Hour:00}-{t.Minute:00}{f.Extension.ToLower()}";
            string newPath = Path.Combine(f.Directory.FullName, newName);
            int n = 2;
            while (File.Exists(newPath))
            {
                newName = $"{t.Year:0000}-{t.Month:00}-{t.Day:00}_{t.Hour:00}-{t.Minute:00}_{n}{f.Extension.ToLower()}";
                newPath = Path.Combine(f.Directory.FullName, newName);
                n++;
            }
            if (newName != f.Name)
            {
                Console.WriteLine($"{f.Name} -> {newName}");
                File.Move(f.FullName, newPath);
                File.SetCreationTime(newPath, t);
            }
        }
    }
}
