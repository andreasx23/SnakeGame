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

        private static string _clearBuffer = null; // Clear this if window size changes
        private static int _windowWidth = -1, _windowHeight = -1;
        public static void ClearConsole()
        {
            if (_clearBuffer == null)
            {
                SetClearBuffer();
            }
            else
            {
                if (_windowWidth != Console.WindowWidth || _windowHeight != Console.WindowHeight)
                {
                    Console.Clear();
                    SetClearBuffer();
                }
            }

            Console.SetCursorPosition(0, 0);
            Console.Write(_clearBuffer);
            Console.SetCursorPosition(0, 0);
        }

        private static void SetClearBuffer()
        {
            _windowWidth = Console.WindowWidth;
            _windowHeight = Console.WindowHeight;
            var line = string.Empty.PadLeft(_windowWidth + 2, ' ');
            var lines = new StringBuilder();

            for (var i = 0; i < _windowHeight + 2; i++) 
                lines.AppendLine(line);

            _clearBuffer = lines.ToString();
        }
    }
}
