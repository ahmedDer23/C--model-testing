namespace Linedata.Amp.Model.Automation.Helpers
{
    using System;
    using System.Net;
    using ReactiveDomain.EventStore;
    using ReactiveDomain.Foundation;

    public static class EventStoreHelper
    {
        public static Func<string, IListener> GetListener { get; }

        public static Func<IRepository> GetRepository { get; }

        static EventStoreHelper()
        {
            var settings = CommonHelper.GetConfig().GetSection("eventStore");

            var userCredentials = new ReactiveDomain.UserCredentials(
                username: settings["ESUserId"],
                password: settings["ESPassword"]);

            var eventStoreLoader = new EventStoreLoader();
            eventStoreLoader.Connect(
                credentials: userCredentials,
                server: IPAddress.Parse(settings["ESipAddress"]),
                tcpPort: int.Parse(settings["ESTcpPort"]));

            var streamNameBuilder = new PrefixedCamelCaseStreamNameBuilder(Constants.DomainPrefix);

            GetListener = name => new StreamListener(
                listenerName: name,
                eventStoreConnection: eventStoreLoader.Connection,
                streamNameBuilder: streamNameBuilder,
                serializer: new JsonMessageSerializer());

            GetRepository = () => new StreamStoreRepository(
                streamNameBuilder: streamNameBuilder,
                eventStoreConnection: eventStoreLoader.Connection,
                eventSerializer: new JsonMessageSerializer());
        }
    }
}
