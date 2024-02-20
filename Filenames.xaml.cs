using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls;
using System;
using System.IO;

namespace Rwb.Images
{

    public partial class Filenames : ContentPage
    {
        private DirectoryInfo _Root;
        public Filenames()
        {
            InitializeComponent();
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
        }
    }
}