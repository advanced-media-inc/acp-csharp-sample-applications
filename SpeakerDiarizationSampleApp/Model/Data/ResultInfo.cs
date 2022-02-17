using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace SpeakerDiarizationSampleApp.Model.Data
{
    public class ResultInfo : ObservableObject
    {
        #region - Type (Label)
        private string _type = "Speaker0";
        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
        #endregion

        #region - Speaker
        private string _speaker = "";
        public string Speaker
        {
            get => _speaker;
            set => SetProperty(ref _speaker, value);
        }
        #endregion
        #region - Text
        private string _text = "";
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
        #endregion

        public Microsoft.Toolkit.Mvvm.Input.IRelayCommand<object> TextBoxChangedCommand { get; set; }
    }
}
