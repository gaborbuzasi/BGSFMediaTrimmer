using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Input;
using System.Diagnostics;
using System.Text;

namespace BGSFMediaTrimmer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static DispatcherTimer uiUpdate = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private double OriginalWaveFormWidth { get; set; }
        private float OriginalChannelFreq = 0f;
        private Slider updateSlider { get; set; }
        private double cutInTime { get; set; }
        private double cutOutTime { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            drpZoom.DisplayMemberPath = "Text";
            drpZoom.SelectedValuePath = "Value";
            var items = new[]
            {
                new { Text = "x1", Value = 1},
                new { Text = "x2", Value = 2},
                new { Text = "x4", Value = 4},
                new { Text = "x8", Value = 8},
                new { Text = "x16", Value = 16},
                new { Text = "x32", Value = 32},
            };
            drpZoom.ItemsSource = items;
            
            uiUpdate.Interval = TimeSpan.FromMilliseconds(100);
            uiUpdate.Tick += UiUpdate_Tick;
            BassEngine bassEngine = BassEngine.Instance;

            
            OriginalWaveFormWidth = waveformTimeline.Width;
            updateSlider = sliAudio;

            UIHelper.Bind(bassEngine, "CanStop", StopButton, Button.IsEnabledProperty);
            UIHelper.Bind(bassEngine, "CanPlay", PlayButton, Button.IsEnabledProperty);
            UIHelper.Bind(bassEngine, "CanPause", PauseButton, Button.IsEnabledProperty);
            UIHelper.Bind(bassEngine, "CanPlay", btnPlay, Button.IsEnabledProperty);
            UIHelper.Bind(bassEngine, "CanPause", btnPause, Button.IsEnabledProperty);

            waveformTimeline.RegisterSoundPlayer(bassEngine);

        }

        private void UiUpdate_Tick(object sender, EventArgs e)
        {
            txtRemain.Text = Un4seen.Bass.Utils.FixTimespan(BassEngine.Instance.ChannelLength - BassEngine.Instance.ChannelPosition, "HHMMSS");
            if (updateSlider == sliAudio)
            {
                sliAudio.Value = BassEngine.Instance.ChannelPosition;
                txtCrtIn.Text = Un4seen.Bass.Utils.FixTimespan(BassEngine.Instance.ChannelPosition, "HHMMSSFF");
            }
            else if (updateSlider == sliAudio2)
            {
                sliAudio2.Value = BassEngine.Instance.ChannelPosition;
                txtCrtOut.Text = Un4seen.Bass.Utils.FixTimespan(BassEngine.Instance.ChannelPosition, "HHMMSSFF");
                
            }

            if (waveScroll.HorizontalOffset > waveformTimeline.progressLine.Margin.Left || (waveScroll.HorizontalOffset + OriginalWaveFormWidth) < waveformTimeline.progressLine.Margin.Left)
            {
                waveScroll.ScrollToHorizontalOffset(waveformTimeline.progressLine.Margin.Left);
            }

            float value = 0f;
            Un4seen.Bass.Bass.BASS_ChannelGetAttribute(BassEngine.Instance.ActiveStreamHandle, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_FREQ, ref value);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (BassEngine.Instance.CanPlay)
                BassEngine.Instance.Play();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (BassEngine.Instance.CanPause)
                BassEngine.Instance.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (BassEngine.Instance.CanStop)
                BassEngine.Instance.Stop();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (BassEngine.Instance.CanPlay)
                BassEngine.Instance.Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (BassEngine.Instance.CanPause)
                BassEngine.Instance.Pause();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Filter = "(*.ac3,*.mpa,*.mp2,*.mp3,*.m4a)|*.ac3;*.mpa;*.mp2;*.mp3;*.m4a";
            if (openDialog.ShowDialog() == true)
            {
                BassEngine.Instance.OpenFile(openDialog.FileName, (bool)rdbWave.IsChecked);
                updateSlider = sliAudio;
                sliAudio.Maximum = BassEngine.Instance.ChannelLength;
                sliAudio2.Maximum = BassEngine.Instance.ChannelLength;
                sliAudio.Background = Brushes.Green;
                sliAudio2.Background = Brushes.Gray;
                FileText.Text = openDialog.FileName;
                Un4seen.Bass.Bass.BASS_ChannelGetAttribute(BassEngine.Instance.ActiveStreamHandle, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_FREQ, ref this.OriginalChannelFreq);
                Un4seen.Bass.Bass.BASS_ChannelSetAttribute(BassEngine.Instance.ActiveStreamHandle, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, (float)sliVolume.Value);
                txtDuration.Text = Un4seen.Bass.Utils.FixTimespan(BassEngine.Instance.ChannelLength, "HHMMSS");
                rdbX1.IsChecked = true;
                drpZoom.SelectedIndex = 0;
            }
        }

        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnPlus5_Click(object sender, RoutedEventArgs e)
        {
            BassEngine.Instance.ChannelPosition += 5;
        }

        private void btnPlus10_Click(object sender, RoutedEventArgs e)
        {
            BassEngine.Instance.ChannelPosition += 10;
        }
        private void btnPlus20_Click(object sender, RoutedEventArgs e)
        {
            BassEngine.Instance.ChannelPosition += 20;
        }

        private void btnMin1_Click(object sender, RoutedEventArgs e)
        {
            BassEngine.Instance.ChannelPosition -= 1;
        }

        private void btnMin2_Click(object sender, RoutedEventArgs e)
        {
            BassEngine.Instance.ChannelPosition -= 2;
        }

        private void btnMin5_Click(object sender, RoutedEventArgs e)
        {
            BassEngine.Instance.ChannelPosition -= 5;
        }

        private void sliAudio_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                PositionByClick(sender, e);
        }
        private void sliAudio2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                PositionByClick(sender, e);
        }

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            waveformTimeline.Width = this.OriginalWaveFormWidth;
        }
        private void btnZoomX2_Click(object sender, RoutedEventArgs e)
        {
            waveformTimeline.Width = this.OriginalWaveFormWidth * 2;
        }
        private void btnZoomX4_Click(object sender, RoutedEventArgs e)
        {
            waveformTimeline.Width = this.OriginalWaveFormWidth * 4;
        }
        private void btnZoomX8_Click(object sender, RoutedEventArgs e)
        {
            waveformTimeline.Width = this.OriginalWaveFormWidth * 8;
        }

        private void PositionByClick(object sender, MouseEventArgs e)
        {
            updateSlider = (Slider)sender;
            Point position = e.GetPosition(updateSlider);
            double d = 1.0d / updateSlider.ActualWidth * position.X;
            var p = updateSlider.Maximum * d;
            BassEngine.Instance.ChannelPosition = p;
            if (updateSlider == sliAudio)
            {
                sliAudio.Background = Brushes.Green;
                sliAudio2.Background = Brushes.Gray;
                txtCrtIn.Text = Un4seen.Bass.Utils.FixTimespan(BassEngine.Instance.ChannelPosition, "HHMMSSFF");
            }
            else if (updateSlider == sliAudio2)
            {
                sliAudio.Background = Brushes.Gray;
                sliAudio2.Background = Brushes.Green;
                txtCrtOut.Text = Un4seen.Bass.Utils.FixTimespan(BassEngine.Instance.ChannelPosition, "HHMMSSFF");
            }
        }

        /// <summary>
        /// Not needed in current logic.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PositionByDrag(object sender, MouseEventArgs e)
        {
            if (((Slider)sender).IsFocused && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                uiUpdate.Stop();
                txtRemain.Text = Un4seen.Bass.Utils.FixTimespan(BassEngine.Instance.ChannelLength - BassEngine.Instance.ChannelPosition, "HHMMSS");
                BassEngine.Instance.ChannelPosition = ((Slider)sender).Value;
                uiUpdate.Start();
            }
        }

        private void btnSetIn_Click(object sender, RoutedEventArgs e)
        {
            cutInTime = BassEngine.Instance.ChannelPosition;
            //cutInTime = sliAudio.Value;
            txtBegin.Text = Un4seen.Bass.Utils.FixTimespan(cutInTime, "HHMMSSFF");
        }

        private void btnSetOut_Click(object sender, RoutedEventArgs e)
        {
            cutOutTime = BassEngine.Instance.ChannelPosition;
            //cutOutTime = sliAudio2.Value;
            txtEnd.Text = Un4seen.Bass.Utils.FixTimespan(cutOutTime, "HHMMSSFF");
        }

        private void btnBegin_Click(object sender, RoutedEventArgs e)
        {
            BassEngine.Instance.ChannelPosition = cutInTime;
            sliAudio.Value = cutInTime;
            txtBegin.Text = Un4seen.Bass.Utils.FixTimespan(cutInTime, "HHMMSSFF");
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            BassEngine.Instance.ChannelPosition = cutOutTime;
            sliAudio2.Value = cutOutTime;
            txtEnd.Text = Un4seen.Bass.Utils.FixTimespan(cutOutTime, "HHMMSSFF");
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (cutInTime > cutOutTime)
            {
                MessageBox.Show("CutIn time is later than CutOut time.\nPlease adjust the timepoints.", "Wrong cut timepoints!");
                return;
            }


            if (System.IO.Path.GetExtension(FileText.Text) == ".ac3")
                ExportData(FileText.Text, "0", "ac3");
            else if (System.IO.Path.GetExtension(FileText.Text) == ".mp2")
                ExportData(FileText.Text, "0", "mp2");
            else if (System.IO.Path.GetExtension(FileText.Text) == ".ts")
            {
                MessageBox.Show("File not supported!");
            }
            else
            {
                MessageBox.Show("File not supported!");
            }
        }
        /// <summary>
        /// Exports the selected region to a new file.
        /// </summary>
        /// <param name="inputFile">Path of the input file</param>
        /// <param name="trackNum">Track identifier for mkvMerge</param>
        /// <param name="extension">Defines the extension based on the source file</param>
        private void ExportData(string inputFile, string trackNum, string extension)
        {
            string outName;
            bool result = File.Exists("mkvmerge.exe");
            if (!result)
            {
                MessageBox.Show("TS Demuxer is not found!");
                outName = "TS Demuxer is not found!";
                return;
            }

            outName = String.Format("{0}_{1}_{2}.{3}", inputFile.Substring(0, inputFile.Length - 3), Un4seen.Bass.Utils.FixTimespan(cutInTime, "HHMMSSFF").Replace(':', '-'),
                                                     Un4seen.Bass.Utils.FixTimespan(cutOutTime, "HHMMSSFF").Replace(':', '-'), extension);
            

            string cutParams = String.Format("-o \"{0}\" -a {1} --split parts:{2}-{3} \"{4}\"", outName, trackNum, Un4seen.Bass.Utils.FixTimespan(cutInTime, "HHMMSSFF"), Un4seen.Bass.Utils.FixTimespan(cutOutTime, "HHMMSSFF"), inputFile);
            if (outName != "")
            {
                var standardOutput = new StringBuilder();
                Process processCut = new Process();
                processCut.StartInfo = new ProcessStartInfo("mkvmerge.exe", cutParams);
                processCut.StartInfo.RedirectStandardOutput = true;
                processCut.StartInfo.RedirectStandardError = true;
                processCut.StartInfo.UseShellExecute = false;
                processCut.Start();

                while (!processCut.HasExited)
                {
                    standardOutput.Append(processCut.StandardOutput.ReadToEnd());
                }

                standardOutput.Append(processCut.StandardOutput.ReadToEnd());
                MessageBox.Show(standardOutput.ToString(), "Process information:", MessageBoxButton.OK);
            }
        }

        private void rdbX1_Checked(object sender, RoutedEventArgs e)
        {
            Un4seen.Bass.Bass.BASS_ChannelSetAttribute(BassEngine.Instance.ActiveStreamHandle, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_FREQ, this.OriginalChannelFreq);
        }

        private void rdbX2_Checked(object sender, RoutedEventArgs e)
        {
            Un4seen.Bass.Bass.BASS_ChannelSetAttribute(BassEngine.Instance.ActiveStreamHandle, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_FREQ, this.OriginalChannelFreq * 2);
        }
        private void rdbX3_Checked(object sender, RoutedEventArgs e)
        {
            Un4seen.Bass.Bass.BASS_ChannelSetAttribute(BassEngine.Instance.ActiveStreamHandle, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_FREQ, this.OriginalChannelFreq * 3);
        }

        private void rdbX4_Checked(object sender, RoutedEventArgs e)
        {
            Un4seen.Bass.Bass.BASS_ChannelSetAttribute(BassEngine.Instance.ActiveStreamHandle, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_FREQ, this.OriginalChannelFreq * 4);
        }

        private void drpZoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            waveformTimeline.Width = OriginalWaveFormWidth * (int)drpZoom.SelectedValue;
        }

        private void sliVolume_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var sli = (Slider)sender;
                Point position = e.GetPosition(sli);
                double d = 1.0d / sli.ActualWidth * position.X;
                var p = sli.Maximum * d;
                sli.Value = p;
                Un4seen.Bass.Bass.BASS_ChannelSetAttribute(BassEngine.Instance.ActiveStreamHandle, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, (float)p);
                lblVolume.Content = "Volume: " + (int)(p * 100) + "%";
            }
        }

    }
}
