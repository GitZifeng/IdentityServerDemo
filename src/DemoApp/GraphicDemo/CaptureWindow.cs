using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GraphicDemo
{
    public static class CaptureWindow
    {
        #region 类

        /// <summary>
        /// Helper class containing User32 API functions
        /// </summary>
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

            [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        }

        private class Gdi32
        {
            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter

            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        #endregion 类

        /// <summary>
        /// 根据句柄截图
        /// </summary>
        /// <param name="hWnd">句柄</param>
        /// <returns></returns>
        public static Image ByHwnd(IntPtr hWnd)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(hWnd);
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(hWnd, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            IntPtr hdcDest = Gdi32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = Gdi32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            Gdi32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, Gdi32.SRCCOPY);
            // restore selection
            Gdi32.SelectObject(hdcDest, hOld);
            // clean up
            Gdi32.DeleteDC(hdcDest);
            User32.ReleaseDC(hWnd, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            Gdi32.DeleteObject(hBitmap);
            return img;
        }

        /// <summary>
        /// 根据句柄截图，截取指定区域
        /// </summary>
        /// <param name="hWnd">句柄</param>
        /// <param name="x1">起始点X坐标</param>
        /// <param name="y1">起始点Y坐标</param>
        /// <param name="x2">结束点X坐标</param>
        /// <param name="y2">结束点Y坐标</param>
        /// <returns></returns>
        public static Image ByHwnd(IntPtr hWnd, int x1, int y1, int x2, int y2)
        {
            // 计算宽度和高度
            int width = x2 - x1;
            int height = y2 - y1;

            // 获取目标窗口的 hDC
            IntPtr hdcSrc = User32.GetWindowDC(hWnd);

            // 创建一个我们可以拷贝到的设备上下文
            IntPtr hdcDest = Gdi32.CreateCompatibleDC(hdcSrc);

            // 创建一个我们可以拷贝到的位图
            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdcSrc, width, height);

            // 选择位图对象
            IntPtr hOld = Gdi32.SelectObject(hdcDest, hBitmap);

            // 位块传输
            Gdi32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, x1, y1, Gdi32.SRCCOPY);

            // 恢复选择
            Gdi32.SelectObject(hdcDest, hOld);

            // 清理
            Gdi32.DeleteDC(hdcDest);
            User32.ReleaseDC(hWnd, hdcSrc);

            // 获取 .NET 图片对象
            Image img = Image.FromHbitmap(hBitmap);

            // 释放位图对象
            Gdi32.DeleteObject(hBitmap);

            return img;
        }

        /// <summary>
        /// 根据窗口名称截图
        /// </summary>
        /// <param name="windowName">窗口名称</param>
        /// <returns></returns>
        public static Image ByName(string windowName)
        {
            IntPtr handle = User32.FindWindow(null, windowName);
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            IntPtr hdcDest = Gdi32.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOld = Gdi32.SelectObject(hdcDest, hBitmap);
            Gdi32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, Gdi32.SRCCOPY);
            Gdi32.SelectObject(hdcDest, hOld);
            Gdi32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            Image img = Image.FromHbitmap(hBitmap);
            Gdi32.DeleteObject(hBitmap);
            return img;
        }
    }
}