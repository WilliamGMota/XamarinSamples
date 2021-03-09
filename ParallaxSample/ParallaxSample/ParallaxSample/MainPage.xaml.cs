using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ParallaxSample
{
    public partial class MainPage : ContentPage
    {

        private double lastScroll = 0;
        private double previousScrollPosition;
        private bool animation = false;
        private bool showHeader = true;

        public MainPage()
        {
            InitializeComponent();
            lstView.ItemsSource = Constants.Items;
        }

        private async void lstView_Scrolled(object sender, ScrolledEventArgs e)
        {
            bool update = false;
            double heightHeader = HeaderFrame.HeightRequest;
            double heightHeaderNegative = heightHeader * -1;
            if (lastScroll != e.ScrollY )
            {

                var y = (int)((float)e.ScrollY / 1.5f);

                //UP
                if (y > lastScroll)
                    update = y > (lastScroll + 0.5); 
                //DOWN
                else
                    update = y < (lastScroll + 0.5);
                    
                if (update)
                {
                    lastScroll = y;
                    ScrollYLabel.Text = "y =>" + y.ToString();
                    double value = heightHeader - y;
                    if (value > 0)
                    {
                        if (lstView.TranslationY != value)
                            lstView.TranslationY = value;
                    }
                    else if (lstView.TranslationY != 0)
                        lstView.TranslationY = 0;
                    if (!animation && showHeader)
                    {
                        value = y * -1;
                        if (value < heightHeaderNegative)
                        {
                            if (HeaderFrame.TranslationY != heightHeaderNegative)
                            {
                                HeaderFrame.TranslationY = heightHeaderNegative;
                                showHeader = false;
                            }
                        }
                        else
                        {
                            if (HeaderFrame.TranslationY != value)
                                HeaderFrame.TranslationY = value;
                        }
                        ListY.Text = "Lista => " + lstView.TranslationY.ToString();
                        StackY.Text = "Header => " + HeaderFrame.TranslationY.ToString();
                    }
                }

                if (previousScrollPosition - 4 > e.ScrollY && HeaderFrame.TranslationY == heightHeaderNegative && !animation)
                {
                    animation = true;
                    showHeader = false;
                    await HeaderFrame.TranslateTo(HeaderFrame.TranslationX, 0, 100);
                }
                else if (previousScrollPosition + 4  < e.ScrollY && HeaderFrame.TranslationY == 0 && animation)
                {
                    animation = false;
                    showHeader = true;
                    if (y > heightHeader - 1)
                        await HeaderFrame.TranslateTo(HeaderFrame.TranslationX, heightHeaderNegative, 100);
                }

                previousScrollPosition = e.ScrollY;
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            lstView.TranslationY = 50;
        }
    }
}
