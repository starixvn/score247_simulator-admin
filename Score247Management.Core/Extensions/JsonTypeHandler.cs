namespace Score247Management.Core.Extensions
{
    using System;
    using System.Data;
    using Dapper;
    using JsonNet.ContractResolvers;
    using Newtonsoft.Json;

    public class JsonTypeHandler : SqlMapper.ITypeHandler
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new PrivateSetterContractResolver()
        };

        public void SetValue(IDbDataParameter parameter, object value)
        {
            parameter.Value = JsonConvert.SerializeObject(value);
        }

        public object Parse(Type destinationType, object value)
        {
            var data = JsonConvert.DeserializeObject(value as string, destinationType, settings);

            return data;
        }
    }
}