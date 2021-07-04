using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;

namespace WebApi.AppStart
{
    public static class CustomBsonSerializers
    {
        public static void Register()
        {
            BsonSerializer.RegisterSerializer(typeof(DateTime), DateTimeSerializer.LocalInstance);
        }
    }
}
