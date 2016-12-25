using System;
using System.Collections.Generic;
using System.Text;

namespace SoXGUI
{
    /// <summary>
    /// SoXで設定可能なフォーマットオプションを取得するインターフェース
    /// </summary>
    public interface ISoXParam
    {
        string[] getSampleFormatTable();
        string[] getBitDepthTable(int index);
        string[] getFsTable(int index);
        string[] getChTable();
    }

    /// <summary>
    /// フォーマットオプション取得の抽象クラス
    /// </summary>
    public abstract class SoXParam : ISoXParam
    {
        protected readonly string[] sampleFormat;

        public SoXParam(string[] format)
        {
            sampleFormat = format;
        }

        public abstract string[] getBitDepthTable(int index);
        public abstract string[] getSampleFormatTable();
        public virtual string[] getFsTable(int index)
        {
            return new string[] { "入力と同じ", "4000", "8000", "11025", "12000", "16000", "22050", "24000", "32000", "44100", "48000", "64000", "88200", "96000", "176400", "192000", "352800", "384000" };
        }
        public virtual string[] getChTable()
        {
            return new string[] { "入力と同じ", "mono", "stereo", "5.1" };
        }
    }

    /// <summary>
    /// Wavフォーマット用のオプションクラス
    /// </summary>
    public class SoXParamWav : SoXParam
    {
        public SoXParamWav()
            : base(new string[] { "入力と同じ", "signed-integer", "unsigned-integer", "floating-point", "u-law", "a-law", "ima-adpcm", "ms-adpcm", "gsm-full-rate" })
        {
        }

        public override string[] getBitDepthTable(int index)
        {
            switch (index) {
                case 1:
                    return new string[] { "入力と同じ", "16-bit", "24-bit", "32-bit" };
                case 2:
                    return new string[] { "8-bit" };
                case 3:
                    return new string[] { "入力と同じ", "32-bit", "64-bit" };
                case 4:
                case 5:
                    return new string[] { "8-bit" };
                case 6:
                case 7:
                    return new string[] { "4-bit" };
                case 8:
                    return new string[] { "16-bit" };
                default:
                    break;
            }
            return new string[] { "入力と同じ" };
        }

        public override string[] getSampleFormatTable()
        {
            return sampleFormat;
        }
    }

    static class SoXParamDictionary
    {
        public static readonly Dictionary<string, SoXParam> dict = new Dictionary<string, SoXParam>()
        {
            { "wav", new SoXParamWav() }
        };
    }
}
