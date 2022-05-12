using HmiPublisher.Model;
using Microsoft.Practices.Prism.PubSubEvents;

namespace HmiPublisher.UI.Events
{
    public class OpenRemoteEditViewEvent : PubSubEvent<int> { }
    public class PublishSingleEvent : PubSubEvent<int> { }
    public class RemoteDeletedEvent : PubSubEvent<int> { }
    public class RemoteSavedEvent : PubSubEvent<Remote> { }
}
