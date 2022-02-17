namespace SpeakerDiarizationSampleApp
{
    public class AmiJobData
    {
        // -> Post
        public string sessionid { get; set; }
        public string text { get; set; }
        // -> Get
        // common
        public string audio_md5 { get; set; }
        public int audio_size { get; set; }
        public string session_id { get; set; }
        public string service_id { get; set; }
        public string status { get; set; }
        //  if success
        public string utteranceid { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public AmiJobResults[] segments { get; set; }
        // if error
        public string error_message { get; set; }

    }


}
