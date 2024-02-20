using Microsoft.Maui.Controls;
using System;

namespace Rwb.Images
{
	public partial class Settings : ContentPage
	{
		public Settings()
		{
			InitializeComponent();
		}

        private async void OnClickButtonBack(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}