using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.SnakeV3
{
    public class Utility
    {
        public static string GetCurrentDirectoryPath()
        {
            var path = $@"{Environment.CurrentDirectory}";
            if (path.Contains("bin\\Debug")) path = path.Replace("bin\\Debug", "");
            else if (path.Contains("bin\\Release")) path = path.Replace("bin\\Release", "");
            return path;
        }

        public static bool IsValidJson(string jsonToValidate)
        {
            if (string.IsNullOrWhiteSpace(jsonToValidate)) return false;

            jsonToValidate = jsonToValidate.Trim();
            if ((jsonToValidate.StartsWith("{") && jsonToValidate.EndsWith("}")) || //For object
                (jsonToValidate.StartsWith("[") && jsonToValidate.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(jsonToValidate);
                    return true;
                }
                catch (Exception)
                { }
            }

            return false;
        }
    }
}
