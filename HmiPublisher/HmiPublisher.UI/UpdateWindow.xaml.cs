using Libs.Update;
using System.Windows;

namespace HmiPublisher
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {

        public UpdateWindow()
        {
            InitializeComponent();
            this.DataContext = new UpdateModel();
        }
    
    }
}
