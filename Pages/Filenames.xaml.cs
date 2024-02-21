using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Rwb.Images
{

    public partial class Filenames : ContentPage
    {
        private DirectoryInfo _Root;
        private Rename _Rename;
        private int _I;

        public Filenames()
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
                    ButtonStart.IsEnabled = false;
                }
                else
                {
                    LabelLocation.Text = "Location: " + result.Folder.Path;
                    ButtonStart.IsEnabled = true;
                }
            }
            else
            {
                LabelLocation.Text = "No location selected.";
                ButtonStart.IsEnabled = true;
            }
        }

        private async void OnClickButtonStart(object sender, EventArgs e)
        {
            _Rename = new Rename(_Root);
            _Rename.OnProgress += _Rename_OnProgress;
            ButtonChooseLocation.IsEnabled = false;
            ButtonStart.IsEnabled = false;
            await Task.Run(_Rename.Detect);
            LabelScanning.Text = $"Found {_Rename.Moves.Count} of {_Rename.Files} files that could be renamed.";
            if (_Rename.Moves.Count > 0)
            {
                _I = 0;
                Show();
                Results.IsVisible = true;
            }
            else
            {
                ButtonChooseLocation.IsEnabled = true;
                ButtonStart.IsEnabled = false;
                return;
            }
        }

        private void _Rename_OnProgress(object sender, ProgressEventArgs args)
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
                MainThread.BeginInvokeOnMainThread(() => _Rename_OnProgress(sender, args));
            }
        }

        private void Show()
        {
            if (_I >= _Rename.Moves.Count)
            {
                LabelScanning.Text = "No more images foud.";
                Results.IsVisible = false;
                return;
            }

            try
            {
                MoveSuggestion m = _Rename.Moves[_I];
                while (!m.File.Exists)
                {
                    _I++;
                    m = _Rename.Moves[_I];
                }

                MoveDescription.Text = $"{m.File.FullName} -> {m.NewName}";
                ImageToMove.Source = ImageSource.FromFile(m.File.FullName);
            }
            catch (Exception e)
            {

            }
        }

        private void OnClickButtonOk(object sender, EventArgs e)
        {
            try
            {
                MoveSuggestion m = _Rename.Moves[_I];
                m.Do();
                _I++;
            }
            catch (Exception ex)
            {

            }
            Show();
        }
        private void OnClickButtonSkip(object sender, EventArgs e)
        {
            _I++;
            Show();
        }
    }
}