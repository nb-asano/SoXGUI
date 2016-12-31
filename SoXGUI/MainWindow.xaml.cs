using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SoXGUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>動作設定</summary>
        private GuiSetting m_GuiSetting = new GuiSetting();
        /// <summary>直前の表示状態</summary>
        private PrevOutputSelection m_PrevOut = new PrevOutputSelection();
        /// <summary>エフェクトリストボックスのデータソース</summary>
        private ObservableCollection<EffCmd> m_EffList = new ObservableCollection<EffCmd>();

        public MainWindow()
        {
            InitializeComponent();

            // エフェクトタブ
            cmbBoxEffType.ItemsSource = SoXConstants.effect;
            cmbBoxEffType.SelectedIndex = 0;

            this.DataContext = m_EffList;
        }

        #region 共通部イベントハンドラ

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 設定読み込み
            m_GuiSetting.LoadFromUserData();

            // タイトル文字列の作成
            var ver = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            mainWindow.Title = "SoX-GUI v" + ver.ProductVersion.ToString();

            // 設定を描画に反映
            textBoxSoxPath.Text = m_GuiSetting.SoxPath;
            chBoxShowInfo.IsChecked = m_GuiSetting.ShowInputFileInfo;

            // 起動引数の処理
            string[] cmd = System.Environment.GetCommandLineArgs();
            if (cmd.Length > 1) {
                textBoxInputFile.Text = cmd[1];
            }

            // 初期動作
            if (string.IsNullOrWhiteSpace(m_GuiSetting.SoxPath)) {
                // SoXのパスが設定されていなければ、設定タブを前面に持ってくる
                tabControlMain.SelectedIndex = tabControlMain.Items.Count - 1;

                var sb = new StringBuilder(128);
                sb.AppendLine("SoXのパスを設定してください。");
                sb.AppendLine("");
                sb.AppendLine("SoX Ver14以降で、ライブラリ等もすべて存在することを想定しています。https://sourceforge.net/projects/sox/files/sox/");
                textBoxConsole.Text += sb.ToString();
            } else {
                if　(cmd.Length > 1 && m_GuiSetting.ShowInputFileInfo) {
                    executeInputFileChange(cmd[1]);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // 設定を保存
            m_GuiSetting.SaveToUserData();
        }

        private void mainWindow_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true)) {
                e.Effects = DragDropEffects.Copy;
            } else {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void mainWindow_Drop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null) {
                if (files.Length >= 1) {
                    textBoxInputFile.Text = files[0];
                    executeInputFileChange(files[0]);
                }
                if (files.Length >= 2) {
                    textBoxOutputFile.Text = files[1];
                    executeOutputFileChange(cmbBoxOutSampleFormat, files[1]);
                }
            }
        }

        private void btnRefFileIn_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.FileName = "";
            ofd.Filter = "Wavファイル|*.wav|AIFFファイル|*.aiff|Rawファイル|*.raw|MP3ファイル|*.mp3|" 
                + "Oggファイル|*.ogg|Vorbisファイル|*.vorbis|すべてのファイル|*.*";
            if (ofd.ShowDialog() == true) {
                textBoxInputFile.Text = ofd.FileName;
            }
        }

        private void btnRefFileOut_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.FileName = "";
            sfd.Filter = "Wavファイル|*.wav|AIFFファイル|*.aiff";
            if (sfd.ShowDialog() == true) {
                textBoxOutputFile.Text = sfd.FileName;
            }
        }

        private void btnCmdShow_Click(object sender, RoutedEventArgs e)
        {
            execute(showonly: true);
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            execute(showonly: false);
        }

        #endregion

        #region Effectタブイベントハンドラ

        private void cmbBoxEffType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string s = SoXConstants.effect[cmbBoxEffType.SelectedIndex];
            switch (s) {
                case "allpass":
                    labelEffParam1.Content = "カットオフ";
                    //textBoxEffParam1.Visibility = Visibility.Hidden;
                    textBoxEffParam1.IsEnabled = true;
                    labelEffUnit1.Content = "kHz";
                    btnEffAdd.IsEnabled = true;
                    break;
                case "norm": {
                        List<EffectsInputUISet> uiList = new List<EffectsInputUISet>()
                        {
                            new EffectsInputUISet(label:labelEffParam1,textBox:textBoxEffParam1,unit:labelEffUnit1),
                            new EffectsInputUISet(label:labelEffParam2,textBox:textBoxEffParam2,unit:labelEffUnit2),
                            new EffectsInputUISet(label:labelEffParam3,textBox:textBoxEffParam3,unit:labelEffUnit3)
                        };
                        btnEffAdd.IsEnabled = loadEffectsInputParameter(s, uiList);
                    }
                    //labelEffParam1.Content = "ゲイン";
                    //textBoxEffParam1.IsEnabled = true;
                    //labelEffUnit1.Content = "dB(～0dB)";
                    //btnEffAdd.IsEnabled = true;
                    break;
                case "speed":
                    labelEffParam1.Content = "速度";
                    textBoxEffParam1.IsEnabled = true;
                    labelEffUnit1.Content = "倍率(0～1.0～)";
                    btnEffAdd.IsEnabled = true;
                    break;
                case "reverse":
                    textBoxEffParam1.IsEnabled = false;
                    btnEffAdd.IsEnabled = true;
                    break;
                default: {
                        List<EffectsInputUISet> uiList = new List<EffectsInputUISet>()
                        {
                            new EffectsInputUISet(label:labelEffParam1,textBox:textBoxEffParam1,unit:labelEffUnit1),
                            new EffectsInputUISet(label:labelEffParam2,textBox:textBoxEffParam2,unit:labelEffUnit2),
                            new EffectsInputUISet(label:labelEffParam3,textBox:textBoxEffParam3,unit:labelEffUnit3)
                        };
                        btnEffAdd.IsEnabled = loadEffectsInputParameter(s, uiList);
                    }
                    //textBoxEffParam1.IsEnabled = false;
                    //btnEffAdd.IsEnabled = false;
                    break;
            }
        }

        /// <summary>
        /// 追加ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEffAdd_Click(object sender, RoutedEventArgs e)
        {
            string s = SoXConstants.effect[cmbBoxEffType.SelectedIndex];
            switch (s) {
                case "reverse":
                    m_EffList.Add(new EffCmd(s, "reverse"));
                    break;
                default:
                    break;
            }
        }

        private void btnEffDel_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxEffects.SelectedIndex != -1) {
                m_EffList.RemoveAt(listBoxEffects.SelectedIndex);
            }
        }

        private void btnEffUp_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxEffects.SelectedIndex != -1) {

            }
        }

        private void btnEffDown_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxEffects.SelectedIndex != -1) {

            }
        }

        #endregion

        #region ヘルプタブイベントハンドラ

        private void cmbBoxHelpDiv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cmbBoxHelpDiv.SelectedIndex) {
                default:
                case 0:
                    if (cmbBoxHelp2nd != null) {
                        cmbBoxHelp2nd.IsEnabled = false;
                    }
                    break;
                case 1:
                    labelHelp2.Content = "フォーマット";
                    if (cmbBoxHelp2nd != null) {
                        cmbBoxHelp2nd.IsEnabled = true;
                        cmbBoxHelp2nd.ItemsSource = SoXConstants.format;
                        cmbBoxHelp2nd.SelectedIndex = 0;
                    }
                    break;
                case 2:
                    labelHelp2.Content = "エフェクト";
                    if (cmbBoxHelp2nd != null) {
                        cmbBoxHelp2nd.IsEnabled = true;
                        cmbBoxHelp2nd.ItemsSource = SoXConstants.effect;
                        cmbBoxHelp2nd.SelectedIndex = 0;
                    }
                    break;
            }
        }

        #endregion

        #region 設定タブイベントハンドラ

        private void btnSoXPath_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.FileName = "";
            ofd.Filter = "EXEファイル|*.exe|すべてのファイル|*.*";
            if (ofd.ShowDialog() == true) {
                textBoxSoxPath.Text = ofd.FileName;
                m_GuiSetting.SoxPath = ofd.FileName;
            }
        }

        private void chBoxShowInfo_Checked(object sender, RoutedEventArgs e)
        {
            m_GuiSetting.ShowInputFileInfo = true;
        }

        private void chBoxShowInfo_Unchecked(object sender, RoutedEventArgs e)
        {
            m_GuiSetting.ShowInputFileInfo = false;
        }

        #endregion

        #region その他

        private void sendMessageToOutput(string s)
        {
            textBoxConsole.Text = s;
        }

        #endregion

        private void textBoxOutputFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filePath = textBoxOutputFile.Text;
            string s = getLowerExtension(filePath);
            if (s == m_PrevOut.Ext || string.IsNullOrWhiteSpace(s)) {
                return;
            }
            m_PrevOut.Ext = s;
            m_PrevOut.indexReset();
            executeOutputFileChange(cmbBoxOutSampleFormat, filePath);
        }

        private void cmbBoxOutSampleFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBoxOutSampleFormat.SelectedIndex == m_PrevOut.FormatIndex) {
                return;
            }
            m_PrevOut.FormatIndex = cmbBoxOutSampleFormat.SelectedIndex;
            executeOutputFormatChange(cmbBoxOutSampleSize,
                cmbBoxOutSampleRate, 
                cmbBoxOutCh,
                textBoxOutputFile.Text, cmbBoxOutSampleFormat.SelectedIndex);
        }
    }

    /// <summary>
    /// 直前の状態管理用クラス
    /// </summary>
    class PrevOutputSelection
    {
        public string Ext { get; set; }
        public int FormatIndex { get; set; }

        public PrevOutputSelection()
        {
            Ext = "";
            indexReset();
        }

        public void indexReset()
        {
            FormatIndex = -1;
        }
    }

    /// <summary>
    /// エフェクト一覧のListBoxにBindingするクラス
    /// </summary>
    public class EffCmd
    {
        /// <summary>エフェクト名</summary>
        public string Effect { get; private set; }
        /// <summary>エフェクトのコマンドオプション</summary>
        public string Command { get; private set; }

        public EffCmd(string name, string cmd)
        {
            Effect = name;
            Command = cmd;
        }
    }
}
