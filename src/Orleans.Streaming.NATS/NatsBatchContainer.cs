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

        /// <summary>
        /// List of events.
        /// </summary>
        [JsonProperty]
        private readonly List<object> events;

        /// <summary>
        /// Generic context of the request.
        /// </summary>
        [JsonProperty]
        private readonly Dictionary<string, object> requestContext;

        /// <summary>
        /// The sequence token for the events batch.
        /// </summary>
        [JsonProperty]
        private EventSequenceTokenV2? sequenceToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="NatsBatchContainer"/> class.
        /// </summary>
        /// <param name="streamGuid">ID of the stream.</param>
        /// <param name="streamNamespace">Namespace of the stream.</param>
        /// <param name="events">List of events.</param>
        /// <param name="requestContext">Context of the request.</param>
        /// <param name="sequenceToken">Sequence token of the message.</param>
        [JsonConstructor]
        private NatsBatchContainer(
            Guid streamGuid,
            string streamNamespace,
            List<object> events,
            Dictionary<string, object> requestContext,
            EventSequenceTokenV2 sequenceToken)
            : this(streamGuid, streamNamespace, events, requestContext)
        {
            this.sequenceToken = sequenceToken;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NatsBatchContainer"/> class.
        /// </summary>
        /// <param name="streamGuid">ID of the stream.</param>
        /// <param name="streamNamespace">Namespace of the stream.</param>
        /// <param name="events">List of events.</param>
        /// <param name="requestContext">Context of the request.</param>
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

        /// <summary>
        /// Gets the ID of the stream.
        /// </summary>
        /// <value>ID of the stream.</value>
        public Guid StreamGuid { get; private set; }

        /// <summary>
        /// Gets the namespace of the stream.
        /// </summary>
        /// <value>Namespace of the stream.</value>
        public string StreamNamespace { get; private set; }

        /// <summary>
        /// Gets the sequence token for the events batch.
        /// </summary>
        /// <value>The sequence token for the events batch.</value>
        public StreamSequenceToken? SequenceToken
        {
            get { return this.sequenceToken; }
        }

        /// <summary>
        /// Gets the events of the message.
        /// </summary>
        /// <typeparam name="T">Contract type.</typeparam>
        /// <returns>List of contract type.</returns>
        public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
        {
            return this.events.OfType<T>().Select((e, i) => Tuple.Create<T, StreamSequenceToken>(e, this.sequenceToken!.CreateSequenceTokenForEvent(i)));
        }

        /// <summary>
        /// Imports the request context.
        /// </summary>
        /// <returns>Whether import occurred.</returns>
        public bool ImportRequestContext()
        {
            if (this.requestContext != null)
            {
                RequestContextExtensions.Import(this.requestContext);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Predicate for delivery.
        /// </summary>
        /// <param name="stream">ID and namespace of the stream.</param>
        /// <param name="filterData">Predicate data.</param>
        /// <param name="shouldReceiveFunc">Predicate action.</param>
        /// <returns>Whether a message should be delivered to a stream.</returns>
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

        /// <summary>
        /// Renders a display message.
        /// </summary>
        /// <returns>Message.</returns>
        public override string ToString()
        {
            return string.Format($"[{nameof(NatsBatchContainer)}:Stream={0},#Items={1}]", this.StreamGuid, this.events.Count);
        }

        /// <summary>
        /// Serialize to NATS message.
        /// </summary>
        /// <param name="serializationManager">Orleans serialization manager.</param>
        /// <param name="streamGuid">ID of the stream.</param>
        /// <param name="streamNamespace">Namespace of the stream.</param>
        /// <param name="events">List of events.</param>
        /// <param name="requestContext">Context of the request.</param>
        /// <typeparam name="T">Contract type.</typeparam>
        /// <returns>NATS message.</returns>
        internal static Msg ToMessage<T>(
            SerializationManager serializationManager,
            Guid streamGuid,
            string streamNamespace,
            IEnumerable<T> events,
            Dictionary<string, object> requestContext)
        {
            var sqsBatchMessage = new NatsBatchContainer(streamGuid, streamNamespace, events.Cast<object>().ToList(), requestContext);
            var rawBytes = serializationManager.SerializeToByteArray(sqsBatchMessage);
            var payload = new JObject();

            payload.Add("payload", JToken.FromObject(rawBytes));

            return new Msg(streamNamespace, Encoding.Default.GetBytes(payload.ToString()));
        }

        /// <summary>
        /// Deserialize from NATS message.
        /// </summary>
        /// <param name="serializationManager">Orleans serialization manager.</param>
        /// <param name="msg">NATS message.</param>
        /// <param name="sequenceId">Sequence token of the message.</param>
        /// <returns>Deserialized Batch Container.</returns>
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