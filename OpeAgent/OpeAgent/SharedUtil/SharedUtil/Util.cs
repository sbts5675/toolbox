using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharedUtil
{
    internal class Util
    {


        public const int LOG_MODE_NORMAL = 0;
        public const int LOG_MODE_DEBUG = 1;

        public static int logMode = LOG_MODE_NORMAL;

        // ログ用ネストレベル
        public static int nestLevel = 0;

        /**
         * 文字列のマスク
         */

        public static string mold(string raw)
        {
            return mold(raw, false);
        }
        public static string mold(string raw, bool isMask)
        {
            bool isText = false;
            bool isComment = false;

            const string MASK_CHAR = "*";

            string result = "";
            string target = " ";
            string next = " ";

            // タブをスペースに置換
            string replaced = raw.Replace("\t", " ");
            string dummy  = replaced + " ";

            for (int i = 0; i < replaced.Length; i++)
            {
                target = replaced[i].ToString();
                next = dummy[i+1].ToString();


                switch (target) 
                {
                    case "\\":
                        // エスケープ記号の場合

                        // テキスト内でのみエスケープされたダブルクォーテーションをテキストの終了ととらえずに読み飛ばす
                        if (isText)
                        {
                            switch (next) 
                            {
                                case "\\":
                                    target = "\\\\";
                                    i++;
                                    break;

                                case "\"":
                                    target = "\\\"";
                                    i++;
                                    break;
                            }
                        }


                        break;

                    case "/":

                        switch (next)
                        {
                            case "/":
                                target = "//";
                                i++;
                                isComment = true;
                                break;
                        }

                        break;

                    case "\"":
                        isText = !isText;

                        // 終了のダブルクォーテーションの際は、そのダブルクォーテーション自体をマスク文字にする必要があるのでターゲットを差し替え
                        if (!isText && isMask)
                        {

                            target = MASK_CHAR;
                        }
                        break;



                    default:
                        break;
                }


                if (isText)
                {
                    // 文字列内であれば、マスク文字or原本を返す
                    result += isMask ? MASK_CHAR : target;
                }
                else if (isComment)
                {
                    // コメント内であれば何も返さない
                }
                else
                {
                    // それ以外の場合、ブランクは読み飛ばして原本を返す
                    if(!target.Equals(" "))result += target;
                }
                //Console.WriteLine(target + "  " + (isComment ? "C" : " ") + (isText ? "T" : " ") + " " + result);


            }

            return result;
        }

        public static string maskText(string raw)
        {
            return mold(raw, true);
        }
        public static string maskTextAndParams(string raw)
        {
            bool isText = false;
            bool isNextTextEnd = false;
            bool isEscape = false;
            bool isNextEscape = false;
            string result = "";
            int nestLevel = 0;

            for (int i = 0; i < raw.Length; i++)
            {
                switch (raw[i])
                {
                    case '(':
                        if (!isText)
                        {
                            nestLevel++;
                        }
                        break;

                    case ')':
                        if (!isText)
                        {
                            nestLevel--;
                        }
                        break;

                    case '"':
                        // 文字列切り替えの判定を行う

                        // エスケープ判定
                        if (isEscape)
                        {
                            // エスケープ中なら無視する
                        }
                        else
                        {
                            // 現在が文字列中であるならば、この文字より後で文字列中へ移行
                            if (isText)
                            {
                                isNextTextEnd = true;
                            }
                            else
                            {
                                isText = true;
                            }
                        }

                        break;

                    case '\\':
                        // エスケープフラグをオンにする
                        isNextEscape = true;

                        break;



                    default:
                        break;
                }


                // 文字列中かどうかを判定
                if (isText || nestLevel > 0)
                {
                    // 文字列中であればマスク文字を返す
                    result += "*";
                }
                else
                {
                    // 文字列中でなければ
                    // スペースは無視する。
                    // それ以外はそのまま返す
                    result += raw[i];
                }

                // 文字列モードからの復帰が予約されている場合、反映して復帰フラグを倒す
                if (isNextTextEnd)
                {
                    isText = false;
                    isNextTextEnd = false;
                }
                // エスケープモードの設定
                isEscape = false;
                if (isNextEscape)
                {
                    isEscape = true;
                    isNextEscape = false;
                }
            }

            return result;
        }

        public static string[] splitArgs(string argsStr) 
        {
            string temp = "";
            string masked = maskTextAndParams(argsStr);
            for(int i=0; i<argsStr.Length; i++) 
            {
                if (argsStr[i]==',' && masked[i] == ',') 
                {
                    temp += "\t";
                }
                else 
                {
                    temp += argsStr[i];
                }

            }

            return temp.Split('\t');
        
        }


        /**
         * コメント除去
         */
        public static string removeComment(string raw) 
        {
            string result = raw;

            string masked = maskText(raw);
            int targetPos = masked.IndexOf("//");

            // もしコメント記号が見つかれば、それ以降を削除して返す
            if (targetPos >= 0)
            {
                result = result.Substring(0, targetPos);
            }
            return result;
        }
        /*
                public static string getFunction(string raw)
                {
                    string masked = maskText(raw);

                }
                public static string getParamsString(string raw)
                {

                }
        */

        public static string indent(int n) 
        {
            return nthRepeat(" ", n);
        }
        public static string nthRepeat(string c, int n) 
        {
            string result = "";
            for(int i=0; i<n; i++) 
            {
                result += c;
            }
            return result;
        }


        // ネストを考慮したログ出力
        public static void writeLog(string mode, string text)
        {
            if (logMode == LOG_MODE_DEBUG || !mode.Equals("D"))
            {
                UtilFile.writeLog(mode, Util.indent(nestLevel) + text);
                Console.WriteLine(Util.indent(nestLevel) + text);

            }
        }

        // 数値にキャストできるか
        public static bool isNum(string str)
        {
            int dummy;
            return int.TryParse(str, out dummy);
        }
        // 数値にキャストできるか
        public static bool isBool(string str)
        {
            bool dummy;
            return bool.TryParse(str, out dummy);
        }

        // 関数か否かの判定
        static public bool isFunction(string text)
        {
            string pattern = @"^[a-zA-Z][a-zA-Z0-9_]*\(.*\)";
            return Regex.IsMatch(text, pattern);
        }


    }
}
