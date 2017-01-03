using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public static class EffectParamUtil
    {
        /// <summary>
        /// 引数リストのチェック処理
        /// </summary>
        /// <param name="count">リストに要求する要素数</param>
        /// <param name="list">チェック対象のリスト</param>
        /// <remarks>チェック結果がエラーの場合は例外を投入する</remarks>
        public static void ListArgumentCheck(int count, List<string> list)
        {
            if (list == null) {
                throw new ArgumentNullException();
            }
            if (list.Count < count) {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// 選択単位からオプション文字列への変換
        /// </summary>
        /// <param name="s">選択単位の文字列</param>
        /// <returns>オプションに付与する文字列</returns>
        public static string createUnitString(string s)
        {
            string u = "";
            switch (s) {
                case "Hz":
                    u = "h";
                    break;
                case "kHz":
                    u = "k";
                    break;
                case "Octaves":
                    u = "o";
                    break;
                case "Q値":
                    u = "q";
                    break;
                case "slope":
                    u = "s";
                    break;
                default:
                    break;
            }
            return u;
        }
    }


    public class EffectParamCombi
    {
        /// <summary>パラメータ項目名</summary>
        public string Name { get; private set; }
        public bool IsSelectable { get; private set; }
        public string[] Values { get; private set; }

        public EffectParamCombi(string name, string[] list = null)
        {
            Name = name;
            Values = list;
            IsSelectable = (Values != null);
        }
    }

    public interface ISoXEffectParam
    {
        /// <summary>
        /// オプションパラメータ生成
        /// </summary>
        /// <param name="list">入力オプション（文字列）のリスト</param>
        /// <returns>生成したオプション文字列</returns>
        string getOptionString(List<string> list);
        /// <summary>
        /// オプションの有効性確認
        /// </summary>
        /// <param name="list">入力オプション（文字列）のリスト</param>
        /// <returns>有効なら真</returns>
        bool isValidOption(List<string> list);
        /// <summary>
        /// 入力項目情報取得
        /// </summary>
        /// <returns>入力項目のリスト</returns>
        ReadOnlyCollection<EffectParamCombi> getParamCombi();
    }

    /// <summary>
    /// パラメータなしのエフェクト情報クラス
    /// </summary>
    public class NoneParamEffect : ISoXEffectParam
    {
        public string getOptionString(List<string> list)
        {
            return " ";
        }

        public bool isValidOption(List<string> list)
        {
            return true;
        }

        public ReadOnlyCollection<EffectParamCombi> getParamCombi()
        {
            return new ReadOnlyCollection<EffectParamCombi>(new List<EffectParamCombi>());
        }
    }

    /// <summary>
    /// 1パラメータを必須とするエフェクト情報クラス
    /// </summary>
    public class OneParamEffect : ISoXEffectParam
    {
        private readonly List<EffectParamCombi> m_List = new List<EffectParamCombi>();

        public string getOptionString(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(1, list);

            return list[0] + " ";
        }

        public bool isValidOption(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(1, list);

            return !string.IsNullOrWhiteSpace(list[0]);
        }

        public ReadOnlyCollection<EffectParamCombi> getParamCombi()
        {
            return new ReadOnlyCollection<EffectParamCombi>(m_List);
        }

        public OneParamEffect(string name)
        {
            m_List.Add(new EffectParamCombi(name));
        }
    }

    /// <summary>
    /// フィルタータイプの基本的なエフェクト情報クラス
    /// </summary>
    public class BasicFilterParamEffect : ISoXEffectParam
    {
        protected readonly List<EffectParamCombi> m_List = new List<EffectParamCombi>();

        public virtual string getOptionString(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(3, list);

            StringBuilder sb = new StringBuilder(16);
            sb.Append(list[0]);     // 周波数
            sb.Append(" ");
            sb.Append(list[1]);     // width
            sb.Append(EffectParamUtil.createUnitString(list[2]));
            sb.Append(" ");
            return sb.ToString();
        }

        public virtual bool isValidOption(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(3, list);

            if (string.IsNullOrWhiteSpace(list[0]) || string.IsNullOrWhiteSpace(list[1])) {
                return false;
            }
            return true;
        }

        public ReadOnlyCollection<EffectParamCombi> getParamCombi()
        {
            return new ReadOnlyCollection<EffectParamCombi>(m_List);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BasicFilterParamEffect()
            : this(new List<EffectParamCombi>()
            {
                new EffectParamCombi("周波数（Hz）"),
                new EffectParamCombi("幅"),
                new EffectParamCombi("幅の単位", new string[] { "Hz", "kHz", "Octaves", "Q値" })
            })
        { }

        /// <summary>
        /// パラメータ設定用コンストラクタ。派生クラスから使用。このクラスもこのコンストラクタを暗に使用
        /// </summary>
        /// <param name="list"></param>
        protected BasicFilterParamEffect(List<EffectParamCombi> list)
        {
            m_List.AddRange(list);
        }
    }

    /// <summary>
    /// 極の指定が可能なフィルターのエフェクト情報クラス
    /// </summary>
    public class PoleFilterParamEffect : BasicFilterParamEffect
    {
        public override string getOptionString(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(4, list);

            StringBuilder sb = new StringBuilder(32);
            if (list[0] == "single-pole") {
                sb.Append("-1 ");
            } else {
                sb.Append("-2 ");
            }
            sb.Append(list[1]);     // 周波数
            sb.Append(" ");
            if (!string.IsNullOrWhiteSpace(list[2])) {
                sb.Append(list[2]);     // width
                sb.Append(EffectParamUtil.createUnitString(list[3]));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        public PoleFilterParamEffect()
            : base(new List<EffectParamCombi>()
            {
                new EffectParamCombi("極", new string[] { "single-pole", "double-pole(default)" }),
                new EffectParamCombi("周波数（Hz @-3dB）"),
                new EffectParamCombi("* 幅"),
                new EffectParamCombi("* 幅の単位", new string[] { "Q値", "Octaves", "Hz", "kHz" })
            })
        {
        }
    }

    /// <summary>
    /// BandPassフィルターのエフェクト情報クラス
    /// </summary>
    public class BandPassFilterParamEffect : BasicFilterParamEffect
    {
        public override string getOptionString(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(4, list);

            StringBuilder sb = new StringBuilder(32);
            if (list[0] == "skirt") {
                sb.Append("-c ");
            }
            sb.Append(list[1]);     // 周波数
            sb.Append(" ");
            sb.Append(list[2]);     // width
            sb.Append(EffectParamUtil.createUnitString(list[3]));
            sb.Append(" ");
            return sb.ToString();
        }

        public BandPassFilterParamEffect()
            : base(new List<EffectParamCombi>()
            {
                new EffectParamCombi("ゲイン", new string[] { "skirt", "0dB peak(default)" }),
                new EffectParamCombi("周波数（Hz @-3dB）"),
                new EffectParamCombi("幅"),
                new EffectParamCombi("幅の単位", new string[] { "Hz", "kHz", "Octaves", "Q値" })
            })
        {
        }
    }

    /// <summary>
    /// ゲイン情報付きのフィルタのエフェクト情報クラス
    /// </summary>
    public class GainFilterParamEffect : BasicFilterParamEffect
    {
        public override string getOptionString(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(4, list);

            StringBuilder sb = new StringBuilder(32);
            sb.Append(list[0]);
            sb.Append(" ");
            if (list[1] != "") {    // 周波数はオプション
                sb.Append(list[1]);
                sb.Append(" ");
            }
            if (list[2] != "") {    // widthもオプション
                sb.Append(list[2]);
                sb.Append(EffectParamUtil.createUnitString(list[3]));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        public override bool isValidOption(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(4, list);

            return !string.IsNullOrWhiteSpace(list[0]);
        }

        public GainFilterParamEffect()
            : base(new List<EffectParamCombi>()
            {
                new EffectParamCombi("ゲイン(-20～20dB)"),
                new EffectParamCombi("* 周波数(Hz @-3dB)"),
                new EffectParamCombi("* 幅"),
                new EffectParamCombi("* 幅の単位", new string[] { "slope", "Hz", "kHz", "Octaves", "Q値" })
            })
        {
        }
    }

    /// <summary>
    /// 双二次フィルタのエフェクト情報クラス
    /// </summary>
    public class BiQuadParamEffect : ISoXEffectParam
    {
        private readonly List<EffectParamCombi> m_List = new List<EffectParamCombi>();

        public string getOptionString(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(6, list);

            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < list.Count; i++) {
                sb.Append(list[i]);
                sb.Append(" ");
            }
            return sb.ToString();
        }

        public bool isValidOption(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(6, list);

            foreach (string s in list) {
                if (string.IsNullOrWhiteSpace(s)) {
                    return false;
                }
            }
            return true;
        }

        public ReadOnlyCollection<EffectParamCombi> getParamCombi()
        {
            return new ReadOnlyCollection<EffectParamCombi>(m_List);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BiQuadParamEffect()
        {
            m_List.Add(new EffectParamCombi("b0"));
            m_List.Add(new EffectParamCombi("b1"));
            m_List.Add(new EffectParamCombi("b2"));
            m_List.Add(new EffectParamCombi("a0"));
            m_List.Add(new EffectParamCombi("a1"));
            m_List.Add(new EffectParamCombi("a2"));
        }
    }

    /// <summary>
    /// Band指定のエフェクト情報クラス
    /// </summary>
    public class BandParamEffect : ISoXEffectParam
    {
        private readonly List<EffectParamCombi> m_List = new List<EffectParamCombi>();

        public string getOptionString(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(4, list);

            StringBuilder sb = new StringBuilder(32);
            if (list[0] == "noise") {
                sb.Append("-n ");
            }
            sb.Append(list[1]);     // 周波数
            sb.Append(" ");
            if (list[2] != "") {    // widthはオプション
                sb.Append(list[2]);
                sb.Append(EffectParamUtil.createUnitString(list[3]));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        public ReadOnlyCollection<EffectParamCombi> getParamCombi()
        {
            return new ReadOnlyCollection<EffectParamCombi>(m_List);
        }

        public bool isValidOption(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(2, list);

            if (string.IsNullOrWhiteSpace(list[0]) || string.IsNullOrWhiteSpace(list[1])) {
                return false;
            }
            return true;
        }

        public BandParamEffect()
        {
            m_List.Add(new EffectParamCombi("モード", new string[] { "pitched audio(default)", "noise" }));
            m_List.Add(new EffectParamCombi("周波数(Hz)"));
            m_List.Add(new EffectParamCombi("* 幅"));
            m_List.Add(new EffectParamCombi("* 幅の単位", new string[] { "Hz", "kHz", "Octaves", "Q値" }));
        }
    }

    /// <summary>
    /// コーラスのエフェクト情報クラス
    /// </summary>
    /// <remarks>2連、3連には未対応</remarks>
    public class ChorusParamEffect : ISoXEffectParam
    {
        private readonly List<EffectParamCombi> m_List = new List<EffectParamCombi>();

        public string getOptionString(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(7, list);

            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < 6; i++) {
                sb.Append(list[i]);
                sb.Append(" ");
            }
            sb.Append((list[6] == "sinusoidal") ? "-s" : "-t");
            sb.Append(" ");
            return sb.ToString();
        }

        public ReadOnlyCollection<EffectParamCombi> getParamCombi()
        {
            return new ReadOnlyCollection<EffectParamCombi>(m_List);
        }

        public bool isValidOption(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(7, list);

            foreach (string s in list) {
                if (string.IsNullOrWhiteSpace(s)) {
                    return false;
                }
            }
            return true;
        }

        public ChorusParamEffect()
        {
            m_List.Add(new EffectParamCombi("Gain-in(比率)"));
            m_List.Add(new EffectParamCombi("Gain-out(比率)"));
            m_List.Add(new EffectParamCombi("Delay(msec)"));
            m_List.Add(new EffectParamCombi("Decay(比率)"));
            m_List.Add(new EffectParamCombi("変調速度(Hz)"));
            m_List.Add(new EffectParamCombi("変調深度(msec)"));
            m_List.Add(new EffectParamCombi("タイプ", new string[] { "sinusoidal", "triangular" }));
        }
    }

    /// <summary>
    /// dcshiftのエフェクト情報クラス
    /// </summary>
    public class DCShiftParamEffect : ISoXEffectParam
    {
        private readonly List<EffectParamCombi> m_List = new List<EffectParamCombi>();

        public string getOptionString(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(2, list);

            StringBuilder sb = new StringBuilder(16);
            sb.Append(list[0]);     // シフト量
            sb.Append(" ");
            if (!string.IsNullOrWhiteSpace(list[1])) {
                sb.Append(list[1]); // limitergain
                sb.Append(EffectParamUtil.createUnitString(list[2]));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        public ReadOnlyCollection<EffectParamCombi> getParamCombi()
        {
            return new ReadOnlyCollection<EffectParamCombi>(m_List);
        }

        public bool isValidOption(List<string> list)
        {
            EffectParamUtil.ListArgumentCheck(2, list);

            if (string.IsNullOrWhiteSpace(list[0])) {
                return false;
            }
            return true;
        }

        public DCShiftParamEffect()
        {
            m_List.Add(new EffectParamCombi("shift(-2.0～2.0)"));
            m_List.Add(new EffectParamCombi("* Limit(-1.0～1.0)"));
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
        /// <summary>
        /// エフェクトパラメータテーブル
        /// </summary>
        public static readonly Dictionary<string, ISoXEffectParam> effDict = new Dictionary<string, ISoXEffectParam>()
        {
            { "allpass", new BasicFilterParamEffect() },
            { "band", new BandParamEffect() },
            { "bandpass", new BandPassFilterParamEffect() },
            { "bandreject", new BasicFilterParamEffect() },
            { "bass", new GainFilterParamEffect() },
            { "biquad", new BiQuadParamEffect() },
            { "chorus", new ChorusParamEffect() },
            { "dcshift", new DCShiftParamEffect() },
            { "earwax", new NoneParamEffect() },
            { "highpass", new PoleFilterParamEffect() },
            { "lowpass", new PoleFilterParamEffect() },
            { "norm", new OneParamEffect("ゲイン(～0dB)") },
            { "reverse", new NoneParamEffect() },
            { "speed", new OneParamEffect("速度(倍率 > 0.1)") },
            { "treble", new GainFilterParamEffect() }
        };
        public static readonly string[] format = new string[] { "8svx", "aif", "aifc", "aiff", "aiffc", "al", "amb", "amr-nb", "amr-wb", "anb", "au", "avr", "awb", "cdda", "cdr", "cvs", "cvsd", "cvu", "dat", "dvms", "f32", "f4", "f64", "f8", "flac", "fssd", "gsm", "gsrt", "hcom", "htk", "ima", "ircam", "la", "lpc", "lpc10", "lu", "maud", "mp2", "mp3", "nist", "ogg", "prc", "raw", "s1", "s16", "s2", "s24", "s3", "s32", "s4", "s8", "sb", "sf", "sl", "sln", "smp", "snd", "sndr", "sndt", "sou", "sox", "sph", "sw", "txw", "u1", "u16", "u2", "u24", "u3", "u32", "u4", "u8", "ub", "ul", "uw", "vms", "voc", "vorbis", "vox", "wav", "wavpcm", "wv", "wve", "xa" };
        public static readonly string[] effect = new string[] { "allpass", "band", "bandpass", "bandreject", "bass", "bend", "biquad", "chorus", "channels", "compand", "contrast", "dcshift", "deemph", "delay", "dither", "divide", "downsample", "earwax", "echo", "echos", "equalizer", "fade", "fir", "firfit", "flanger", "gain", "highpass", "hilbert", "input", "ladspa", "loudness", "lowpass", "mcompand", "noiseprof", "noisered", "norm", "oops", "output", "overdrive", "pad", "phaser", "pitch", "rate", "remix", "repeat", "reverb", "reverse", "riaa", "silence", "sinc", "spectrogram", "speed", "splice", "stat", "stats", "stretch", "swap", "synth", "tempo", "treble", "tremolo", "trim", "upsample", "vad", "vol" };
    }
}
