using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using static Dapper.SqlMapper;

namespace Library.Parameters
{
    public class JsonParameter : ICustomQueryParameter
    {
        private readonly string _value;

        public JsonParameter(string value)
        {
            if (value == null)
                _value = "{}";
            else
                _value = value;
        }

        public JsonParameter(object obj)
        {
            if(obj==null)
                _value = "{}";
            else
                _value = JsonConvert.SerializeObject(obj);
        }

        public void AddParameter(IDbCommand command, string name)
        {
            var parameter = new NpgsqlParameter(name, NpgsqlDbType.Json);
            parameter.Value = _value;

            command.Parameters.Add(parameter);
        }
    }
}
