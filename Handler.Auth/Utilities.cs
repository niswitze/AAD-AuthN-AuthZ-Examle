using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Handler.AuthN
{
    public class Utilities
    {
        public static void ConvertToDictionary<T>(T convertObject, out IDictionary<string, string> appSettings)
        {
            var json = JsonConvert.SerializeObject(convertObject);
            appSettings = JsonConvert.DeserializeObject<IDictionary<string, string>>(json);
        }
    }
}
