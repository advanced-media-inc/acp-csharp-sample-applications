using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using SpeakerDiarizationSampleApp.Model.Data;
using System;
using System.Threading.Tasks;
using System.Timers;
using SpeakerDiarizationSampleApp.Model;

namespace SpeakerDiarizationSampleApp.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        // -> Const 
        private const int DefMinSpeaker = 1;
        private const int DefMaxSpeaker = 10;

        // -> 
        private AppSettings appSettings;

        // -> Properties
        private ObservableCollection<AudioInfo> _audios = new();
        public ObservableCollection<AudioInfo> Audios
        {
            get => _audios;
            set => SetProperty(ref _audios, value);
        }
        private ObservableCollection<ResultInfo> _resultInfoList = new();
        public ObservableCollection<ResultInfo> ResultInfoList
        {
            get => _resultInfoList;
            set => SetProperty(ref _resultInfoList, value);
        }

        // -> Command
        public IRelayCommand SendCommand { get; }
        public IRelayCommand UpdateCommand { get; }
        public IRelayCommand OpenFolderCommand { get; }
        public IRelayCommand<object> SaveTextCommand { get; }
        public IRelayCommand<object> SelectedItemChangedCommand { get; }

        // -> 
        public MainWindowViewModel()
        {
            //
            SendCommand = new RelayCommand(SendAction);
            OpenFolderCommand = new RelayCommand(OpenFolderAction);
            UpdateCommand = new RelayCommand(UpdateAction);
            SaveTextCommand = new RelayCommand<object>(SaveTextAction);
            //
            SelectedItemChangedCommand = new RelayCommand<object>(ChangedSelectedItem);

            //
            appSettings = JsonCoder.GetAppSettings();
        }

        #region - Button Action
        private void SendAction()
        {
            int pollingTime = (appSettings != null) ? appSettings.pollingTime : 10;

            foreach (AudioInfo info in _audios)
            {
                RequestSpeechRecognization(info, (compInfo) => {
                    Timer timer = new(1000 * pollingTime); // [msec]
                    timer.Elapsed += (sender, e) =>
                    {
                        if (info.Status is "error" or "completed")
                        {
                            Debug.WriteLine("Timer stop");
                            timer.Stop();
                            return;
                        }
                        UpdateJobState(info);
                    };
                    timer.Start();
                });
            }
        }
        private void OpenFolderAction()
        {
            using var cofd = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog()
            {
                Title = "wavファイルがあるフォルダを選択してください",
                InitialDirectory = @"C:\",
                Multiselect = true
            };
            
            cofd.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("wavファイル", "*.wav"));

            if (cofd.ShowDialog() != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok) return;

            foreach(string path in cofd.FileNames)
            {
                AddAudioFilePath(path);
            }
        }
        private void UpdateAction()
        {
            foreach (AudioInfo info in _audios)
            {
                UpdateJobState(info);
            }
        }
        private void SaveTextAction(object param)
        {
            if (param is not Collection<ResultInfo> list) return;
            if (list.Count < 1) return;

            using var cofd = new Microsoft.WindowsAPICodePack.Dialogs.CommonSaveFileDialog()
            {
                Title = "名前を付けて保存",
                InitialDirectory = @"C:\"
            };
            cofd.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("テキストファイル", "*.txt"));

            if (cofd.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                string fileName = cofd.FileName;
                if (!fileName.Contains(".txt"))
                {
                    fileName += ".txt";
                }
                _ = CustomTextWriter.WriteAsync(fileName, list);
            }
        }
        #endregion

        #region - Event Handler
        private void ChangedSelectedItem(object param)
        {
            Debug.WriteLine("ChangedSelectedItem");
            if (param == null) return;
            if (param is not AudioInfo info) return;

            if (info.Status == "error")
            {
                System.Windows.MessageBox.Show($"{info.ErrorMessage}", "!!!!ERROR!!!!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (info.ResultInfoList == null) return;
            _resultInfoList.Clear();

            foreach (AudioInfo.ResultInfo ele in info.ResultInfoList)
            {
                ResultInfo info_ = new ResultInfo()
                {
                    Type = ele.Label,
                    Text = ele.Written,
                    Speaker = ele.Label
                };

                info_.TextBoxChangedCommand = new RelayCommand<object>(execute: (param) =>
                {
                    if (param is not ResultInfo info) return;
                    //Debug.WriteLine("ChangedTextBox -> Speaker: " + info.Speaker + " Type: " + info.Type);
                    foreach (ResultInfo ele in _resultInfoList)
                    {
                        if (ele.Type != info.Type) continue;
                        ele.Speaker = info.Speaker;
                    }
                });

                _resultInfoList.Add(info_);
            }
        }
        #endregion

        #region - Get Audio files
        public void GetAudioFileNamesAtPath(string path)
        {
            if (!Directory.Exists(path)) { return; }
            string[] files = Directory.GetFiles(path, "*");

            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                Debug.WriteLine("file: " + file);

                string last = file.Split("/").Last();
                if (file.Contains("\\")) 
                {
                    last = file.Split("\\").Last();
                }

                if (!last.Contains("wav")) continue;

                DateTime time = File.GetLastWriteTime(file);
                string timeStr = time.ToString("g");

                _audios.Add(new AudioInfo()
                {
                    FilePath = file,
                    Id = i,
                    Name = last,
                    Time = timeStr
                });
            }
        }
        public void AddAudioFilePath(string path)
        {
            int index = _audios.Count;

            string last = path.Split("/").Last();
            if (path.Contains("\\"))
            {
                last = path.Split("\\").Last();
            }

            if (!last.Contains("wav")) return;

            DateTime time = File.GetLastWriteTime(path);
            string timeStr = time.ToString("g");

            _audios.Add(new AudioInfo()
            {
                FilePath = path,
                Id = index,
                Name = last,
                Time = timeStr
            });
        }
        #endregion

        #region - AmiHTTP
        private void RequestSpeechRecognization(AudioInfo info, Action<AudioInfo> completeAction)
        {
            if (info.SessionId != null) return;
            if (info.Status == "sending") return;
            info.Status = "sending";

            string appKey = (appSettings != null) ? appSettings.appKey : "";

            // -> POST
            _ = AmiHTTP.RequestSpeechRecog(info.FilePath, appKey, GetDValue(info), (result) =>
            {
                Debug.WriteLine("DONE: RequestSpeechRecog");
                // 
                if (result.error != null)
                {
                    Debug.WriteLine(result.error);
                    info.ErrorMessage = result.error;
                    info.Status = "error";
                    return;
                }

                object json = JsonSerializer.Deserialize(result.success, typeof(AmiJobData));
                AmiJobData res = (AmiJobData)json;

                info.SessionId = res.sessionid;
                info.Status = "send";

                completeAction(info);
            });
        }
        private void UpdateJobState(AudioInfo info)
        {
            if (info.SessionId == null) { return; }
            if (info.Status is "error" or "completed") { return; }

            // -> 
            GetJobState(info);
        }
        private void GetJobState(AudioInfo info)
        {
            string appKey = (appSettings != null) ? appSettings.appKey : "";

            _ = AmiHTTP.GetJobState(info.SessionId, appKey, (result) =>
            {
                Debug.WriteLine("DONE: GetJobState");
                // 
                if (result.error != null)
                {
                    Debug.WriteLine(result.error);
                    if (info.ErrorCount >= 3)
                    {
                        info.Status = "error";
                        info.ErrorMessage = result.error;
                        return;
                    }
                    info.ErrorCount++;
                    return;
                }

                object json = JsonSerializer.Deserialize(result.success, typeof(AmiJobData));
                AmiJobData job = (AmiJobData)json;
                info.Status = job.status;
                Debug.Write("result.success: " + result.success);
                if (info.Status == "error")
                {
                    info.ErrorMessage = job.error_message;
                }

                if (job.segments != null)
                {
                    info.Segments = job.segments;
                }
            });
        }
        private string GetDValue(AudioInfo info)
        {
            int min = int.TryParse(info.MinSpeaker, out int num_min) ? num_min : DefMinSpeaker;
            int max = int.TryParse(info.MaxSpeaker, out int num_max) ? num_max : DefMaxSpeaker;

            if (min < 0 || min > 10)
            {
                min = DefMinSpeaker;
            }

            if (max < 0 || max > 10)
            {
                max = DefMaxSpeaker;
            }

            if (min > max)
            {
                min = DefMinSpeaker;
                max = DefMaxSpeaker;
            }

            info.MinSpeaker = min.ToString();
            info.MaxSpeaker = max.ToString();

            string grammar = (appSettings != null) ? appSettings.grammarFileNames : "-a-general";
            
            string dValue = "grammarFileNames=" +Uri.EscapeDataString(grammar);
            dValue += " speakerDiarization=" + Uri.EscapeDataString("True");
            dValue += " diarizationMinSpeaker=" + Uri.EscapeDataString(info.MinSpeaker) + " " + "diarizationMaxSpeaker=" + Uri.EscapeDataString(info.MaxSpeaker);
            //Debug.WriteLine(dValue);
            return dValue;
        }
        #endregion
    }
}
