using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Rwb.Images
{
    public partial class Duplicates : ContentPage
    {
        private DirectoryInfo _Root;
        private DuplicateDetector _Detector;
        private int _I;

        public Duplicates()
        {
            InitializeComponent();
        }

        private async void OnClickButtonBack(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnClickButtonChooseLocation(object sender, EventArgs e)
        {
            FolderPickerResult result = await FolderPicker.Default.PickAsync();
            if (result.IsSuccessful)
            {
                _Root = new DirectoryInfo(result.Folder.Path);
                if (!_Root.Exists)
                {
                    LabelLocation.Text = "INVALID: " + result.Folder.Path;
                    ButtonStartScan.IsEnabled = false;
                }
                else
                {
                    LabelLocation.Text = "Location: " + result.Folder.Path;
                    ButtonStartScan.IsEnabled = true;
                }
            }
            else
            {
                LabelLocation.Text = "No location selected.";
                ButtonStartScan.IsEnabled = true;
            }
        }

        private async void OnClickButtonStartScan(object sender, EventArgs e)
        {
            _Detector = new DuplicateDetector(_Root);
            _Detector.OnProgress += _Detector_OnProgress;
            ButtonChooseLocation.IsEnabled = false;
            ButtonStartScan.IsEnabled = false;
            try
            {
                await Task.Run(_Detector.Detect);
            }
            catch (Exception ex)
            {

            }

            if(_Detector.Compared.Count > 0)
            {
                LabelScanning.Text = $"{_Detector.Files} files. {_Detector.Compared.Where(z => z.Compare >= 0.75).Count()} pairs with at least 75% match.";
                _I = 0;
                Show();
                Setup.IsVisible = false;
                Results.IsVisible = true;
            }
            else
            {
                LabelScanning.Text = $"{_Detector.Files} files. No matches.";
                ButtonChooseLocation.IsEnabled = true;
                ButtonStartScan.IsEnabled = false;
            }
        }

        private void _Detector_OnProgress(object sender, ProgressEventArgs args)
        {
            if (MainThread.IsMainThread)
            {
                try
                {
                    LabelScanning.Text = args.Message;
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => _Detector_OnProgress(sender, args));
            }
        }

        private void Show()
        {
            Hashed h = _Detector.Compared[_I];
            while (!h.Left.Exists || !h.Right.Exists)
            {
                _I++;
                h = _Detector.Compared[_I];
            }

            ButtonSkip.Text = $"Skip ({h.Compare * 100:#0})%";

            LabelLeft.Text = h.Left.FullName;
            ImageLeft.Source = ImageSource.FromFile(h.Left.FullName);

            LabelRight.Text = h.Right.FullName;
            ImageRight.Source = ImageSource.FromFile(h.Right.FullName);
        }

        private void OnClickButtonDeleteLeft(object sender, EventArgs e)
        {
            Hashed h = _Detector.Compared[_I];
            h.Left.Delete();
            _I++;
            Show();
        }

        private void OnClickButtonDeleteRight(object sender, EventArgs e)
        {
            Hashed h = _Detector.Compared[_I];
            h.Right.Delete();
            _I++;
            Show();
        }

        private void OnClickButtonButtonSkip(object sender, EventArgs e)
        {
            _I++;
            Show();
        }
    }
}