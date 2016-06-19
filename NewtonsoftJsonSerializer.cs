﻿using RestSharp.Serializers;

namespace cc.newspring.CacheBreak.Utilities
{
    public class NewtonsoftJsonSerializer : ISerializer
    {
        public NewtonsoftJsonSerializer(ISerializer serializer)
        {
            ContentType = serializer.ContentType;
            DateFormat = serializer.DateFormat;
            Namespace = serializer.Namespace;
            RootElement = serializer.RootElement;
        }

        public string ContentType { get; set; }

        public string DateFormat { get; set; }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        public string Serialize(object obj)
        {
            var json = string.Empty;
            var group = obj as Rock.Model.Group;

            if (group != null)
            {
                // Groups have a circular reference. Something like this is problematic for the serializer because it is infinite: group.GroupType.Groups[n] == group
                var temp = group.GroupType;
                group.GroupType = null;
                json = Newtonsoft.Json.JsonConvert.SerializeObject(group);
                group.GroupType = temp;
            }
            else
            {
                json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }

            return json;
        }
    }
}