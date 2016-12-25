/*
 * Copyright (C) 2013-2016 Sakura-Zen soft <nb.asano@gmail.com>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307, USA.
 */
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace SoXGUI
{
    public partial class MainWindow
    {
        /// <summary>
        /// パスを取得する
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string getLowerExtension(string filePath)
        {
            try {
                string s = System.IO.Path.GetExtension(filePath);
                if (s.Length >= 1) {
                    return s.Remove(0, 1).ToLower();
                }
            } catch (System.ArgumentException) {
                return "";
            }
            return "";
        }

        /// <summary>
        /// 同期的にSoXを実行する
        /// </summary>
        /// <param name="arg">SoXに渡す引数</param>
        private void runSox(string arg)
        {
            ProcessStartInfo psInfo = new ProcessStartInfo();
            psInfo.FileName = m_GuiSetting.SoxPath;
            psInfo.Arguments = arg;
            psInfo.CreateNoWindow = true;
            psInfo.UseShellExecute = false;
            psInfo.RedirectStandardOutput = true;
            psInfo.RedirectStandardError = true;

            // 実行
            try {
                using (Process p = Process.Start(psInfo)) {

                    var stdout = new StringBuilder();
                    var stderr = new StringBuilder();

                    p.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null) { stdout.AppendLine(e.Data); }
                    };
                    p.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null) { stderr.AppendLine(e.Data); }
                    };
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

                    var isTimedOut = false;
                    // 3分以内に処理は終わらなければならないものとする
                    if (!p.WaitForExit((int)TimeSpan.FromMinutes(3).TotalMilliseconds)) {
                        isTimedOut = true;
                    }
                    p.CancelOutputRead();
                    p.CancelErrorRead();

                    if (isTimedOut) {
                        textBoxConsole.Text = "規定時間以内にSoXが完了しませんでした。";
                    } else {
                        string msgText = (p.ExitCode == 0) ? stdout.ToString() : stderr.ToString();
                        if (p.ExitCode == 1 && string.IsNullOrWhiteSpace(msgText)) {
                            msgText = stdout.ToString();
                        }
                        textBoxConsole.Text = msgText;
                    }
                }
            } catch {
                MessageBox.Show("実行エラーが発生しました。");
            }
        }
        /// <summary>
        /// ファイル情報表示用オプションを生成します。
        /// </summary>
        /// <param name="fileName">ファイルのパス</param>
        /// <returns>オプション文字列</returns>
        private string createInformationOption(string filename)
        {
            return "--i \"" + filename + "\" ";
        }

        private string createGlobalHelpOption()
        {
            return "-h";
        }

        private string createFormatHelpOption(string fmt)
        {
            return "--help-format " + fmt;
        }

        private string createEffectHelpOption(string effect)
        {
            return "--help-effect " + effect;
        }

        private string createSampleOption(string sampleType)
        {
            if (sampleType == "入力と同じ") {
                return "";
            }
            return "-e " + sampleType + " ";
        }

        private string createBitDepthOption(string bitDepth)
        {
            if (bitDepth == "入力と同じ") {
                return "";
            }
            string s = bitDepth.Replace("-bit", "");
            return "-b " + s + " ";
        }

        private string createFsOption(string fs)
        {
            if (fs == "入力と同じ") {
                return "";
            }
            return "-r " + fs + " ";
        }

        private string createChannelOption(string channel)
        {
            if (channel == "入力と同じ") {
                return "";
            } else if (channel == "mono") {
                return "-c 1 ";
            } else if (channel == "stereo") {
                return "-c 2 ";
            } else if (channel == "5.1") {
                return "-c 6 ";
            }
            return "-c " + channel + " ";
        }

        private string createVolumeOption(string vol)
        {
            return "-v " + vol + " ";
        }

        /// <summary>
        /// 実行動作のルート
        /// </summary>
        /// <param name="showonly">コマンドを実際に実行しない場合は真</param>
        private void execute(bool showonly)
        {
            switch (tabControlMain.SelectedIndex) {
                case 0:
                    executeMain(showonly);
                    break;
                case 1:
                    executeHelp(showonly);
                    break;
                default:
                    break;
            }
        }

        private void executeMain(bool showonly)
        {
            // ファイルパスが空なら実行しない
            if (string.IsNullOrWhiteSpace(textBoxInputFile.Text) || string.IsNullOrWhiteSpace(textBoxOutputFile.Text)) {
                return;
            }
            string aaa = cmbBoxOutSampleFormat.Text;
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            sb.Append(textBoxInputFile.Text);
            sb.Append("\" ");
            sb.Append(createSampleOption(cmbBoxOutSampleFormat.Text));
            sb.Append(createBitDepthOption(cmbBoxOutSampleSize.Text));
            sb.Append(createFsOption(cmbBoxOutSampleRate.Text));
            sb.Append(createChannelOption(cmbBoxOutCh.Text));
            sb.Append(" -S ");
            //sb.Append(createVolumeOption(textBoxOutputVolume.Text));
            sb.Append("\"");
            sb.Append(textBoxOutputFile.Text);
            sb.Append("\" ");

            if (showonly) {
                sendMessageToOutput("sox.exe " + sb.ToString());
            } else {
                runSox(sb.ToString());
            }
        }

        private void executeHelp(bool showonly)
        {
            string s = "";
            switch (cmbBoxHelpDiv.SelectedIndex) {
                default:
                case 0:
                    s = createGlobalHelpOption();
                    break;
                case 1:
                    s = createFormatHelpOption(cmbBoxHelp2nd.SelectedItem.ToString());
                    break;
                case 2:
                    s = createEffectHelpOption(cmbBoxHelp2nd.SelectedItem.ToString());
                    break;
            }
            if (showonly) {
                sendMessageToOutput("sox.exe " + s);
            } else {
                runSox(s);
            }
        }
        /// <summary>
        /// 入力ファイル変更時に実行する処理
        /// </summary>
        /// <param name="filePath">新たな入力ファイル</param>
        private void executeInputFileChange(string filePath)
        {
            string s = createInformationOption(filePath);
            runSox(s);
        }
        /// <summary>
        /// 出力ファイル変更時に実行する処理
        /// </summary>
        /// <param name="filePath">新たな出力ファイル</param>
        private void executeOutputFileChange(System.Windows.Controls.ComboBox cmbboxFormat, string filePath)
        {
            string ext = getLowerExtension(filePath);
            System.Diagnostics.Debug.WriteLine("cmbbox format change:" + ext);

            if (SoXParamDictionary.dict.ContainsKey(ext)) {
                ISoXParam param = SoXParamDictionary.dict[ext];

                // イベントを連鎖させる
                cmbboxFormat.ItemsSource = param.getSampleFormatTable();
                cmbboxFormat.SelectedIndex = 0;
            }
        }

        private void executeOutputFormatChange(
            System.Windows.Controls.ComboBox cmbboxSample, 
            System.Windows.Controls.ComboBox cmbboxFs,
            System.Windows.Controls.ComboBox cmbboxCh,
            string filePath, int index)
        {
            string ext = getLowerExtension(filePath);
            System.Diagnostics.Debug.WriteLine("cmbbox format change:" + ext + " index:" + index);
            if (SoXParamDictionary.dict.ContainsKey(ext)) {
                ISoXParam param = SoXParamDictionary.dict[ext];

                cmbboxSample.ItemsSource = param.getBitDepthTable(index);
                cmbboxSample.SelectedIndex = 0;

                cmbboxFs.ItemsSource = param.getFsTable(index);
                cmbboxFs.SelectedIndex = 0;

                cmbboxCh.ItemsSource = param.getChTable();
                cmbboxCh.SelectedIndex = 0;
            }
        }
    }
}
