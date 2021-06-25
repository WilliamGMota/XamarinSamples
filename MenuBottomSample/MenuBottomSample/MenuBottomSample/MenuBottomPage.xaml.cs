using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MenuBottomSample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuBottomPage : ContentPage
    {
        private TaskCompletionSource<String> _taskCompletionSource;

        public MenuBottomPage(string title, List<String> actions, List<String> redActions)
        {
            InitializeComponent();

            this.BackgroundColor = Color.Transparent;

            //Evento Tapped para quando clicar fora do menu
            var closeGestureRecognizer = new TapGestureRecognizer();
            closeGestureRecognizer.Tapped += async (s, e) => {
                await Navigation.PopModalAsync(false);
            };

            //Linha para separar os menus
            BoxView boxLine = new BoxView()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#20000000")
            };

            //Grid principal da tela
            Grid gridMain = new Grid()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            //BoxView invisivle para o Menu
            BoxView boxView = new BoxView()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#20000000")
            };

            boxView.GestureRecognizers.Add(closeGestureRecognizer);

            //Grid para desenhar o menu
            Grid gridMenu = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition()
                },
                Margin = new Thickness(8),
                ColumnSpacing = 0,
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = 0
            };

            //Frame para o menu de ações
            Frame frameMenu = new Frame()
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                BackgroundColor = Color.FromHex("#FFF"),
                CornerRadius = 8,
                Opacity = 0.9,
                Padding = new Thickness(0)
            };

            //Grid para os itens dos menus
            Grid gridMenuItems = new Grid()
            {
                Margin = new Thickness(0),
                RowSpacing = 0
            };

            //Montagens do Grid para as linhas do menu
            RowDefinitionCollection definitions = new RowDefinitionCollection();
            for (int i=0;i<actions.Count+redActions.Count+2;i++)
            {
                definitions.Add(new RowDefinition()
                {
                    Height = new GridLength(0, GridUnitType.Auto)
                });
                definitions.Add(new RowDefinition()
                {
                    Height = new GridLength(0, GridUnitType.Auto)
                });
            }

            gridMenuItems.RowDefinitions = definitions;

            //Montagens do leiaute para o título
            StackLayout titleStackLayout = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Spacing = 0,
                Padding = new Thickness(0, 4)
            };

            Label titleLabel = new Label()
            {
                Text = title,
                FontSize = 16,
                TextColor = Color.Gray,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            titleLabel.GestureRecognizers.Add(closeGestureRecognizer);

            titleStackLayout.Children.Add(titleLabel);
            gridMenuItems.Children.Add(titleStackLayout);
            gridMenuItems.Children.Add(BoxLine(), 0, 1);

            //Desenho dos itens dos menus
            int row = 2;
            foreach (var text in actions)
            {
                gridMenuItems.Children.Add(MenuItem(text, Color.Blue), 0, row++);
                gridMenuItems.Children.Add(BoxLine(), 0, row++);
            }
            foreach (var text in redActions)
            {
                gridMenuItems.Children.Add(MenuItem(text, Color.Red), 0, row++);
                gridMenuItems.Children.Add(BoxLine(), 0, row++);
            }

            //Desenho dos menu cancelar
            Frame frameCancel = new Frame()
            {
                CornerRadius = 8,
                Padding = new Thickness(0),
                Margin = new Thickness(0)
            };

            frameCancel.Content = MenuItemCancel("Cancel");

            //Montagem dos filhos do Content
            frameMenu.Content = gridMenuItems;
            gridMenu.Children.Add(frameMenu);
            gridMenu.Children.Add(frameCancel, 0, 1);

            gridMain.Children.Add(boxView);
            gridMain.Children.Add(gridMenu);

            this.Content = gridMain;
        }

        private Task<String> GetValue()
        {
            _taskCompletionSource = new TaskCompletionSource<String>();
            return _taskCompletionSource.Task;
        }

        public static async Task<String> ShowMenu(INavigation navigation, string title, List<String> action, List<String> redActions)
        {
            var viewModel = new MenuBottomPage(title, action, redActions);
            await navigation.PushModalAsync(viewModel, false);
            var value = await viewModel.GetValue();
            await navigation.PopModalAsync(false);
            return value;
        }

        private BoxView BoxLine()
        {
            return new BoxView()
            {
                BackgroundColor = Color.Gray,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = 0.1,
            };
        }

        private StackLayout MenuItem(string text, Color textColor)
        {

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += async (s, e) => {
                var stack = (StackLayout)s;
                stack.Opacity = 0;
                stack.BackgroundColor = Color.FromHex("#f5f5f5");
                await stack.FadeTo(1, 100);
                var item = (Label)stack.Children.First();
                _taskCompletionSource.SetResult(item.Text);
                _taskCompletionSource = null;
            };

            var stackLayout = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(0, 12),
                Spacing = 0
            };

            var label = new Label()
            {
                Text = text,
                FontSize = 18,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                TextColor = textColor
            };

            stackLayout.GestureRecognizers.Add(tapGestureRecognizer);
            stackLayout.Children.Add(label);

            var teste = new TouchEffect();

            return stackLayout;
        }


        private StackLayout MenuItemCancel(string text)
        {

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += async (s, e) => {
                var stack = (StackLayout)s;
                stack.Opacity = 0;
                stack.BackgroundColor = Color.FromHex("#f5f5f5");
                await stack.FadeTo(1, 100);
                await Navigation.PopModalAsync(false);
            };

            var stackLayout = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Padding = new Thickness(0, 12),
                Spacing = 0
            };

            var label = new Label()
            {
                Text = text,
                FontSize = 18,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                TextColor = Color.Blue
            };

            stackLayout.GestureRecognizers.Add(tapGestureRecognizer);
            stackLayout.Children.Add(label);

            return stackLayout;
        }

    }
}