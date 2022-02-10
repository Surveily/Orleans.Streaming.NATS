// <copyright file="NatsBatchContainer.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Text;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Streams;

namespace Orleans.Streaming.NATS
{
    /// <summary>
    /// Generic container for NATS events.
    /// </summary>
    [Serializable]
    public class NatsBatchContainer : IBatchContainer
    {
        /// <summary>
        /// Need to store reference to the original Message to be able to delete it later on.
        /// </summary>
        [NonSerialized]
        public Msg? Message;

        [JsonProperty]
        private readonly List<object> events;

        [JsonProperty]
        private readonly Dictionary<string, object> requestContext;

        [JsonProperty]
        private EventSequenceTokenV2? sequenceToken;

        [JsonConstructor]
        private NatsBatchContainer(Guid streamGuid, string streamNamespace, List<object> events, Dictionary<string, object> requestContext, EventSequenceTokenV2 sequenceToken)
            : this(streamGuid, streamNamespace, events, requestContext)
        {
            this.sequenceToken = sequenceToken;
        }

        private NatsBatchContainer(Guid streamGuid, string streamNamespace, List<object> events, Dictionary<string, object> requestContext)
        {
            if (events == null)
            {
                throw new ArgumentNullException("events", "Message contains no events");
            }

            this.events = events;
            this.StreamGuid = streamGuid;
            this.requestContext = requestContext;
            this.StreamNamespace = streamNamespace;
        }

        public Guid StreamGuid { get; private set; }

        public string StreamNamespace { get; private set; }

        public StreamSequenceToken? SequenceToken
        {
            get { return this.sequenceToken; }
        }

        public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
        {
            return this.events.OfType<T>().Select((e, i) => Tuple.Create<T, StreamSequenceToken>(e, this.sequenceToken!.CreateSequenceTokenForEvent(i)));
        }

        public bool ImportRequestContext()
        {
            if (this.requestContext != null)
            {
                RequestContextExtensions.Import(this.requestContext);

                return true;
            }

            return false;
        }

        public bool ShouldDeliver(IStreamIdentity stream, object filterData, StreamFilterPredicate shouldReceiveFunc)
        {
            foreach (object item in this.events)
            {
                if (shouldReceiveFunc(stream, filterData, item))
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format($"[{nameof(NatsBatchContainer)}:Stream={0},#Items={1}]", this.StreamGuid, this.events.Count);
        }

        internal static Msg ToMessage<T>(SerializationManager serializationManager, Guid streamGuid, string streamNamespace, IEnumerable<T> events, Dictionary<string, object> requestContext)
        {
            var sqsBatchMessage = new NatsBatchContainer(streamGuid, streamNamespace, events.Cast<object>().ToList(), requestContext);
            var rawBytes = serializationManager.SerializeToByteArray(sqsBatchMessage);
            var payload = new JObject();

            payload.Add("payload", JToken.FromObject(rawBytes));

            return new Msg(streamNamespace, Encoding.Default.GetBytes(payload.ToString()));
        }

        internal static NatsBatchContainer FromNatsMessage(SerializationManager serializationManager, Msg msg, long sequenceId)
        {
            var json = JObject.Parse(Encoding.Default.GetString(msg.Data));
            var payload = json["payload"];

            if (payload != null)
            {
                var sqsBatch = serializationManager.DeserializeFromByteArray<NatsBatchContainer>(payload.ToObject<byte[]>());

                sqsBatch.Message = msg;
                sqsBatch.sequenceToken = new EventSequenceTokenV2(sequenceId);

                return sqsBatch;
            }

            throw new InvalidOperationException("Payload is null");
        }
    }
}