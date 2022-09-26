using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuickConnect
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public ItemWrapper Item;
        public EditWindow(ItemWrapper item)
        {
            InitializeComponent();
            DataContext = item;

            Title = item?.Name ?? "";

            PreviewKeyDown += new KeyEventHandler(HandleEsc);

            Name.Focus();

            Topmost = true;
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Item = DataContext as ItemWrapper;
            Close();
        }

        private void Name_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
    }

    public class RichTextBoxHelper : DependencyObject
    {
        public static string GetDocumentXaml(DependencyObject obj) { return (string)obj.GetValue(DocumentXamlProperty); }

        public static void SetDocumentXaml(DependencyObject obj,
                                           string value)
        {
            obj.SetValue(DocumentXamlProperty, value);
        }

        public static readonly DependencyProperty DocumentXamlProperty = DependencyProperty.RegisterAttached
        (
            "DocumentXaml",
            typeof(string),
            typeof(RichTextBoxHelper),
            new FrameworkPropertyMetadata
            {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (obj,
                                           e) =>
                {
                    var richTextBox = (RichTextBox)obj;
                    var xaml = GetDocumentXaml(richTextBox);
                    Stream sm = new MemoryStream(Encoding.UTF8.GetBytes(xaml));
                    richTextBox.Document = (FlowDocument)XamlReader.Load(sm);
                    sm.Close();
                }
            }
        );
    }
}
