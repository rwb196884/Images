using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Rwb.Images.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rwb.Images
{
    public partial class Filenames : ContentPage
    {
        class MoveAction : ObservableObject
        {

            private bool _Apply;
            public bool Apply
            {
                get { return _Apply; }
                set
                {
                    SetProperty(ref _Apply, value);
                    SetProperty(ref _Colour, value ? "#aaffaa" : "#FFFFFF");
                }
            }

            private MoveSuggestion _Suggestion;
            public MoveSuggestion Suggestion
            {
                get { return _Suggestion; }
                set { SetProperty(ref _Suggestion, value); }
            }

            public ImageSource Source { get { return ImageSource.FromFile(_Suggestion.File.FullName); } }
            public string Path { get { return _Suggestion.File.Directory.FullName; } }
            public string OldName { get { return _Suggestion.File.Name; } }
            public string NewName { get { return _Suggestion.NewName; } }

            private string _Colour;
            public string Colour { get { return _Colour; } }
        }

        //class FilenamesBindingContext: BindableObject
        //{
        //    public List<MoveAction> Moves { get; set; }
        //}

        private DirectoryInfo _Root;
        private Rename _Rename;
        private int _BatchSize;
        private int _I;

        //private readonly FilenamesBindingContext _BindingContext;
        public Filenames(BuildConfiguration buildConfiguration)
        {
            _BatchSize = buildConfiguration.Paid ? 13 : 5;
            //_BindingContext = new FilenamesBindingContext()
            //{
            //    Moves = new List<MoveAction>(),
            //};
            InitializeComponent();
            //BindingContext = _BindingContext;
            // Binding doesn't work. Fucking cunt.
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
                OnClickButtonApply(null, null);
                // Results.IsVisible = true;
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

        private List<MoveAction> _CurrentItems;

        private void OnClickButtonApply(object sender, EventArgs e)
        {
            //_BindingContext.Moves = _Rename.Moves.Skip(_I).Take(13).Select(
            //    z => new MoveAction()
            //    {
            //        Apply = true,
            //        Suggestion = z
            //    }).ToList();

            if (_I >= _Rename.Moves.Count)
            {
                Cunt.IsVisible = false;
                ButtonChooseLocation.IsEnabled = true;
                ButtonStart.IsEnabled = false;
                LabelScanning.Text = "No more images.";
                return;
            }

            _CurrentItems = _Rename.Moves.Skip(_I).Take(_BatchSize).Select(
                z => new MoveAction()
                {
                    Apply = false,
                    Suggestion = z
                }).ToList();
            Cunt.ItemsSource = new ObservableCollection<MoveAction>(_CurrentItems);
            Cunt.IsVisible = true;
            LabelScanning.Text = $"{_I + 1} to {(_I + 1 + _BatchSize < _Rename.Moves.Count() ? _I + 1 + _BatchSize : _Rename.Moves.Count())} of {_Rename.Moves.Count()}";
            _I += _BatchSize;
        }

        void OnItemTapped(object sender, ItemTappedEventArgs args)
        {
            MoveAction item = args.Item as MoveAction;
            item.Apply = !item.Apply;
            // This is the only way I can find to get the bastard piece of shit to update.
            Cunt.ItemsSource = new ObservableCollection<MoveAction>(_CurrentItems);
        }
    }
}