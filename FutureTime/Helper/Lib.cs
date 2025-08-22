using Library.Data;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace FutureTime.Helper
{
    public static class Lib
    {
        public static BsonRegularExpression _BsonRegularExpression(string data, string options)
        {
            string pattern = Regex.Escape(data); // no ^ and $
            return new BsonRegularExpression(pattern, options);
        }
    }
}
