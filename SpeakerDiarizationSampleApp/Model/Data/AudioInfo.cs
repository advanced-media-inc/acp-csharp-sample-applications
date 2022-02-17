using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows;

namespace SpeakerDiarizationSampleApp.Model.Data
{
    public class AudioInfo : ObservableObject
    {
        // -> Properties for View
        #region - Id
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        #endregion

        #region - Name
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        #endregion

        #region - Time
        private string _time;
        public string Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }
        #endregion

        #region - Status
        private string _status;
        public string Status
        {
            get => _status;
            //set => SetProperty(ref _status, value);
            set
            {
                SetProperty(ref _status, value);
                ChangeStatusBGColor(value);
            }
        }
        #endregion

        #region - StatusBGColor
        private Brush _statusBGColor = new SolidColorBrush(Colors.Silver);
        public Brush StatusBGColor
        {
            get => _statusBGColor;
            set => SetProperty(ref _statusBGColor, value);
        }
        #endregion

        #region - MinSpeaker
        private string _minSpeaker = "1";
        public string MinSpeaker
        {
            get => _minSpeaker;
            set => SetProperty(ref _minSpeaker, value);
        }
        #endregion

        #region - MaxSpeaker
        private string _maxSpeaker = "10";
        public string MaxSpeaker
        {
            get => _maxSpeaker;
            set => SetProperty(ref _maxSpeaker, value);
        }
        #endregion

        // -> Only stored
        public string FilePath { get; set; }
        public string SessionId { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCount { get; set; }
        public List<ResultInfo> ResultInfoList { get; set; }
        public AmiJobResults[] Segments
        {
            set
            {
                List<ResultInfo> jobs = new();

                foreach (AmiJobResults segment in value)
                {
                    foreach (AmiJobResults.Results jobResult in segment.results)
                    {
                        foreach (AmiJobResults.Results.Tokens token in jobResult.tokens)
                        {
                            ResultInfo job_ = new();
                            job_.Label = token.label;
                            job_.Written = token.written;

                            if (jobs.Count < 1)
                            {
                                jobs.Add(job_);
                                continue;
                            }

                            ResultInfo lastJob = jobs.Last();

                            if (lastJob.Label == token.label)
                            {
                                jobs.Last().Written += token.written;
                            }
                            else
                            {
                                jobs.Add(job_);
                            }
                        }
                    }
                }
                ResultInfoList = jobs;
            }
        }

        // -> Constructa
        public AudioInfo() {
            _id = -1;
            _name = "";
            _time = "";
            _status = "not send";
        }

        public class ResultInfo
        {
            public string Label { get; set; } = "Speaker0";
            public string Written { get; set; } = "";
        }

        private void ChangeStatusBGColor(string status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Color color = Colors.Silver;
                if (status is "no send" or "sending" or "send")
                {
                    color = Colors.Silver;
                }
                else if (status is "queued" or "started" or "processing")
                {
                    color = Colors.DarkGray;
                }
                else if (status is "completed")
                {
                    color = Colors.DimGray;

                }
                else if (status is "error")
                {
                    color = Colors.DarkRed;
                }
                StatusBGColor = new SolidColorBrush(color);
            });
        }

    }
}
