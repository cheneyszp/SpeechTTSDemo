using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SpeechTTSDemo.Common;
using SpeechTTSDemo.Views;
using Windows.Media.SpeechSynthesis;
using Windows.ApplicationModel.Resources.Core;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SpeechTTSDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer timer;

        private ResourceContext speechContext;
        private SpeechSynthesizer synthesizer;
        private string _command = String.Empty;

        //mapping info
        private Dictionary<string, Type> pageInformation = new Dictionary<string, Type>();  

        public List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="开户", ClassType=typeof(NewAccount)},
            new Scenario() { Title="查询", ClassType=typeof(Audit)},
            new Scenario() { Title="转账", ClassType=typeof(Transfer)},
            new Scenario() { Title="理财产品", ClassType=typeof(Finace)}
        };
        public List<Scenario> Scenarios
        {
            get { return this.scenarios; }
        }
        public MainPage()
        {
            this.InitializeComponent();          
            pageInformation.Add(ResourceHelper.GetString("account page"), typeof(NewAccount));
            pageInformation.Add(ResourceHelper.GetString("audit page"), typeof(Audit));
            pageInformation.Add(ResourceHelper.GetString("finace page"), typeof(Finace));
            pageInformation.Add(ResourceHelper.GetString("transfer page"), typeof(Transfer));


        }

        private void InitTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Tick += TimerTick;
            timer.Start();
        }

        private async void TimerTick(Object sender, Object arg)
        {
            timer.Stop();
            statusText.Text = ResourceHelper.GetString("waiting for");
            //处理语音以及TTS逻辑
            _command = await SpeechTTSHelper.RecognizeVoiceCommand();
            Type t = null;
            foreach(KeyValuePair<string,Type> item in pageInformation)
            {
                if(item.Key.Equals(_command))
                {
                    t = item.Value;
                    break;
                }
            }
            if(null != t && pageInformation.ContainsKey(_command))
            {
                PlayTTS("正在" + _command);
                foreach (var item in Scenarios)
                {
                    if(t == item.ClassType)
                    {
                        int index = Scenarios.IndexOf(item);
                        listBox.SelectedIndex = index;
                        break;
                    }
                }
            }
            else
            {
                statusText.Text = _command;
                timer.Start();
            }
                   
        }
    
        private async void PlayTTS(string message)
        {

            speechContext = ResourceContext.GetForCurrentView();
            speechContext.Languages = new string[] { SpeechSynthesizer.DefaultVoice.Language };
            synthesizer = new SpeechSynthesizer();
            var voices = SpeechSynthesizer.AllVoices;

            VoiceInformation currentVoice = synthesizer.Voice;
            VoiceInformation voice = null;
            foreach (VoiceInformation item in voices.OrderBy(p => p.Language))
            {
                string tag = item.Language;
                if(tag.Equals("zh-CN") && item.Gender == VoiceGender.Female)
                {
                    voice = item;
                }
            }
            if(null != voice)
            {
                synthesizer.Voice = voice;
                SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(message);

                media.AutoPlay = true;
                media.SetSource(synthesisStream, synthesisStream.ContentType);
                media.Play();
            }
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            listBox.ItemsSource = scenarios;
            textBlock.Text = "底部信息提示为等待语音命令时对麦克风输入语音命令，命令示例:转到开户页面 转到查询页面\n转到理财产品页面 转到转账页面";
            //初始化并且启动timer
            listBox.SelectedIndex = 0;
            InitTimer();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox scenarioListBox = sender as ListBox;
            Scenario s = scenarioListBox.SelectedItem as Scenario;
            if (s != null)
            {
                ScenarioFrame.Navigate(s.ClassType);
            }
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {          
            statusText.Text = "已经"+_command;          
            timer.Start();
        }
    }

    public class ScenarioBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Scenario s = value as Scenario;
            return s.Title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return true;
        }
    }
}
