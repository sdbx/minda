using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Models.Events
{
    class EventConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Event).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken obj = JToken.Load(reader);
            string discriminator = (string)obj["type"];

            Event item;
            switch (discriminator)
            {
                case "connected":
                    item = new ConnectedEvent();
                    break;

                case "started":
                    item = new GameStartedEvent();
                    break;

                case "entered":
                    item = new EnteredEvent();
                    break;

                case "moved":
                    item = new MoveEvent();
                    break;

                case "left":
                    item = new LeftEvent();
                    break;
                case "confed":
                    item = new ConfedEvent();
                    break;
                case "error":
                    item = new ErrorEvent();
                    break;
                case "ticked":
                    item = new TickedEvent();
                    break;
                case "ended":
                    item = new EndedEvent();
                    break;
                case "chated":
                    item = new ChattedEvent();
                    break;
                default:
                    throw new NotImplementedException();
            }

            serializer.Populate(obj.CreateReader(), item);

            return item;

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

        }
    }
}