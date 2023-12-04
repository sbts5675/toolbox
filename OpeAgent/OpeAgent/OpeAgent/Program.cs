using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedUtil;


namespace OpeAgent
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("スクリプトファイルが指定されていません。");
                Console.ReadLine();
                Environment.Exit(1);
            }

                string filePath = args[0]; // ファイルパスを指定

            string curDir = Path.GetDirectoryName(filePath);
            if (curDir != null && !string.Empty.Equals(curDir)) {
                Environment.CurrentDirectory = Path.GetDirectoryName(filePath);
            }

            int offset = 0;

            if(args.Length > 1)
            {
                offset = int.Parse(Console.ReadLine())-1;
            }


            try
            {
                UtilFile.writeLog("I", "開始" + filePath);


                // テキストファイルを読み込んで配列にセットする
                string[] lines = UtilFile.getFileLines(filePath);


                Processor.run(lines,Util.LOG_MODE_NORMAL, offset);

                UtilFile.writeLog("I", "終了");


            }

            catch (Exception e)
            {
                Console.WriteLine("予期せぬエラー発生: " + e.Message);
                Console.WriteLine("予期せぬエラー発生: " + e.StackTrace);
                Console.ReadLine();
            }
            //Console.ReadLine();

        }
    }
}
