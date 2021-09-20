using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.SnakeV3
{
    public class ReplayDTO
    {
        private const string DIRECTORY_NAME = "SavedReplays";

        public int Height { get; private set; }
        public int Width { get; private set; }
        public List<(int x, int y)> ReplayBody { get; private set; }
        public List<(int x, int y)> ReplayFood { get; private set; }

        public ReplayDTO(int height, int width)
        {
            Height = height;
            Width = width;
            ReplayBody = new List<(int x, int y)>();
            ReplayFood = new List<(int x, int y)>();
        }

        public static ReplayDTO LoadReplay(string replayFileName)
        {
            if (!replayFileName.EndsWith(".txt")) replayFileName += ".txt";

            string path = Utility.GetCurrentDirectoryPath();
            var directoryCombine = Path.Combine(path, DIRECTORY_NAME);
            if (!Directory.Exists(directoryCombine)) return null;

            var fileCombine = Path.Combine(directoryCombine, replayFileName);
            if (!File.Exists(fileCombine)) return null;

            var replayFile = File.ReadAllText(fileCombine);
            if (!Utility.IsValidJson(replayFile)) return null;

            ReplayDTO replay = JsonConvert.DeserializeObject<ReplayDTO>(replayFile);
            return replay;
        }

        public void SaveReplay(int score)
        {
            string replayBody = JsonConvert.SerializeObject(this);
            string path = Utility.GetCurrentDirectoryPath();
            string directoryCombine = Path.Combine(path, DIRECTORY_NAME);
            if (!Directory.Exists(directoryCombine)) Directory.CreateDirectory(directoryCombine);

            string fileName = $"Score {score} {DateTime.Now.ToShortDateString()}.txt";
            string fileCombine = Path.Combine(directoryCombine, fileName);
            if (File.Exists(fileCombine))
            {
                string[] files = Directory.GetFiles(directoryCombine);
                int count = files.Count(file => file.Contains(fileName));
                int dotIndex = fileName.IndexOf(".");
                fileName = fileName.Insert(dotIndex, count.ToString());
                fileCombine = Path.Combine(directoryCombine, fileName);
            }

            FileStream stream = File.Create(fileCombine);
            stream.Close();
            File.WriteAllText(fileCombine, replayBody);
        }
    }
}
