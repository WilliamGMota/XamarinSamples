using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ViewHtml
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            string html = @"<p><p>Aqui o texto simples <strong>liguei o negrito <em>itálico <u>sublinhado </u><s><u>riscado</u></s></em></strong></p><ol><li><strong><em><s><u>Tópico 1</u></s></em></strong></li><li>tópico 2</li></ol><p>Tópico<span class=""ql-cursor""><br></span></p><ol><li>Ponto </li><li><strong>ponto</strong></li><li><em>ponto </em></li></ol><p><img  src=""https://geniews01.azurewebsites.net/GetAsset/b28af1b5-d9a1-eb11-94b3-0003ff344fbe"" height=""128"" width=""256"" style=""""></p><p><span class=""ql-blot-publication"">﻿<span contenteditable=""false""><a target=""_blank"" href=""/Publication/PostViewer/0c76978d-5101-43ef-bc45-f485a8bc0a79"" contenteditable=""false"">teste aprovacao</a></span>﻿</span></p><p>Referência em cima</p><p><strong><em><s><u><br></u></s></em></strong></p><p><strong><em><u> </u> </em></strong></p></p>";


            html = @"<p><p>Texto formatado</p><ul><li>Bullet 1</li><li>Bullet 2</li></ul><ol><li>Item 1</li><li>Item 2</li></ol><p><strong>Negrito</strong> </p><p><em>Itálico</em> </p><p><u>Sublinhado</u> </p><p><s>Riscado</s></p><p><span class='ql-blot-publication'>﻿<span contenteditable='false'><a target='_blank' href='/Publication/PostViewer/26534253-9523-49f4-8e98-08d900370dd1' contenteditable='false'>Apenas texto (longo)</a></span>﻿</span></p></p>";

            //html = @"<p>Aqui o texto simples <strong>liguei o negrito <em>itálico</em> <u>sublinhado</u> <s>riscado</s></strong></p>";

            //html = "Teste <u>sublinhado </u><s><u>riscado</u></s>";



            HtmlContent.Html = html;
            LabelTest.Text = html;

        }
    }
}
