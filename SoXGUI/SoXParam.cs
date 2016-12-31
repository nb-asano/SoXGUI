using System;
using System.Collections.Generic;
using System.Text;

namespace SoXGUI
{
    #region フォーマット関連

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

    #endregion

    #region エフェクト関連

    public class EffectParamCombi
    {
        /// <summary>パラメータ項目名</summary>
        public string Name { get; private set; }
        public string Unit { get; private set; }
        public bool IsSelectable { get; private set; }

        public EffectParamCombi(string name, string unit, bool isSelectable)
        {
            Name = name;
            Unit = unit;
            IsSelectable = isSelectable;
        }
    }

    public interface ISoXEffectParam
    {
        List<EffectParamCombi> getParamCombi();
    }

    /// <summary>
    /// パラメータなしを意味する共通のエフェクト情報クラス
    /// </summary>
    public class NoneParamEffect : ISoXEffectParam
    {
        public List<EffectParamCombi> getParamCombi()
        {
            return new List<EffectParamCombi>();
        }
    }

    /// <summary>
    /// 1パラメータを意味する共通のエフェクト情報クラス
    /// </summary>
    /// <remarks>
    /// 選択式パラメータには対応していません。
    /// </remarks>
    public class OneParamEffect : ISoXEffectParam
    {
        private List<EffectParamCombi> list;

        public List<EffectParamCombi> getParamCombi()
        {
            return list;
        }

        public OneParamEffect(string name, string unit)
        {
            list = new List<EffectParamCombi>();
            list.Add(new EffectParamCombi(name, unit, false));
        }
    }

    #endregion

    /// <summary>
    /// 定数宣言
    /// </summary>
    static class SoXConstants
    {
        /// <summary>
        /// フォーマットパラメータテーブル
        /// </summary>
        public static readonly Dictionary<string, SoXParam> fmtDict = new Dictionary<string, SoXParam>()
        {
            { "wav", new SoXParamWav() }
        };
        public static readonly Dictionary<string, ISoXEffectParam> effDict = new Dictionary<string, ISoXEffectParam>()
        {
            { "norm", new OneParamEffect("ゲイン", "dB(～0dB)") },
            { "reverse", new NoneParamEffect() }
        };
        public static readonly string[] format = new string[] { "8svx", "aif", "aifc", "aiff", "aiffc", "al", "amb", "amr-nb", "amr-wb", "anb", "au", "avr", "awb", "cdda", "cdr", "cvs", "cvsd", "cvu", "dat", "dvms", "f32", "f4", "f64", "f8", "flac", "fssd", "gsm", "gsrt", "hcom", "htk", "ima", "ircam", "la", "lpc", "lpc10", "lu", "maud", "mp2", "mp3", "nist", "ogg", "prc", "raw", "s1", "s16", "s2", "s24", "s3", "s32", "s4", "s8", "sb", "sf", "sl", "sln", "smp", "snd", "sndr", "sndt", "sou", "sox", "sph", "sw", "txw", "u1", "u16", "u2", "u24", "u3", "u32", "u4", "u8", "ub", "ul", "uw", "vms", "voc", "vorbis", "vox", "wav", "wavpcm", "wv", "wve", "xa" };
        public static readonly string[] effect = new string[] { "allpass", "band", "bandpass", "bandreject", "bass", "bend", "biquad", "chorus", "channels", "compand", "contrast", "dcshift", "deemph", "delay", "dither", "divide", "downsample", "earwax", "echo", "echos", "equalizer", "fade", "fir", "firfit", "flanger", "gain", "highpass", "hilbert", "input", "ladspa", "loudness", "lowpass", "mcompand", "noiseprof", "noisered", "norm", "oops", "output", "overdrive", "pad", "phaser", "pitch", "rate", "remix", "repeat", "reverb", "reverse", "riaa", "silence", "sinc", "spectrogram", "speed", "splice", "stat", "stats", "stretch", "swap", "synth", "tempo", "treble", "tremolo", "trim", "upsample", "vad", "vol" };
    }
}
