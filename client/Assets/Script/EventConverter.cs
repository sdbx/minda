using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            case "gamestart":
                item = new GameStartEvent();
                break;

            case "enter":
                item = new EnterEvent();
                break;

            case "move":
                item = new MoveEvent();
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