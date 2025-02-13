using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CameraControl
{
    public class EnforceSquareWindow : MonoBehaviour
    {
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_FRAMECHANGED = 0x0020;

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        private IntPtr unityWindow;

        private struct Rect
        {
            public int left, top, right, bottom;
        }

#if !UNITY_EDITOR

        void Start()
        {
            unityWindow = GetActiveWindow();
        }

        void Update()
        {
            if (unityWindow == IntPtr.Zero) return;

            Rect winRect;
            if (GetWindowRect(unityWindow, out winRect))
            {
                int width = winRect.right - winRect.left;
                int height = winRect.bottom - winRect.top;

                // Find the smaller dimension and enforce it
                int newSize = Mathf.Max(width, height);

                SetWindowPos(unityWindow, IntPtr.Zero, winRect.left, winRect.top, newSize, newSize, SWP_NOZORDER | SWP_FRAMECHANGED);
            }
        }
#endif
    }
}