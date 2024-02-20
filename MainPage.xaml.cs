using Microsoft.Maui.Controls;
using System;

namespace Rwb.Images
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnClickButtonFilenames(object sender, EventArgs e)
        {
            Filenames w = new Filenames();
            await Navigation.PushAsync(w);
        }

        private async void OnClickButtonDuplicates(object sender, EventArgs e)
        {
            Duplicates w = new Duplicates();
            await Navigation.PushAsync(w);
        }

        private async void OnClickButtonSettings(object sender, EventArgs e)
        {
            Settings w = new Settings();
            await Navigation.PushAsync(w);
        }
    }
}
