using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedUtil
{
    internal class UtilFile
    {
        static private string fileNameLog = null;

        static public string[] getFileLines(string path)
        {
            // テキストファイルを読み込む
            string content = File.ReadAllText(path);
            // 改行コードを統一
            content = content.Replace("\r\n", "\n");
            // 改行コードでスプリット
            return content.Split('\n');

        }

        static public bool writeFile(string path, string text, bool isAdd) 
        {
            bool result = true;

            try
            {
                using(StreamWriter sw  = new StreamWriter(path, isAdd))
                {
                    sw.WriteLine(text);
                }
            }catch (Exception e)
            {
                result = false;
            }

            return result;
        }

        static public void writeLog(string mode, string text)
        {
            DateTime now = DateTime.Now;
            string dTime = now.ToString("yyyy/MM/dd HH:mm:ss.fff");

            // 初回のみ、ファイル名の設定とディレクトリ作成(既に存在すれば空振り)
            if (fileNameLog == null) 
            {
                fileNameLog = "log\\" + now.ToString("yyyy_MM_dd_HH_mm_ss") + ".txt";
                Directory.CreateDirectory("log");
            }
            text = dTime + " " + mode + " " + text;

            writeFile(fileNameLog, text, true);
        }
    }
}
