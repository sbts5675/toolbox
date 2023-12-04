using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;


namespace SharedUtil
{
    internal class ImageControl
    {

        const int RETURN_MODE_TOPLEFT = 0;
        const int RETURN_MODE_MIDDLE = 1;

        const int MATCH_MODE_NORMAL = 0;
        const int MATCH_MODE_SENSITIVE = 1;

        static public string getImagePos(string path)
        {
            return findImage(path, 80, RETURN_MODE_TOPLEFT, MATCH_MODE_NORMAL);
        }
        static public string getImagePosMiddle(string path)
        {
            return findImage(path, 80, RETURN_MODE_MIDDLE, MATCH_MODE_NORMAL);
        }

        static public string getImagePosMiddleEx(string path, int matchMode, int threshold)
        {
            return findImage(path, threshold, RETURN_MODE_MIDDLE, matchMode);
        }


        static public bool findImage(string path)
        {
            // 「findImageの実行結果が存在しない、と一致する」の否定を返す
            return !findImage(path, 80, RETURN_MODE_TOPLEFT, MATCH_MODE_NORMAL).Equals("-1,-1");
        }


        static public string findImage(string path, int ts, int returnMode, int matchMode) 
        {
            string result = "-1,-1";

//            Mat templateImage = Cv2.ImRead(path); //ImReadの結果をToMatするとMatchTemplateでエラーになる。BitmapConverterだとうまくいく。なぜ?
            Mat templateImage = BitmapConverter.ToMat(new Bitmap(path));

            Mat targetImage = getScreenshot();

            Mat resultImage = new Mat();
            switch (matchMode) 
            {
                case MATCH_MODE_NORMAL:
                    Cv2.MatchTemplate(targetImage, templateImage, resultImage, TemplateMatchModes.CCoeffNormed);
                    break;
                case MATCH_MODE_SENSITIVE:
                    Cv2.MatchTemplate(targetImage, templateImage, resultImage, TemplateMatchModes.SqDiffNormed);
                    break;
            }


            double minVal, maxVal;
            OpenCvSharp.Point minLoc, maxLoc;
            Cv2.MinMaxLoc(resultImage, out minVal, out maxVal, out minLoc, out maxLoc); // 最高値、最低値の座標を求める。最低値は使わない。


            double threshold = ts/100.0; // 閾値を設定

            Util.writeLog("D", "閾値:" + ts +" 最大類似度:" + maxVal*100 + "(" + maxLoc.X + "," + maxLoc.Y + ") size(" + templateImage.Size().Width + "," + templateImage.Size().Height + ")");


            // 結果を残す
            Rect matchRect = new Rect(maxLoc, templateImage.Size());
            Cv2.Rectangle(targetImage, matchRect, Scalar.Red, 2);
            Cv2.PutText(targetImage, maxVal.ToString(), new OpenCvSharp.Point(0, 100), HersheyFonts.HersheySimplex, 2.0, Scalar.Red);
            Cv2.ImWrite("image\\result.png", targetImage);


            if (maxVal >= threshold)
            {
                // テンプレートが見つかった場合の処理
                Util.writeLog("I", "一致(" + (int)(maxVal*100) + ">=" + ts + ")");

                switch (returnMode)
                {
                    case RETURN_MODE_TOPLEFT:
                        result = maxLoc.X + "," + maxLoc.Y;
                        break;
                    case RETURN_MODE_MIDDLE:
                        result = (maxLoc.X + (templateImage.Size().Width / 2)) + "," + (maxLoc.Y + (templateImage.Size().Height / 2));
                        break;

                }

            }
            else 
            {
                Util.writeLog("I", "不一致(" + (int)(maxVal * 100) + "<" + ts + ")");

            }

            templateImage.Release();
            targetImage.Release();
            resultImage.Release();
            templateImage = null;
            targetImage = null;
            resultImage = null;

            return result;
        }

        static private Mat getScreenshot() {
            // スクリーンのサイズを取得
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

            Rectangle seachRect = new Rectangle();


            // Bitmapオブジェクトを作成して、スクリーンのサイズに合わせる
            using (Bitmap screenshot = new Bitmap(screenBounds.Width, screenBounds.Height))
            {
                // Graphicsオブジェクトを作成し、スクリーン全体をキャプチャ
                using (Graphics graphics = Graphics.FromImage(screenshot))
                {
                    graphics.CopyFromScreen(
                        screenBounds.Left, screenBounds.Top, 0, 0,
                        screenBounds.Size,
                        CopyPixelOperation.SourceCopy);
                }

                // BitmapをMatに変換
                
                Mat mat = BitmapConverter.ToMat(screenshot);

//                Cv2.ImWrite(".\\image\\temp.png", mat);
//                mat = Cv2.ImRead(".\\image\\temp.png");


                return mat;
            }



        }
    }
}
