using System;
using System.IO;
using Newtonsoft.Json;

namespace SpeakerDiarizationSampleApp.Model
{
    public class JsonCoder
    {
        public AppSettings settings;
        private static string path_ = "./appSettings.json";


        public static AppSettings GetAppSettings()
        {
            AppSettings settings_;

            if (!File.Exists(path_)) return null;

            try
            {
                using StreamReader file = File.OpenText(path_);
                JsonSerializer serializer = new();
                settings_ = (AppSettings)serializer.Deserialize(file, typeof(AppSettings));
            }
            catch
            {
                return null;
            }

            return settings_;
        }
    }

    public class AppSettings
    {
        public string appKey { get; set; }
        public string grammarFileNames { get; set; }
        public int pollingTime { get; set; }
        public AppSettings()
        {
            appKey = "";
            grammarFileNames = "-a-general";
            pollingTime = 10;
        }
    }
}
