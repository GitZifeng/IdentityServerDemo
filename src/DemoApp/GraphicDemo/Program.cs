using System;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace GraphicDemo
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var getTime = MeasureMethodExecutionTime(() =>
             {
                 for (int i = 0; i < 1; i++)
                 {
                     var image = CaptureWindow.ByHwnd((IntPtr)6232946, 100, 100, 400, 400);
                     image.Save("D:\\test1.bmp", ImageFormat.Bmp);
                     image.Dispose();
                     Thread.Sleep(100);
                 }
             });

            Console.WriteLine(getTime);
        }

        private static double MeasureMethodExecutionTime(Action methodToMeasure)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            methodToMeasure();

            stopwatch.Stop();
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        private static string GetStampTime()
        {
            return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();
        }

        private static async void Capture()
        {
            Console.Write("按任意键开始DX截图……");
            Console.ReadKey();

            string path = @"E:\截图测试";

            var cancel = new CancellationTokenSource();
            await Task.Run(() =>
            {
                Task.Run(() =>
                {
                    Thread.Sleep(5000);
                    cancel.Cancel();
                    Console.WriteLine("DX截图结束！");
                });
                var savePath = $@"{path}\DX";
                Directory.CreateDirectory(savePath);

                using var dx = new DirectXScreenCapturer();
                Console.WriteLine("开始DX截图……");

                while (!cancel.IsCancellationRequested)
                {
                    var (result, isBlackFrame, image) = dx.GetFrameImage();
                    if (result.Success && !isBlackFrame) image.Save($@"{savePath}\{DateTime.Now.Ticks}.jpg", ImageFormat.Jpeg);
                    image?.Dispose();
                }
            }, cancel.Token);

            var windows = WindowEnumerator.FindAll();
            for (int i = 0; i < windows.Count; i++)
            {
                var window = windows[i];
                Console.WriteLine($@"{i.ToString().PadLeft(3, ' ')}. {window.Title}
            {window.Bounds.X}, {window.Bounds.Y}, {window.Bounds.Width}, {window.Bounds.Height}");
            }

            var savePath = $@"{path}\Gdi";
            Directory.CreateDirectory(savePath);
            Console.WriteLine("开始Gdi窗口截图……");

            foreach (var win in windows)
            {
                var image = CaptureWindow.ByHwnd(win.Hwnd);
                image.Save($@"{savePath}\{win.Title.Substring(win.Title.LastIndexOf(@"\") < 0 ? 0 : win.Title.LastIndexOf(@"\") + 1).Replace("/", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace(":", "").Replace("<", "").Replace(">", "").Replace("|", "")}.jpg", ImageFormat.Jpeg);
                image.Dispose();
            }
            Console.WriteLine("Gdi窗口截图结束！");

            Console.ReadKey();
        }
    }
}