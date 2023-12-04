using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedUtil;

namespace SharedUtil
{
    internal class ProcessorException : Exception 
    {
        public ProcessorException(string message) : base(message) { }
    }

    internal class Processor
    {


        public Processor() 
        { 
                    
        }

        private static  string[] lines;
        public static int currentRow = 0;



        public static void run(string[] l, int lMode, int offset) {
            Util.logMode = lMode;
            lines = l;

            try
            {
                // 全行処理
                for (currentRow = offset; currentRow < lines.Length; currentRow++)
                {

                    // 実行前にマウス位置確認
                    // もし左下ならキー入力待機、入力後、5秒待って再開
                    if (Cursor.Position.X == 0 && Cursor.Position.Y == (Screen.PrimaryScreen.Bounds.Height - 1))
                    {
                        Console.WriteLine("処理中断 Enter押下で処理再開");
                        Console.ReadLine();
                        for (int i = 5; i > 0; i--)
                        {
                            Console.Write(string.Format("\r処理再開まで{0}秒", i));
                            Thread.Sleep(1000);
                        }
                        Console.WriteLine("\r処理再開");

                    }


                    Util.writeLog("I", "実行行:" + (currentRow + 1));
                    Util.nestLevel++;
                    Util.writeLog("I", "原本:" + lines[currentRow]);
                    Util.writeLog("I", "成形:" + Util.mold(lines[currentRow]));
                    Util.nestLevel++;

                    Functions.currentRow = currentRow;
                    runFunction(Util.mold(lines[currentRow]));


                    Util.nestLevel--;
                    Util.nestLevel--;
                }
            }
            catch (ProcessorException pe)
            {
                Util.writeLog("E", pe.Message);

                // keyDownを解消
                foreach (KeyValuePair<string, byte> keyInfo in Functions.keyCodeList)
                {
                    Functions.functionKeyUp(keyInfo.Key);

                }

                throw pe;

            }
            finally 
            {
                // keyDownを解消
                foreach (KeyValuePair<string, byte> keyInfo in Functions.keyCodeList)
                {
                    Functions.functionKeyUp(keyInfo.Key);

                }

            }

        }



        static public string runFunction(string text)
        {
            string result = "";

            Util.writeLog("I", "関数実行:" + text);
            Util.nestLevel++;

            // 関数名、引数に分割する
            if (text.IndexOf('(') >= 0)
            {
                Util.writeLog("D", "パラメータ内関数処理開始");
                Util.nestLevel++;
                String function = text.Substring(0, text.IndexOf('('));
                String argsStr = text.Substring(text.IndexOf('(') + 1);
                argsStr = argsStr.Substring(0, argsStr.Length - 1);

                string[] args = Util.splitArgs(argsStr);

                string newArgsStr = "";
                for (int i = 0; i < args.Length; i++)
                {
                    string temp = args[i];
                    if (Util.isFunction(args[i]))
                    {

                        temp = runFunction(temp);
                    }
                    newArgsStr += temp + ",";
                }
                newArgsStr = newArgsStr.Substring(0, newArgsStr.Length - 1);


                Util.nestLevel--;
                Util.writeLog("D", "パラメータ内関数処理終了");
                // ここからが本処理




                Util.writeLog("I", "関数処理開始:" + function + "(" + newArgsStr + ")");

                // ifの条件を満たしている場合は実行、満たしていない場合はifとifEndのみ処理
                // (ifとifEndの対応ずれにならないよう、条件を満たしていない場合もifはカウントアップが必要)


                if (!Functions.ifCondition[Functions.ifLevel])
                {
                    // if文の条件がfalseの時、処理は行わないが、ifとifEndの対応付けをするため処理をする必要がある。(ifEndがどれと対応づくかわからない)
                    // while等では不要。

                    switch (function)
                    {
                        case "if":

                            result = Functions.functionIf("false"); //レベルのカウントアップが必要なので引数をfalseで固定して呼び出す
                            break;
                        case "ifEnd":

                            Functions.checkParams(function, newArgsStr);

                            result = Functions.functionIfEnd(newArgsStr);
                            break;
                        default:

                            result = "SKIP";
                            Util.writeLog("I", "if条件を満たさないためスキップ");
                            break;
                    }

                } else if (Functions.repeatBreak[Functions.repeatLevel]) {
                    // Break中はif、ifEnd、repeatEndのみ読む

                    switch (function)
                    {
                        case "if":

                            result = Functions.functionIf("false"); //レベルのカウントアップが必要なので引数をfalseで固定して呼び出す
                            break;
                        case "ifEnd":

                            Functions.checkParams(function, newArgsStr);

                            result = Functions.functionIfEnd(newArgsStr);
                            break;

                        case "repeatEnd":

                            Functions.checkParams(function, newArgsStr);

                            result = Functions.functionRepeatEnd(newArgsStr);
                            break;

                        default:

                            result = "SKIP";
                            Util.writeLog("I", "if条件を満たさないためスキップ");
                            break;
                    }


                }
                else
                {
                    // すべての引数をチェック
                    // 引数に関数が含まれる場合、関数処理を行い、結果に置き換える
                    Functions.checkParams(function, newArgsStr);


                    switch (function)
                    {
                        case "and":
                            result = Functions.functionAnd(newArgsStr);
                            break;

                        case "calc":
                            result = Functions.functionCalc(newArgsStr);
                            break;

                        case "clickLeft":
                            result = Functions.functionClickLeft(newArgsStr);
                            break;

                        case "clickRight":
                            result = Functions.functionClickRight(newArgsStr);
                            break;

                        case "closeWindow":
                            result = Functions.functionCloseWindow(newArgsStr);
                            break;

                        case "command":
                            result = Functions.functionCommand(newArgsStr);
                            break;

                        case "compare":
                            result = Functions.functionCompare(newArgsStr);
                            break;

                        case "concat":
                            result = Functions.functionConcat(newArgsStr);
                            break;

                        case "doubleClick":
                            result = Functions.functionDoubleClick(newArgsStr);
                            break;

                        case "foreach":
                            result = Functions.functionForeach(newArgsStr);
                            break;

                        case "foreachEnd":
                            result = Functions.functionForeachEnd(newArgsStr);
                            break;



                        case "getCursorPos":
                            result = Functions.functionGetCursorPos(newArgsStr);
                            break;
                            

                        case "getNth":
                            result = Functions.functionGetNth(newArgsStr);

                            break;
                        case "getImagePos":
                            result = Functions.functionGetImagePos(newArgsStr);
                            break;

                        case "getForegroundAppName":
                            result = Functions.functionGetForegroundAppName(newArgsStr);
                            break;

                        case "getImagePosMiddle":
                            result = Functions.functionGetImagePosMiddle(newArgsStr);
                            break;

                        case "getImagePosMiddleEx":
                            result = Functions.functionGetImagePosMiddleEx(newArgsStr);
                            break;

                        case "getWindowPos":
                            result = Functions.functionGetWindowPos(newArgsStr);
                            break;

                        case "getWindowPosMiddle":
                            result = Functions.functionGetWindowPosMiddle(newArgsStr);
                            break;

                        case "getWindowRect":
                            result = Functions.functionGetWindowRect(newArgsStr);
                            break;





                        case "if":
                            result = Functions.functionIf(newArgsStr);
                            break;

                        case "ifEnd":
                            result = Functions.functionIfEnd(newArgsStr);
                            break;

                        case "isThereImage":
                            result = Functions.functionIsThereImage(newArgsStr);
                            break;


                        case "keyDown":
                            result = Functions.functionKeyDown(newArgsStr);
                            break;

                        case "keyUp":
                            result = Functions.functionKeyUp(newArgsStr);
                            break;

                        case "load":
                            result = Functions.functionLoad(newArgsStr);
                            break;

                        case "loadPos":
                            result = Functions.functionLoadPos(newArgsStr);
                            break;

                        case "loadStr":
                            result = Functions.functionLoadStr(newArgsStr);
                            break;

                        case "move":
                            result = Functions.functionMove(newArgsStr);
                            break;

                        case "not":
                            result = Functions.functionNot(newArgsStr);
                            break;
                        case "offset":
                            result = Functions.functionOffset(newArgsStr);
                            break;
                        case "or":
                            result = Functions.functionOr(newArgsStr);
                            break;

                        case "random":
                            result = Functions.functionRandom(newArgsStr);
                            break;


                        case "repeat":
                            result = Functions.functionRepeat(newArgsStr);
                            break;

                        case "repeatBreak":
                            result = Functions.functionRepeatBreak(newArgsStr);
                            break;

                        case "repeatEnd":
                            result = Functions.functionRepeatEnd(newArgsStr);
                            break;

                        case "restartAt":
                            result = Functions.functionRestartAt(newArgsStr);
                            break;

                        case "save":
                            result = Functions.functionSave(newArgsStr);
                            break;

                        case "savePos":
                            result = Functions.functionSavePos(newArgsStr);
                            break;

                        case "scroll":
                            result = Functions.functionScroll(newArgsStr);
                            break;

                        case "setBaseDir":
                            result = Functions.functionSetBaseDir(newArgsStr);
                            break;

                        case "setWindowToForeground":
                            result = Functions.functionSetWindowToForeground(newArgsStr);
                            break;


                        case "type":
                            result = Functions.functionType(newArgsStr);
                            break;








                        case "wait":
                            result = Functions.functionWait(newArgsStr);
                            break;

                        case "waitMilli":
                            result = Functions.functionWaitMilliseconds(newArgsStr);
                            break;

                        case "waitForImage":
                            result = Functions.functionWaitForImage(newArgsStr);
                            break;

                        case "waitForImageDisappear":
                            result = Functions.functionWaitForImageDisappear(newArgsStr);
                            break;

                        case "waitImage":
                            result = Functions.functionWaitImage(newArgsStr);
                            break;

                        case "walk":
                            result = Functions.functionWalk(newArgsStr);
                            break;





                    }


                }

                Util.nestLevel--;
                Util.writeLog("D", "関数処理終了"); 

                Util.writeLog("I", "関数実行終了:" + text);
                Util.writeLog("I", "返り値:" + result);

            }
            else 
            {
                Util.writeLog("I", Util.indent(Util.nestLevel) + "関数なし");
                Util.nestLevel--;

            }




            return result;
        }












    }
}

