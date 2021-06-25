using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MenuBottomSample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            List<string> options = new List<string>()
            {
                "Option 1",
                "Option 2"
            };

            var option = await MenuBottomPage.ShowMenu(Navigation, "AÇÕES", options, options);

            ResultaLabel.Text = option;
            

        }
    }
}
