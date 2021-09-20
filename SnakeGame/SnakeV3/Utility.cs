using System;
using System.Collections.Generic;
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
    }
}
