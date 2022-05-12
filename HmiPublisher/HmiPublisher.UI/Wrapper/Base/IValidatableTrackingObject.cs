using System.ComponentModel;

namespace HmiPublisher.UI.Wrapper
{
    public interface IValidatableTrackingObject : IRevertibleChangeTracking, INotifyPropertyChanged
    {
        bool IsValid { get; }
    }
}
