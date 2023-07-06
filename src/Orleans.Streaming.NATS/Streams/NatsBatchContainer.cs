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

namespace Orleans.Streaming.NATS.Streams
{
    /// <summary>
    /// Generic container for NATS events.
    /// </summary>
    [Serializable]
    [GenerateSerializer]
    public class NatsBatchContainer : IBatchContainer
    {
        /// <summary>
        /// Need to store reference to the original Message to be able to delete it later on.
        /// </summary>
        [NonSerialized]
        public Msg? Message;

        [JsonProperty]
        [Id(1)]
        private readonly List<object> _events;

        [JsonProperty]
        [Id(2)]
        private readonly Dictionary<string, object> _requestContext;

        [JsonProperty]
        [Id(0)]
        private EventSequenceTokenV2 _sequenceToken;

        [JsonConstructor]
        private NatsBatchContainer(StreamId streamId,
                                   List<object> events,
                                   Dictionary<string, object> requestContext,
                                   EventSequenceTokenV2 sequenceToken)
            : this(streamId, events, requestContext)
        {
            _sequenceToken = sequenceToken;
        }

        private NatsBatchContainer(StreamId streamId,
                                   List<object> events,
                                   Dictionary<string, object> requestContext)
        {
            if (events == null)
            {
                throw new ArgumentNullException("events", "Message contains no events");
            }

            StreamId = streamId;
            _events = events;
            _requestContext = requestContext;
        }

        [Id(3)]
        public StreamId StreamId { get; }

        public StreamSequenceToken SequenceToken => _sequenceToken;

        public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
        {
            return _events.OfType<T>().Select((e, i) => Tuple.Create<T, StreamSequenceToken>(e, _sequenceToken.CreateSequenceTokenForEvent(i)));
        }

        public bool ImportRequestContext()
        {
            if (_requestContext != null)
            {
                RequestContextExtensions.Import(_requestContext);
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format($"[{nameof(NatsBatchContainer)}:Stream={0},#Items={1}]", StreamId, _events.Count);
        }

        internal static Msg ToMessage<T>(Serializer<NatsBatchContainer> serializer, StreamId streamId, IEnumerable<T> events, Dictionary<string, object> requestContext)
        {
            var batchMessage = new NatsBatchContainer(streamId, events.Cast<object>().ToList(), requestContext);
            var rawBytes = serializer.SerializeToArray(batchMessage);
            var payload = new JObject();

            payload.Add("payload", JToken.FromObject(rawBytes));

            return new Msg(Encoding.Default.GetString(streamId.Namespace.ToArray()), Encoding.Default.GetBytes(payload.ToString()));
        }

        internal static NatsBatchContainer FromNatsMessage(Serializer<NatsBatchContainer> serializer, Msg msg, long sequenceId)
        {
            var json = JObject.Parse(Encoding.Default.GetString(msg.Data));
            var payload = json["payload"];

            if (payload != null)
            {
                var data = payload.ToObject<byte[]>();
                var batch = serializer.Deserialize(data);

                batch.Message = msg;
                batch._sequenceToken = new EventSequenceTokenV2(sequenceId);

                return batch;
            }

            throw new InvalidOperationException("Payload is null");
        }
    }
}