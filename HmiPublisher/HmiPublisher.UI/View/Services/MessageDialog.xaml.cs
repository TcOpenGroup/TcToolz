using System.Windows;
using System.Windows.Controls;

namespace HmiPublisher.UI.View.Services
{
  public partial class MessageDialog : Window
  {
    private MessageDialogResult _result;

    public MessageDialog(string title, string text, MessageDialogResult defaultResult, params MessageDialogResult[] buttons)
    {
      InitializeComponent();
      Title = title;
      textBlock.Text = text;
      InitializeButtons(buttons);
      _result = defaultResult;
    }

    private void InitializeButtons(MessageDialogResult[] buttons)
    {
      if (buttons == null || buttons.Length == 0)
      {
        buttons = new[] { MessageDialogResult.Ok };
      }
      foreach (var button in buttons)
      {
        var btn = new Button { Content = button, Tag = button };
        ButtonsPanel.Children.Add(btn);
        btn.Click += ButtonClick;
      }
    }

    private void ButtonClick(object sender, RoutedEventArgs e)
    {
      var button = e.Source as Button;
      if (button != null)
      {
        _result = (MessageDialogResult)button.Tag;
        this.Close();
      }
    }

    public new MessageDialogResult ShowDialog()
    {
      base.ShowDialog();
      return _result;
    }
  }
}
