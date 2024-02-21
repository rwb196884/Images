using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Rwb.Images.Services;
using System;

namespace Rwb.Images
{
    public partial class MainPage : ContentPage
    {
        private readonly BuildConfiguration _BuildConfiguration;

        public MainPage(BuildConfiguration buildConfiguration)
        {
            InitializeComponent();
            _BuildConfiguration = buildConfiguration;
            if (_BuildConfiguration.Paid)
            {
                LabelPaid.Text = "Thanks for coughing up; the app is unlimited.";
            }
            else
            {
                LabelPaid.Text = "Oi: cough up. I'll probably get tired and quit soon.";
            }
        }

        private async void OnClickButtonFilenames(object sender, EventArgs e)
        {
            PermissionStatus sp1 = await Permissions.RequestAsync<Permissions.StorageRead>();
            PermissionStatus ps2 = await Permissions.RequestAsync<Permissions.StorageWrite>();
            Filenames w = new Filenames(_BuildConfiguration);
            await Navigation.PushAsync(w);
        }

        private async void OnClickButtonDuplicates(object sender, EventArgs e)
        {
            PermissionStatus sp1 = await Permissions.RequestAsync<Permissions.StorageRead>();
            PermissionStatus ps2 = await Permissions.RequestAsync<Permissions.StorageWrite>();
            Duplicates w = new Duplicates(_BuildConfiguration);
            await Navigation.PushAsync(w);
        }

        private async void OnClickButtonSettings(object sender, EventArgs e)
        {
            PermissionStatus sp1 = await Permissions.RequestAsync<Permissions.StorageRead>();
            PermissionStatus ps2 = await Permissions.RequestAsync<Permissions.StorageWrite>();
            Settings w = new Settings();
            await Navigation.PushAsync(w);
        }
    }
}
