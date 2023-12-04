using OpenCvSharp.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SharedUtil
{
	public class Functions
	{



		// 変数保持用ディクショナリ
		static Dictionary<string, string> variables = new Dictionary<string, string>();

		// 実行行
		public static int currentRow = 0;

		// repeat用
		static public int repeatLevel = 0; // ネストレベル
		static int[] repeatStart = new int[] { -1, -1, -1, -1, -1, -1 }; //repeatの復帰行
		static int[] repeatCount = new int[] { -1, -1, -1, -1, -1, -1 }; //repeatのカウンタ
		static int[] repeatLimit = new int[] { -1, -1, -1, -1, -1, -1 }; //repeatの繰り返し上限
		static public bool[] repeatBreak = new bool[] { false, false, false, false, false, false }; //repeatのBreakフラグ

		// if用
		public static int ifLevel = 0; // ネストレベル
		public static bool[] ifCondition = new bool[] { true, false, false, false, false, false }; //ifの条件判定結果(レベル0は常にtrue)
		static bool[] ifIsElse = new bool[] { false, false, false, false, false, false }; //Elseに入ったか否か(未指定)

		// foreach用
		static int foreachLevel = 0;
		static string[] foreachArgs = new string[] { "", "", "", "", "", "" }; // 要素配列のカンマ区切り文字列
		static string[] foreachArg = new string[] { "", "", "", "", "", "" }; // 処理中の要素
		static int[] foreachStart = new int[] { -1, -1, -1, -1, -1, -1 }; //foreachの復帰行
		static int[] foreachCount = new int[] { -1, -1, -1, -1, -1, -1 }; //foreachのカウンタ
		static int[] foreachLimit = new int[] { -1, -1, -1, -1, -1, -1 };//foreachの繰り返し上限

		// システム変数の初期値設定
		private const string VARIABLE_REPEAT = "_REPEAT";
		private const string VARIABLE_REPEAT_FROM_ZERO = "_REPEAT_FROM_ZERO";
		private const string VARIABLE_FOREACH = "_FOREACH";

		static Dictionary<string, string> preservedVariableName = new Dictionary<string, string>
		{
			{  VARIABLE_REPEAT_FROM_ZERO,"-1" },
			{  VARIABLE_REPEAT,"-1" },
			{  VARIABLE_FOREACH,"-1" }
		};

		// 汎用入力チェック
		static Dictionary<string, string> checkParamList = new Dictionary<string, string>
		{
			{ "and", "真偽値,真偽値"},
			{ "calc", "整数,文字列,整数"},
			{ "clickLeft", "整数,整数"},
			{ "clickRight", "整数,整数"},
			{ "closeWindow", "文字列"},
			{ "command", "文字列,文字列"},
			{ "compare", "文字列,文字列,文字列"},
			{ "concat", "1つ以上の文字列"},
			{ "doubleClick", "整数,整数"},		// 調整中
			{ "type", "文字列"},
			{ "foreach", "1つ以上の文字列"},	// 未実装
			{ "foreachEnd", ""},				// 未実装
			{ "getCursorPos", ""},
			{ "getForegroundAppName", ""},
			{ "getImagePos", "文字列"},
			{ "getImagePosMiddle", "文字列"},
			{ "getImagePosMiddleEx", "文字列,整数,整数"},	// 調整中
			{ "getNth", "整数,1つ以上の文字列"},
			{ "getWindowRect", "文字列"},
			{ "getWindowPos", "文字列"},
			{ "getWindowPosMiddle", "文字列"},
			{ "keyDown", "文字列"},
			{ "keyUp", "文字列"},
			{ "if", "真偽値"},
			{ "ifEnd", ""},
			{ "isThereImage", "文字列"},
			{ "load", "文字列"},
			{ "move", "整数(0以上),整数(0以上)"},
			{ "not", "真偽値"},
			{ "offset", "整数,整数,整数,整数"},
			{ "or", "真偽値,真偽値"},
			{ "random", "整数,整数"},
			{ "repeat", "整数(0以上)"},
			{ "repeatBreak", ""},
			{ "repeatEnd", ""},
			{ "restartAt", "整数,整数"},
			{ "save", "文字列,文字列"},
			{ "savePos", "文字列,整数,整数"},
			{ "scroll", "整数"},
			{ "setBaseDir", "文字列"},
			{ "setWindowToForeground", "文字列"},
			{ "walk", "整数(0以上),整数(0以上)"},
			{ "wait", "整数(0以上)"},
			{ "waitMilli", "整数(0以上)"},
			{ "waitAndGetImagePos", "文字列,整数"},			// 調整中
			{ "waitAndGetImagePosMiddle", "文字列,整数"},	// 調整中
			{ "waitForImage", "文字列,整数"},
			{ "waitForImageDisappear", "文字列,整数"},
			{ "waitMilliseconds", "整数(0以上)"},
			{ "waitImage", "文字列"},						// 廃止予定
		};

		// keyUp / keyDownに使うキーコードリスト
		public static Dictionary<string, byte> keyCodeList = new Dictionary<string, byte>
		{
			{ "shift", 0x10},
			{ "ctrl", 0x11},
			{ "alt", 0x12},
			{ "win", 0x5B}
		};

		private static string baseDir = "";












		public static string functionAnd(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			bool bool1 = bool.Parse(args[0]);
			bool bool2 = bool.Parse(args[1]);

			bool result = bool1 && bool2;

			return result.ToString();

		}




		public static string functionCalc(string argsStr)
		{
			string result = "";
			string[] args = Util.splitArgs(argsStr);

			// 比較演算子を判定
			string ope = trimDoubleQuotes(args[1]);
			int leftVal = int.Parse(args[0]);
			int rightVal = int.Parse(args[2]);


			switch (ope)
			{
				case "+":
					result = (leftVal + rightVal).ToString();
					break;

				case "-":
					result = (leftVal - rightVal).ToString();
					break;

				case "*":
					result = (leftVal * rightVal).ToString();
					break;

				case "/":
					if (rightVal == 0)
					{
						throw new ProcessorException("0での除算はできません");
					}
					result = (leftVal / rightVal).ToString();
					break;

				case "%":
					if (rightVal == 0)
					{
						throw new ProcessorException("0での剰余計算はできません");
					}
					result = (leftVal % rightVal).ToString();
					break;

				default:
					throw new ProcessorException("対象外の演算子「" + ope + "」が指定されています");
			}

			return result;
		}

		// C

		public static string functionClickLeft(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			int x = int.Parse(args[0]);
			int y = int.Parse(args[1]);

			if (x == -1 && y == -1)
			{
				Util.writeLog("W", "座標-1,-1が指定されたためマウス操作をスキップしました");

			}
			else
			{

				Cursor.Position = new System.Drawing.Point(x, y);
				Thread.Sleep(10);
				MouseControl.click(MouseControl.BUTTON_LEFT);

			}

			return "";
		}

		public static string functionClickRight(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			int x = int.Parse(args[0]);
			int y = int.Parse(args[1]);

			if (x == -1 && y == -1)
			{
				Util.writeLog("W", "座標-1,-1が指定されたためマウス操作をスキップしました");

			}
			else
			{

				Cursor.Position = new System.Drawing.Point(x, y);
				Thread.Sleep(10);
				MouseControl.click(MouseControl.BUTTON_RIGHT);

			}

			return "";
		}

		public static string functionCloseWindow(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			string title = trimDoubleQuotes(args[0]);

			WindowControl.closeWindow(title);

			return "";
		}

		public static string functionCommand(string argsStr)
		{

			string[] args = Util.splitArgs(argsStr);
			string command = trimDoubleQuotes(args[0]);
			string arguments = trimDoubleQuotes(args[1]);

			// ProcessStartInfoを設定
			ProcessStartInfo processInfo = new ProcessStartInfo
			{
				FileName = command, // 実行するコマンドのパス
				Arguments = arguments, // コマンドに渡す引数
				RedirectStandardOutput = true, // 標準出力をリダイレクト
				UseShellExecute = false, // シェルを使用しない
				CreateNoWindow = true // ウィンドウを表示しない
			};

			// プロセスを起動
			Process process = new Process
			{
				StartInfo = processInfo
			};

			process.Start();

			// 標準出力を読み取り
			string output = process.StandardOutput.ReadToEnd();

			// プロセスの終了を待つ
			process.WaitForExit();

			return output;
		}

		public static string functionCompare(string argsStr)
		{
			bool result = false;
			string[] args = Util.splitArgs(argsStr);

			// 比較演算子を判定
			string ope = trimDoubleQuotes(args[1]);
			string leftVal = trimDoubleQuotes(args[0]);
			string rightVal = trimDoubleQuotes(args[2]);

			switch (ope)
			{
				case "=":
					// 等号の場合は型関係なく単純比較
					result = leftVal.Equals(rightVal);
					break;

				case "!=":
					// 等号の場合は型関係なく単純比較
					result = leftVal.Equals(rightVal);
					break;

				case "==":
					// 等号の場合は型関係なく単純比較 とりあえず上と同じ。厳密等価演算子みたいにダブルクォートの有り無しもやる?でもそうすると文字列と数値の比較が変なことになる
					result = leftVal.Equals(rightVal);
					break;

				case ">=":
					// 等号の場合は型関係なく単純比較 とりあえず上と同じ。厳密等価演算子みたいにダブルクォートの有り無しもやる?でもそうすると文字列と数値の比較が変なことになる
					if (!Util.isNum(leftVal) || Util.isNum(rightVal))
					{
						throw new ProcessorException("比較演算子「" + ope + "」の場合、引数1と引数3は数値でないといけません");
					}
					result = int.Parse(leftVal) >= int.Parse(rightVal);
					break;

				case ">":
					// 等号の場合は型関係なく単純比較 とりあえず上と同じ。厳密等価演算子みたいにダブルクォートの有り無しもやる?でもそうすると文字列と数値の比較が変なことになる
					if (!Util.isNum(leftVal) || Util.isNum(rightVal))
					{
						throw new ProcessorException("比較演算子「" + ope + "」の場合、引数1と引数3は数値でないといけません");
					}
					result = int.Parse(leftVal) > int.Parse(rightVal);
					break;

				case "<=":
					// 等号の場合は型関係なく単純比較 とりあえず上と同じ。厳密等価演算子みたいにダブルクォートの有り無しもやる?でもそうすると文字列と数値の比較が変なことになる
					if (!Util.isNum(leftVal) || Util.isNum(rightVal))
					{
						throw new ProcessorException("比較演算子「" + ope + "」の場合、引数1と引数3は数値でないといけません");
					}
					result = int.Parse(leftVal) <= int.Parse(rightVal);
					break;

				case "<":
					// 等号の場合は型関係なく単純比較 とりあえず上と同じ。厳密等価演算子みたいにダブルクォートの有り無しもやる?でもそうすると文字列と数値の比較が変なことになる
					if (!Util.isNum(leftVal) || Util.isNum(rightVal))
					{
						throw new ProcessorException("比較演算子「" + ope + "」の場合、引数1と引数3は数値でないといけません");
					}
					result = int.Parse(leftVal) < int.Parse(rightVal);
					break;

				default:
					throw new ProcessorException("対象外の比較演算子「" + ope + "」が指定されています");
			}

			return result.ToString();
		}


		public static string functionDoubleClick(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			int x = int.Parse(args[0]);
			int y = int.Parse(args[1]);

			if (x == -1 && y == -1)
			{
				Util.writeLog("W", "座標-1,-1が指定されたためマウス操作をスキップしました");

			}
			else
			{

				Cursor.Position = new System.Drawing.Point(x, y);
				Thread.Sleep(100);
				MouseControl.click(MouseControl.BUTTON_LEFT);
				Thread.Sleep(200);
				MouseControl.click(MouseControl.BUTTON_LEFT);

			}

			return "";
		}




		// F
		public static string functionForeach(string argsStr)
		{
			if (foreachLevel < 5)
			{
				foreachLevel++;
				foreachCount[foreachLevel] = 0; // 0からLengthまで繰り返す。
				foreachArgs[foreachLevel] = argsStr;
				string[] args = Util.splitArgs(argsStr);
				foreachArg[foreachLevel] = trimDoubleQuotes(args[0]);
				foreachLimit[foreachLevel] = args.Length;
				foreachStart[foreachLevel] = currentRow;
				Util.writeLog("I", "foreach:" + foreachArg[foreachLevel]);

			}
			else
			{
				throw new ProcessorException("foreachの上限5層を超えています");
			}
			return "";

		}

		public static string functionForeachEnd(string argsStr)
		{
			if (foreachLevel > 0)
			{
				// repeatとは条件が異なる。repeatは1オリジンだがforeachは0オリジンで、10個要素がある場合、8までは次の要素を渡して復帰、9が来たら終了
				if (foreachCount[foreachLevel] < foreachLimit[foreachLevel] - 1)
				{
					// カウンタを加算して先頭に戻す
					foreachCount[foreachLevel]++;
					currentRow = foreachStart[foreachLevel];

					// 要素を差し替え
					string[] args = Util.splitArgs(foreachArgs[foreachLevel]);
					foreachArg[foreachLevel] = trimDoubleQuotes(args[foreachCount[foreachLevel]]);


					Util.writeLog("I", "foreach:" + foreachArg[foreachLevel] + " of " + foreachArg[foreachLevel]);
				}
				else
				{
					// カウンタ等を初期化(参照する可能性があるため)
					foreachCount[foreachLevel] = -1;
					foreachLimit[foreachLevel] = -1;
					foreachStart[foreachLevel] = -1;
					foreachArgs[foreachLevel] = "";
					foreachArg[foreachLevel] = "";
					foreachLevel--;
				}

			}
			else
			{
				throw new ProcessorException("foreachが開始していない状態でforeachEndが呼ばれました");
			}
			return "";

		}





		// G


		// G
		public static string functionGetNth(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			return args[int.Parse(args[0])];
		}



		public static string functionGetForegroundAppName(string argsStr)
		{
			return WindowControl.getForegroundAppName();
		}



		public static string functionGetImagePos(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			return ImageControl.getImagePos(baseDir + trimDoubleQuotes(args[0]));
		}

		public static string functionGetImagePosMiddle(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			return ImageControl.getImagePosMiddle(baseDir + trimDoubleQuotes(args[0]));
		}

		public static string functionGetImagePosMiddleEx(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			return ImageControl.getImagePosMiddleEx(baseDir + trimDoubleQuotes(args[0]), int.Parse(args[1]), int.Parse(args[2]));
		}

		public static string functionGetWindowRect(string argsStr)
		{
			return WindowControl.getWindowRect(trimDoubleQuotes(argsStr),WindowControl.WindowRectResultPattern.Rect);
		}

		public static string functionGetWindowPos(string argsStr)
		{
			return WindowControl.getWindowRect(trimDoubleQuotes(argsStr), WindowControl.WindowRectResultPattern.TopLeft);
		}
		public static string functionGetWindowPosMiddle(string argsStr)
		{
			return WindowControl.getWindowRect(trimDoubleQuotes(argsStr), WindowControl.WindowRectResultPattern.Middle);
		}




		// I

		public static string functionIf(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			if (ifLevel < 4)
			{
				ifLevel++;
				ifCondition[ifLevel] = bool.Parse(args[0]);
				ifIsElse[ifLevel] = true;
				Util.writeLog("I", "if:" + ifLevel);

			}
			else
			{
				throw new ProcessorException("ifの上限5層を超えています");
			}
			return "";

		}

		public static string functionIfEnd(string argsStr)
		{
			if (ifLevel > 0)
			{
				ifLevel--;
				Util.writeLog("I", "ifEnd:" + ifLevel);

			}
			else
			{
				throw new ProcessorException("ifが開始していない状態でifEndが呼ばれました");
			}

			return "";

		}

		public static string functionIsThereImage(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			return ImageControl.findImage(baseDir + trimDoubleQuotes(args[0])).ToString();
		}


		public static string functionKeyDown(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			string keyName = trimDoubleQuotes(args[0]).ToLower();
			byte keyCode;
			if (!keyCodeList.TryGetValue(keyName, out keyCode))
			{
				throw new ProcessorException("該当するキーがありません:" + keyName);
			}

			KeyControl.keyDown(keyCode);
			return "";

		}

		public static string functionKeyUp(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			string keyName = trimDoubleQuotes(args[0]).ToLower();
			byte keyCode;
			if (!keyCodeList.TryGetValue(keyName, out keyCode))
			{
				throw new ProcessorException("該当するキーがありません:" + keyName);
			}

			KeyControl.keyUp(keyCode);
			return "";

		}


		// L

		public static string functionLoadStr(string argsStr)
		{
			string result;
			string[] args = Util.splitArgs(argsStr);
			string key = trimDoubleQuotes(args[0]);

			switch (key)
			{
				case VARIABLE_REPEAT:
					result = repeatCount[repeatLevel].ToString();
					break;
				case VARIABLE_REPEAT_FROM_ZERO:
					// -1の時は-1を返す
					result = repeatCount[repeatLevel] > 0 ? (repeatCount[repeatLevel] - 1).ToString() : "-1";
					break;
				case VARIABLE_FOREACH:
					result = foreachArg[foreachLevel].ToString();
					break;
				default:
					if (!variables.TryGetValue(key, out result))
					{
						throw new ProcessorException("登録されていない変数「" + key + "」が参照されました");
					}
					break;
			}

			return result;
		}
		public static string functionLoadPos(string argsStr)
		{
			return functionLoadStr(argsStr);
		}
		public static string functionLoad(string argsStr)
		{
			return functionLoadStr(argsStr);
		}







		// N
		public static string functionNot(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			return (!bool.Parse(args[0])).ToString();
		}

		// O
		public static string functionOffset(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			if (int.Parse(args[0]) == -1 && int.Parse(args[1]) == -1)
			{
				Util.writeLog("W", "座標-1,-1が指定されたためoffsetをスキップしました");

				return "-1,-1";

			}
			else
			{
				return (int.Parse(args[0]) + int.Parse(args[2])) + "," + (int.Parse(args[1]) + int.Parse(args[3]));
			}

		}

		public static string functionOr(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			bool bool1 = bool.Parse(args[0]);
			bool bool2 = bool.Parse(args[1]);

			bool result = bool1 || bool2;

			return result.ToString();

		}


		// R

		public static string functionRandom(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			int lowest = int.Parse(args[0]);
			int highest = int.Parse(args[1]);
			Random r = new Random();
			return r.Next(lowest, highest).ToString();
		}
		public static string functionRepeat(string argsStr)
		{
			if (repeatLevel < 5)
			{
				repeatLevel++;
				repeatCount[repeatLevel] = 1;
				repeatLimit[repeatLevel] = int.Parse(argsStr);
				repeatStart[repeatLevel] = currentRow;
				Util.writeLog("I", "repeat:" + repeatCount[repeatLevel] + "/" + repeatLimit[repeatLevel]);

			}
			else
			{
				throw new ProcessorException("repeatの上限5層を超えています");
			}
			return "";

		}

		public static string functionRepeatBreak(string argsStr) 
		{
			repeatBreak[repeatLevel] = true;
			return "";
		}

		public static string functionRepeatEnd(string argsStr)
		{
			if (repeatLevel > 0)
			{

				Util.writeLog("I",repeatCount[repeatLevel] + ":" + repeatLimit[repeatLevel] +"CR"+ repeatStart[repeatLevel]);

				// カウントアップ前に比較していることに注意。1-10であれば、1-9の時ここに該当して開始位置に復帰し、それ以外(10の場合)は終了処理を行い次の行へ
				if (repeatCount[repeatLevel] < repeatLimit[repeatLevel] && !repeatBreak[repeatLevel])
				{
					// カウンタを加算して先頭に戻す
					repeatCount[repeatLevel]++;
					Processor.currentRow = repeatStart[repeatLevel];
					Util.writeLog("I", "repeat:" + repeatCount[repeatLevel] + "/" + repeatLimit[repeatLevel]);
				}
				else
				{
					// カウンタ等を初期化(参照する可能性があるため)
					repeatCount[repeatLevel] = -1;
					repeatLimit[repeatLevel] = -1;
					repeatStart[repeatLevel] = -1;
					repeatBreak[repeatLevel] = false;
					repeatLevel--;
				}

			}
			else
			{
				throw new ProcessorException("repeatが開始していない状態でrepeatEndが呼ばれました");
			}
			return "";

		}


		public static string functionRestartAt(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			int hour = int.Parse(args[0]);
			int minute = int.Parse(args[1]);

			//時間を取得してhour、minuteと一致したら終了、一致しなければ10秒待機
			DateTime currentDateTime = DateTime.Now;
			while (currentDateTime.Hour != hour || currentDateTime.Minute != minute)
			{
				Thread.Sleep(10000);
				currentDateTime = DateTime.Now;
			}


			return "";

		}

		// S
		public static string functionSave(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			string key = trimDoubleQuotes(args[0]);
			string value = args[1];

			string dummy;
			//もし予約語一覧にキーが存在する場合エラー
			if (preservedVariableName.TryGetValue(key, out dummy))
			{
				throw new ProcessorException("「" + key + "」は予約語として使用されています");
			}
			variables[key] = value;
			return "";
		}


		public static string functionSavePos(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			string key = trimDoubleQuotes(args[0]);
			string x = args[1];
			string y = args[2];

			string dummy;
			//もし予約語一覧にキーが存在する場合エラー
			if (preservedVariableName.TryGetValue(key, out dummy))
			{
				throw new ProcessorException("「" + key + "」は予約語として使用されています");
			}


			variables[key] = x + "," + y;
			return "";
		}



		public static string functionScroll(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			int scrollAmount = int.Parse(args[0]) * -1; // Windows APIでは下方向がマイナスだが気持ち悪いので逆転させる

			MouseControl.scroll(scrollAmount);

			return "";
		}
		public static string functionSetBaseDir(string argsStr)
		{
			baseDir = trimDoubleQuotes(argsStr);
			Console.WriteLine(baseDir);
			return "";
		}

		public static string functionSetWindowToForeground(string argsStr)
		{
			return WindowControl.setWindowToForeground(trimDoubleQuotes(argsStr)).ToString();
		}










		public static string functionMove(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			int tx = int.Parse(args[0]);
			int ty = int.Parse(args[1]);

			Cursor.Position = new System.Drawing.Point(tx, ty);

			return "";
		}
		public static string functionWalk(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);
			int tx = int.Parse(args[0]);
			int ty = int.Parse(args[1]);

			int cx = Cursor.Position.X;
			int cy = Cursor.Position.Y;

			int x = cx;
			int y = cy;

			// ループ添え字わかりやすくするため少し変えた
			for (int i = 1; i <= 10; i++)
			{
				x = cx + ((tx - cx) * i / 10);
				y = cy + ((ty - cy) * i / 10);
				Cursor.Position = new System.Drawing.Point(x, y);

				Thread.Sleep(100);
			}
			Cursor.Position = new System.Drawing.Point(tx, ty);


			return "";
		}

		public static string functionGetCursorPos(string argsStr)
		{
			return Cursor.Position.X + "," + Cursor.Position.Y;
		}

		public static string functionConcat(string argsStr)
		{
			string result = "";
			string[] args = Util.splitArgs(argsStr);
			foreach (string arg in args)
			{
				result += trimDoubleQuotes(arg);
			}
			return "\"" + result + "\"";
		}


		public static string functionType(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			SendKeys.SendWait(trimDoubleQuotes(args[0]));

			return "";
		}

		public static string functionWait(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			Thread.Sleep(int.Parse(args[0]) * 1000);

			return "";
		}

		public static string functionWaitMilliseconds(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			Thread.Sleep(int.Parse(args[0]));

			return "";
		}



		public static string functionWaitImage(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			string searchResult;

			while (true)
			{
				// 引数をそのまま渡して座標を取得
				searchResult = functionGetImagePosMiddle(argsStr);
				// 結果が存在すればBreakする
				if (!searchResult.Equals("-1,-1"))
				{
					break;
				}

				Thread.Sleep(1000);

			}

			return searchResult;
		}

		public static string functionWaitForImage(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			string imagePath = trimDoubleQuotes(args[0]);
			int waitLimit = int.Parse(args[1]);

			bool result = false;
			string searchResult;
			int count = 1;

			while (count<waitLimit && count < 1000)
			{
				// 引数をそのまま渡して座標を取得
				searchResult = functionGetImagePosMiddle(imagePath);
				// 結果が存在すればBreakする
				if (!searchResult.Equals("-1,-1"))
				{
					result = true;
					break;
				}

				Thread.Sleep(1000);
				count++;

			}

			return result.ToString();
		}

		public static string functionWaitForImageDisappear(string argsStr)
		{
			string[] args = Util.splitArgs(argsStr);

			string imagePath = trimDoubleQuotes(args[0]);
			int waitLimit = int.Parse(args[1]);

			bool result = false;
			string searchResult;
			int count = 1;

			while (count < waitLimit && count < 1000)
			{
				// 引数をそのまま渡して座標を取得
				searchResult = functionGetImagePosMiddle(imagePath);
				// 結果が存在すればBreakする
				if (searchResult.Equals("-1,-1"))
				{
					result = true;
					break;
				}

				Thread.Sleep(1000);
				count++;

			}

			return result.ToString();
		}




		static public string trimDoubleQuotes(string raw)
		{
			string result = raw;

			if (raw.StartsWith("\"") && raw.EndsWith("\""))
			{
				result = raw.Substring(1, raw.Length - 2);
			}
			return result;
		}



		static public bool checkParams(string functionName, string argsStr)
		{
			Util.writeLog("D", "引数チェック : " + functionName + "(" + argsStr + ")");
			string[] args = Util.splitArgs(argsStr);
			string checkListStr;

			// リストになければエラー
			if (!checkParamList.TryGetValue(functionName, out checkListStr))
			{
				throw new ProcessorException("該当する関数がありません:" + functionName);
			};
			string[] checkList = checkListStr.Split(',');

			// 引数の数のエラー判定
			// 引数の数が一致しない、かつ、最後の引数が「1つ以上の文字列、でない場合」
			if (args.Length != checkList.Length && (checkList.Length > 0 && !checkList[checkList.Length - 1].Equals("1つ以上の文字列")))
			{
				throw new ProcessorException("引数の数が異なります:" + functionName + "(" + checkListStr + ")" + "<->" + functionName + "(" + argsStr + ")");
			}

			// リストとの突合せ
			for (int i = 0; i < checkList.Length; i++)
			{
				if (checkList[i].Equals("整数"))
				{
					if (!Util.isNum(args[i]))
					{
						throw new ProcessorException((i + 1) + "番目の引数の型が異なります:" + functionName + "(" + checkListStr + ")" + "<->" + functionName + "(" + argsStr + ")");
					}
				}
				if (checkList[i].Equals("整数(0以上)"))
				{
					if (!Util.isNum(args[i]))
					{
						throw new ProcessorException((i + 1) + "番目の引数の型が異なります:" + functionName + "(" + checkListStr + ")" + "<->" + functionName + "(" + argsStr + ")");
					}
					else if (int.Parse(args[i]) < 0)
					{
						throw new ProcessorException((i + 1) + "番目の引数の型が異なります:" + functionName + "(" + checkListStr + ")" + "<->" + functionName + "(" + argsStr + ")");
					}
				}
				if (checkList[i].Equals("真偽値"))
				{
					if (!Util.isBool(args[i]))
					{
						throw new ProcessorException((i + 1) + "番目の引数の型が異なります:" + functionName + "(" + checkListStr + ")" + "<->" + functionName + "(" + argsStr + ")");
					}
				}

			}

			return true;
		}



	}

}
