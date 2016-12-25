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

namespace SoXGUI
{
    /// <summary>
    /// SoX-GUI設定情報クラス
    /// </summary>
    class GuiSetting
    {
        /// <summary>SoX本体のパス</summary>
        public string SoxPath { get; set; } = "";
        /// <summary>入力ファイル追加時に情報表示をするか否か</summary>
        public bool ShowInputFileInfo { get; set; } = false;

        /// <summary>
        /// ユーザースコープデータから読み込む
        /// </summary>
        public void LoadFromUserData()
        {
            SoxPath = global::SoXGUI.Properties.Settings.Default.BinPath;
            ShowInputFileInfo = global::SoXGUI.Properties.Settings.Default.ShowInputFileInfo;
        }

        /// <summary>
        /// ユーザースコープデータとして保存する
        /// </summary>
        public void SaveToUserData()
        {
            global::SoXGUI.Properties.Settings.Default.BinPath = SoxPath;
            global::SoXGUI.Properties.Settings.Default.ShowInputFileInfo = ShowInputFileInfo;
            global::SoXGUI.Properties.Settings.Default.Save();
        }
    }
}
