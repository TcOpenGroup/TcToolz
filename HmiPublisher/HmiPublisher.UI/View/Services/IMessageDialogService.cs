namespace HmiPublisher.UI.View.Services
{
  public interface IMessageDialogService
  {
    MessageDialogResult ShowYesNoDialog(string title, string text,
        MessageDialogResult defaultResult = MessageDialogResult.Yes);
  }
}
