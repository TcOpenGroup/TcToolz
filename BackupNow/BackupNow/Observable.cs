
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BackupNow
{
    public class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {

                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}