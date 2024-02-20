using DupImageLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rwb.Images
{
    internal class Hashed
    {
        public FileInfo Left;
        public FileInfo Right;
        public float Compare;
    }

    internal class ProgressEventArgs
    {
        public string Message { get; set; }
    }

    internal delegate void ProgressEvent(object sender, ProgressEventArgs args);


    internal class DuplicateDetector
    {
        public event ProgressEvent OnProgress;
        private int _Files;

        private readonly string[] _Extensions = new string[] { ".jpg", ".jpeg", ".png" };

        private readonly Dictionary<FileInfo, ulong> _Hashes;

        private readonly ImageHashes _ImageHasher;
        public List<Hashed> Compared { get; private set; }

        private readonly DirectoryInfo _Root;

        public DuplicateDetector(DirectoryInfo root)
        {
            _Hashes = new Dictionary<FileInfo, ulong>();
            _ImageHasher = new ImageHashes(new ImageSharpTransformer());
            _Root = root;
        }

        public void Detect()
        {
            _Files = 0;
            Process(_Root);
            if (OnProgress != null)
            {
                OnProgress(this, new ProgressEventArgs() { Message = $"Comparing {_Files} files..." });
            }

            Compared = _Hashes.SelectMany(z =>
            {
                return _Hashes.Where(y => StringComparer.OrdinalIgnoreCase.Compare(y.Key.FullName, z.Key.FullName) > 0).Select(y => new Hashed()
                {
                    Left = z.Key,
                    Right = y.Key,
                    Compare = ImageHashes.CompareHashes(z.Value, y.Value)
                });
            }).OrderByDescending(z => z.Compare)
            .ToList();
            if (OnProgress != null)
            {
                OnProgress(this, new ProgressEventArgs() {  Message = $"Compared {_Hashes.Count} files." });
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
                return;
            }
            try
            {
                _Hashes.Add(file, _ImageHasher.CalculateDctHash(file.FullName));
            }
            catch (Exception e)
            {

            }
            _Files++;
            if(OnProgress != null)
            {
                OnProgress(this, new ProgressEventArgs() { Message = $"Found {_Files} files..." });
            }
        }
    }
}
