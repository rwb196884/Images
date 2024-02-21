using DupImageLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rwb.Images
{
    public class Hashed
    {
        public FileInfo Left;
        public FileInfo Right;
        public float Compare;
    }

    public class ProgressEventArgs
    {
        public string Message { get; set; }
    }

    public delegate void ProgressEvent(object sender, ProgressEventArgs args);

    public enum ImageHashingAlgorighm
    {
        Average,
        Median64,
        //Median256,
        Difference64,
        //Difference256,
        Dct
    }

    public class DuplicateDetector
    {
        public event ProgressEvent OnProgress;
        private int _Files;
        public int Files { get { return _Files; } }

        private readonly string[] _Extensions = new string[] { ".jpg", ".jpeg", ".png" };

        private readonly Dictionary<FileInfo, ulong> _Hashes;

        private readonly ImageHashes _ImageHasher;
        public List<Hashed> Compared { get; private set; }

        private readonly DirectoryInfo _Root;
        private readonly ImageHashingAlgorighm _Alorithm;

        public DuplicateDetector(DirectoryInfo root, ImageHashingAlgorighm algorighm)
        {
            _Hashes = new Dictionary<FileInfo, ulong>();
            _ImageHasher = new ImageHashes(new ImageSharpTransformer());
            _Root = root;
            _Alorithm = algorighm;
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
                switch (_Alorithm)
                {
                    case ImageHashingAlgorighm.Average:
                        _Hashes.Add(file, _ImageHasher.CalculateAverageHash64(file.FullName));
                        break;
                    case ImageHashingAlgorighm.Median64:
                        _Hashes.Add(file, _ImageHasher.CalculateMedianHash64(file.FullName));
                        break;
                    //    case ImageHashingAlgorighm.Median256:
                    //_Hashes.Add(file, _ImageHasher.CalculateMedianHash256(file.FullName));
                    //        break;
                    case ImageHashingAlgorighm.Difference64:
                        _Hashes.Add(file, _ImageHasher.CalculateDifferenceHash64(file.FullName));
                        break;
                    //    case ImageHashingAlgorighm.Difference256:
                    //_Hashes.Add(file, _ImageHasher.CalculateDifferenceHash256(file.FullName));
                    //        break;
                    case ImageHashingAlgorighm.Dct:
                        _Hashes.Add(file, _ImageHasher.CalculateDctHash(file.FullName));
                        break;
                    default:
                        throw new NotImplementedException();

                }
            }
            catch (Exception e)
            {

            }
            _Files++;
            if (OnProgress != null)
            {
                OnProgress(this, new ProgressEventArgs() { Message = $"Found {_Files} files..." });
            }
        }
    }
}
