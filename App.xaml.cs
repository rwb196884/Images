using Microsoft.Maui.Controls;

namespace Rwb.Images
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
