namespace SpeakerDiarizationSampleApp
{
    public class AmiJobResults
    {
        public string text { get; set; }
        public Results[] results { get; set; }

        // -> 
        public class Results
        {
            public double confidence { get; set; }
            public int starttime { get; set; }
            public int endtime { get; set; }
            public string rulename { get; set; }
            public int[] tags { get; set; }
            public string text { get; set; }
            public Tokens[] tokens { get; set; }

            public class Tokens
            {
                public double confidence { get; set; }
                public int starttime { get; set; }
                public int endtime { get; set; }
                public string spoken { get; set; }
                public string written { get; set; }
                public string label { get; set; }
            }
        }
    }
}
