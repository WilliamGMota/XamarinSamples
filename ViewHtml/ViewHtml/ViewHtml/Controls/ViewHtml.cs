using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace ViewHtml.Controls
{
    public class ViewHtml : ContentView
    {

        public static readonly BindableProperty HtmlProperty = BindableProperty.Create(
                                                                 propertyName: "Html",
                                                                 returnType: typeof(string),
                                                                 declaringType: typeof(ViewHtml),
                                                                 defaultValue: "",
                                                                 defaultBindingMode: BindingMode.TwoWay,
                                                                 propertyChanged: HtmlPropertyChanged);

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set
            {
                SetValue(HtmlProperty, value);
                HtmlToView(value);
            }
        }

        private static void HtmlPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ViewHtml)bindable;
            control.Html = newValue.ToString();
        }

        void HtmlToView(string html)
        {

            var result = new List<StringSection>();

            if (html is string val)
            {

                // Remove tags que não estão tratadas
                val = Regex.Replace(val, "<span([^>]*)>", string.Empty);
                val = Regex.Replace(val, "<p([^>]*)>", string.Empty);
                val = Regex.Replace(val, "<div([^>]*)>", string.Empty);

                val = Regex.Replace(val, "&nbsp;", " ");
                val = val.Replace("</b>", string.Empty);
                val = val.Replace("</span>", string.Empty);
                val = val.Replace("</p>", Environment.NewLine);
                val = val.Replace("</div>", Environment.NewLine);
                val = val.Replace("<br>", Environment.NewLine);
                val = val.Replace("<br />", Environment.NewLine);
                val = val.Replace("<br/>", Environment.NewLine);
                val = Regex.Replace(val, "<b([^>]*)>", string.Empty);

                if (val?.Length > 0 && val.Substring(val.Length - 1) == Environment.NewLine)
                    val = val.Remove(val.Length - 1);

                //Processa os Links do Html
                var sections = ProcessString(val);
                var resultImage = new List<StringSection>();

                foreach (var item in sections)
                {
                    var sectionsDetail = ProcessImage(item.Text, item.Link);
                    if (sectionsDetail.Count > 0)
                    {
                        foreach (var sectionItem in sectionsDetail)
                            resultImage.Add(sectionItem);
                    }
                    else
                        resultImage.Add(item);
                }

                //Processa as Tags <H1> a <H6>
                foreach (var item in resultImage)
                {
                    var sectionsDetail = ProcessH(item.Text, item.Link);
                    if (sectionsDetail.Count > 0)
                    {
                        foreach (var sectionItem in sectionsDetail)
                            result.Add(sectionItem);
                    }
                    else
                        result.Add(item);
                }

                //Monta a formatação
                foreach (var item in result)
                    item.Sections = FormatTextSection(item);
            }

            // Cria o StackLayout para armazenar os Labels e imagens
            StackLayout stackLayout = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 0),
                Padding = new Thickness(0, 0),
                Spacing = 0,
                Orientation = StackOrientation.Vertical
            };

            Label label = NewLabel();
            var formatted = new FormattedString();
            foreach (var item in result)
            {
                //Ajusta tags que não foram tratadas
                item.Image = FindUrlImage(item.Image);

                if (!string.IsNullOrEmpty(item.Image))
                {
                    //Inclui o Label no StackLayout 
                    if (formatted != new FormattedString())
                    {
                        label.FormattedText = formatted;
                        stackLayout.Children.Add(label);
                        label = NewLabel();
                        formatted = new FormattedString();
                    }

                    // Cria o Grid para a Imagem
                    Grid grid = new Grid
                    {
                        RowDefinitions =
                        {
                            new RowDefinition { Height = 100 }
                        },
                        ColumnDefinitions =
                        {
                            new ColumnDefinition() { Width = 130 }
                        },
                        Margin = new Thickness(0),
                        Padding = new Thickness(0),
                        ColumnSpacing = 0,
                        RowSpacing = 0
                    };

                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += async (s, e) => {
                        Debug.Print("ABRIR VISUALIZAÇÃO");
                        //await Application.Current.MainPage.AbrirModal(new ViewImagePage(item.Image), false);
                    };

                    //Cria a Imagem
                    var ffImage = new FFImageLoading.Forms.CachedImage()
                    {
                        Source = item.Image,
                        VerticalOptions = LayoutOptions.Fill,
                        HorizontalOptions = LayoutOptions.Fill,
                        Margin = new Thickness(0),
                        Aspect = Aspect.AspectFill,
                    };
                    ffImage.GestureRecognizers.Add(tapGestureRecognizer);

                    //Incluir a Imagem no Grid e o Grid no StackLayout
                    grid.Children.Add(ffImage);
                    stackLayout.Children.Add(grid);
                }
                else
                {
                    foreach(var sectionSpan in item.Sections)
                    {
                        sectionSpan.Text = RemoveTags(sectionSpan.Text);
                        if (!string.IsNullOrEmpty(sectionSpan.Text))
                            formatted.Spans.Add(CreateSpan(sectionSpan));
                    }
                        
                }
                    
            }

            if (formatted != new FormattedString())
            {
                label.FormattedText = formatted;
                stackLayout.Children.Add(label);
            }

            Content = stackLayout;
        }


        List<StringSection> FormatTextSection(StringSection section)
        {

            var tagHml = GetNextTag(section.Text);

            if (string.IsNullOrEmpty(tagHml))
                return new List<StringSection>() { section };

            string spanPattern = $@"(<{tagHml}.*>.*?</{tagHml}>)";

            MatchCollection collection = Regex.Matches(section.Text, spanPattern, RegexOptions.Singleline);

            var sections = new List<StringSection>();

            var lastIndex = 0;
            string rawText = section.Text;

            foreach (Match item in collection)
            {

                var nextSection = new StringSection()
                {
                    Text = string.Empty,
                    Bold = section.Bold,
                    Italic = section.Italic,
                    Strikethrough = section.Strikethrough,
                    Underline = section.Underline
                };


                var itemSection = new StringSection()
                {
                    Text = rawText.Substring(lastIndex, item.Index - lastIndex),
                    Bold = section.Bold,
                    Italic = section.Italic,
                    Strikethrough = section.Strikethrough,
                    Underline = section.Underline
                };

                sections.Add(itemSection);
                lastIndex = item.Index + item.Length;

                // Get HTML href 
                itemSection = new StringSection()
                {
                    Text = rawText.Substring(item.Index),
                    Bold = section.Bold,
                    Italic = section.Italic,
                    Strikethrough = section.Strikethrough,
                    Underline = section.Underline
                };

                MatchCollection matchs = Regex.Matches(itemSection.Text, $"<{tagHml}.*?>");
                if (matchs.Count > 0)
                {
                    var matchItem = matchs[0];
                    itemSection.Text = itemSection.Text.Remove(matchItem.Index, matchItem.Value.Length);
                }

                matchs = Regex.Matches(itemSection.Text, $"</{tagHml}>");
                if (matchs.Count > 0)
                {
                    var matchItem = matchs[0];
                    nextSection.Text = itemSection.Text.Substring(matchItem.Index + matchItem.Value.Length);
                    itemSection.Text = itemSection.Text.Remove(matchItem.Index, matchItem.Value.Length);
                    itemSection.Text = itemSection.Text.Substring(0, matchItem.Index);
                }


                /*
                itemSection.Text = Regex.Replace(itemSection.Text, $"<{tagHml}.*?>", string.Empty);
                itemSection.Text = Regex.Replace(itemSection.Text, $"</{tagHml}>", string.Empty);
                */

                switch (tagHml)
                {
                    case "strong":
                        itemSection.Bold = true;
                        break;
                    case "em":
                        itemSection.Italic = true;
                        break;
                    case "u":
                        itemSection.Underline = true;
                        break;
                    case "s":
                        itemSection.Strikethrough = true;
                        break;
                }

                sections.Add(itemSection);

                if (!string.IsNullOrEmpty(nextSection.Text))
                    sections.Add(nextSection);
            }

            /*
            if (lastIndex < rawText.Length)
            {
                // Get HTML href 
                var itemSection = new StringSection()
                {
                    Text = rawText.Substring(lastIndex),
                    Bold = section.Bold,
                    Italic = section.Italic,
                    Strikethrough = section.Strikethrough,
                    Underline = section.Underline
                };

                sections.Add(itemSection);
            }
            */

            if (sections.Count > 1)
            {
                var newSections = new List<StringSection>();
                foreach (var current in sections)
                {
                    var items = FormatTextSection(current);
                    newSections.AddRange(items);
                }
                sections = newSections;
            }

            return sections;

        }

        string GetNextTag(string text)
        {
            text = " " + text;
            var positions = new Dictionary<string, int>();
            var patterns = new Dictionary<string, string>();

            patterns.Add("strong", "<strong([^>]*)>");
            patterns.Add("s", "<s([^>]*)>");
            patterns.Add("em", "<em([^>]*)>");
            patterns.Add("u", "<u([^>]*)>");

            foreach(var pattern in patterns)
            {
                if (Regex.Match(text, pattern.Value).Success)
                    positions.Add(pattern.Key, Regex.Match(text, pattern.Value).Index);
            }

            var tag = positions.Where(x => x.Value != 0).OrderBy(x => x.Value).FirstOrDefault();

            if (tag.Value == 0)
                return string.Empty;

            return tag.Key;
        }

        private Span CreateSpan(StringSection section)
        {
            var span = new Span()
            {
                Text = section.Text,
                FontSize = 12,
                TextColor = Color.Black
            };

            if (!string.IsNullOrEmpty(section.Link))
            {

                span.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = _navigationCommand,
                    CommandParameter = section.Link
                });

                /*
                if (Funcoes.GetPublicationTypeByUrl(section.Link) != PublicationType.None)
                {
                    span.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = _publicationCommand,
                        CommandParameter = section.Link
                    });
                }
                else
                {
                    span.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = _navigationCommand,
                        CommandParameter = section.Link
                    });
                }
                */

                span.TextColor = Color.Blue;
                span.TextDecorations = TextDecorations.Underline;
            }

            if (section.Mencao)
            {
                span.TextColor = Color.Blue;
                span.TextDecorations = TextDecorations.Underline;
            }

            if (section.Bold)
                span.FontAttributes = FontAttributes.Bold;
            if (section.Italic)
                span.FontAttributes = FontAttributes.Italic;
            if (section.Bold && section.Italic)
                span.FontAttributes = FontAttributes.Bold | FontAttributes.Italic;

            if (section.Underline)
                span.TextDecorations = TextDecorations.Underline;
            if (section.Strikethrough)
                span.TextDecorations = TextDecorations.Strikethrough;
            if (section.Underline && section.Strikethrough)
                span.TextDecorations = TextDecorations.Underline | TextDecorations.Strikethrough;

            if (!string.IsNullOrEmpty(section.H))
            {
                if (section.H == "1")
                    span.FontSize = 15;
                if (section.H == "2")
                    span.FontSize = 14.5;
                if (section.H == "3")
                    span.FontSize = 14;
                if (section.H == "4")
                    span.FontSize = 13.5;
                if (section.H == "5")
                    span.FontSize = 13;
                if (section.H == "6")
                    span.FontSize = 12.5;
            }

            return span;
        }


        public List<StringSection> ProcessString(string rawText)
        {
            const string spanPattern = @"(<a.*?>.*?</a>)";

            MatchCollection collection = Regex.Matches(rawText, spanPattern, RegexOptions.Singleline);

            var sections = new List<StringSection>();

            var lastIndex = 0;

            foreach (Match item in collection)
            {
                sections.Add(new StringSection() { Text = rawText.Substring(lastIndex, item.Index - lastIndex) });
                lastIndex = item.Index + item.Length;

                // Get HTML href 
                var html = new StringSection()
                {
                    Link = Regex.Match(item.Value, "(?<=href=\\\")[\\S]+(?=\\\")").Value,
                    Text = Regex.Replace(item.Value, "<.*?>", string.Empty)
                };

                sections.Add(html);
            }

            if (lastIndex < rawText.Length)
                sections.Add(new StringSection() { Text = rawText.Substring(lastIndex) });
            return sections;
        }

        public List<StringSection> ProcessImage(string rawText, string link)
        {
            const string spanPattern = @"(<img.*?>)";

            MatchCollection collection = Regex.Matches(rawText, spanPattern, RegexOptions.Singleline);

            var sections = new List<StringSection>();

            var lastIndex = 0;

            foreach (Match item in collection)
            {

                sections.Add(new StringSection() { Text = rawText.Substring(lastIndex, item.Index - lastIndex) });
                lastIndex = item.Index + item.Length;

                // Get HTML href 
                var html = new StringSection()
                {
                    Image = Regex.Match(item.Value, "(?<=src=\\\")[\\S]+(?=\\\")").Value,
                    Text = Regex.Replace(item.Value, "<.*?>", string.Empty),
                    Link = link
                };

                sections.Add(html);
            }

            if (lastIndex < rawText.Length)
                sections.Add(new StringSection() { Text = rawText.Substring(lastIndex), Link = link });
            return sections;
        }


        public List<StringSection> ProcessH(string rawText, string link)
        {
            const string spanPattern = @"(<h[0-6].*>.*?</h[0-6]>)";

            MatchCollection collection = Regex.Matches(rawText, spanPattern, RegexOptions.Singleline);

            var sections = new List<StringSection>();

            var lastIndex = 0;

            foreach (Match item in collection)
            {
                sections.Add(new StringSection() { Text = rawText.Substring(lastIndex, item.Index - lastIndex) });
                lastIndex = item.Index + item.Length;

                // Get HTML href 
                var html = new StringSection()
                {
                    Text = Regex.Replace(item.Value, "<.*?>", string.Empty),
                    Bold = true,
                    H = item.Value.Substring(2, 1),
                    Link = link
                };

                sections.Add(html);
            }

            if (lastIndex < rawText.Length)
                sections.Add(new StringSection() { Text = rawText.Substring(lastIndex), Link = link });
            return sections;
        }


        /// <summary>
        /// Estrutura para montagem do Span e Imagem
        /// </summary>
        public class StringSection
        {
            public string Text { get; set; }
            public string Link { get; set; }
            public bool Bold { get; set; }
            public bool Italic { get; set; }
            public bool Underline { get; set; }
            public bool Strikethrough { get; set; }
            public bool Mencao { get; set; }
            public string H { get; set; }
            public string Image { get; set; }
            public List<StringSection> Sections { get; set; }
        }

        /// <summary>
        /// Função para Criar um Label Vazio com as propriedades padrão
        /// </summary>
        private Label NewLabel()
        {
            return new Label()
            {
                Margin = new Thickness(0, 0),
                Padding = new Thickness(0, 0),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Start,
                TextColor = Color.Black,
                FontSize = 12
            };
        }

        /// <summary>
        /// Remove as Tags não tratada no por conflitos
        /// </summary>
        private string RemoveTags(string value)
        {

            value = Regex.Replace(value, $"<ol>", string.Empty);
            value = Regex.Replace(value, $"</ol>", string.Empty);
            value = Regex.Replace(value, $"<ul>", string.Empty);
            value = Regex.Replace(value, $"</ul>", string.Empty);

            value = Regex.Replace(value, $"<li>", "    •");
            value = Regex.Replace(value, $"</li>", Environment.NewLine);

            value = Regex.Replace(value, $"\n\n", "\n");

            //value = Regex.Replace(value, "<i([^>]*)>", string.Empty);
            //value = value.Replace("</i>", string.Empty);
            return value;
        }

        private string FindUrlImage(string url)
        {

            if (string.IsNullOrEmpty(url))
                return url;

            int position = url.IndexOf("&amp;width");
            if (position > -1)
                url = url.Substring(0, position);

            position = url.IndexOf("image=http");
            if (position > -1)
                url = url.Substring(position + 6);

            return url;
        }


        private Command _navigationCommand = new Command<string>(async (url) =>
        {
            if (url.IndexOf("/user/Profile/") > -1)
            {
                url = ".../";
                // COMANDO PARA ABRIR PROFILE
                /*
                LoginController loginController = new LoginController();
                Usuario usuario = await loginController.GetSession();
                string id = url.Replace("/user/Profile/", string.Empty);

                if (usuario?.Id == id)
                    await Application.Current.MainPage.AbrirModal(new MeuPerfilTabbedPage(id));
                else
                    await Application.Current.MainPage.AbrirModal(new NavigationPage(new UsuarioPage(id)));
                */
            }
            else
            {
                url = ".../";
                // COMANDO PARA ABRIR LINK NO NAVEGADOR
                /*
                await Launcher.OpenAsync(new Uri(url));
                */
            }
            Debug.Write(url);
        });


        private Command _publicationCommand = new Command<string>(async (url) =>
        {
            Debug.Write(url);
            //SEPARAR URL ESPECÍFICA EM UM COMANDO
            /*
            var publicationType = Funcoes.GetPublicationTypeByUrl(url);
            string id = Funcoes.GetPublicationId(url, publicationType);

            if (!id.IsNullOrEmpty())
            {
                KnowledgeController _knowledgeController = new KnowledgeController();
                ServerErrorResponseManager _serverErrorResponseManager = new ServerErrorResponseManager();

                GetFeedMainResponse.GetFeedMain response = await _knowledgeController.GetFeedItemPublication(id, true);
                if (response?.items != null)
                {
                    List<Card> _lista = await Funcoes.TratarCards(response.items);

                    if (_lista[0].BannedByAuthor)
                        await Application.Current.MainPage.AbrirModal(new NavigationPage(new PublicacaoBloqueadaPage(_lista[0])));
                    else
                        await Application.Current.MainPage.AbrirModal(new NavigationPage(new VisualizarPublicacaoPage(_lista[0], null, false, default, null)));
                }
            }
            */
        });
    }
}