using UnityEngine;
using System.Runtime.InteropServices;

public class WindowResizer : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(int hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

    public int windowWidth = 800; // 设置窗口的宽度
    public int windowHeight = 600; // 设置窗口的高度

    void Start()
    {
        SetWindowSizeAndPosition(windowWidth, windowHeight);
    }

    // 调整窗口大小和位置的方法
    private void SetWindowSizeAndPosition(int width, int height)
    {
        int hwnd = GetActiveWindow();

        int screenWidth = Screen.currentResolution.width;
        int screenHeight = Screen.currentResolution.height;

        int windowX = (screenWidth - width) / 2; // 居中设置窗口的X坐标
        int windowY = (screenHeight - height) / 2; // 居中设置窗口的Y坐标

        SetWindowPos(hwnd, 0, windowX, windowY, width, height, 0);
    }

    // 获取活动窗口的方法
    [DllImport("user32.dll")]
    private static extern int GetActiveWindow();
}