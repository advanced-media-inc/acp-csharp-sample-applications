using SpeakerDiarizationSampleApp.Model.Data;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace SpeakerDiarizationSampleApp.Model
{
    class CustomTextWriter
    {
        public static async Task WriteAsync(string path, Collection<ResultInfo> list)
        {
            using StreamWriter file = new(path);

            foreach (ResultInfo info in list)
            {
                string speaker = (info.Speaker == null || info.Speaker.Length < 1) ? "No Name" : info.Speaker;
                await file.WriteLineAsync("[ " + speaker + " ]\n" + info.Text + "\n");
            }
        }
    }
}
