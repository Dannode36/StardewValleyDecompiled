using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley;

public static class KeyboardInput
{
	private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	private static bool initialized;

	private static IntPtr prevWndProc;

	private static WndProc hookProcDelegate;

	private static IntPtr hIMC;

	private const int GWL_WNDPROC = -4;

	private const int WM_KEYDOWN = 256;

	private const int WM_KEYUP = 257;

	private const int WM_CHAR = 258;

	private const int WM_IME_SETCONTEXT = 641;

	private const int WM_INPUTLANGCHANGE = 81;

	private const int WM_GETDLGCODE = 135;

	private const int DLGC_WANTALLKEYS = 4;

	/// <summary>
	/// Event raised when a character has been entered.
	/// </summary>
	public static event CharEnteredHandler CharEntered;

	/// <summary>
	/// Event raised when a key has been pressed down. May fire multiple times due to keyboard repeat.
	/// </summary>
	public static event KeyEventHandler KeyDown;

	/// <summary>
	/// Event raised when a key has been released.
	/// </summary>
	public static event KeyEventHandler KeyUp;

	[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr ImmGetContext(IntPtr hWnd);

	[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	/// <summary>
	/// Initialize the TextInput with the given GameWindow.
	/// </summary>
	/// <param name="window">The XNA window to which text input should be linked.</param>
	public static void Initialize(GameWindow window)
	{
		if (initialized)
		{
			throw new InvalidOperationException("TextInput.Initialize can only be called once!");
		}
		hookProcDelegate = HookProc;
		prevWndProc = (IntPtr)SetWindowLong(window.Handle, -4, (int)Marshal.GetFunctionPointerForDelegate(hookProcDelegate));
		hIMC = ImmGetContext(window.Handle);
		initialized = true;
	}

	private static IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
	{
		IntPtr returnCode = CallWindowProc(prevWndProc, hWnd, msg, wParam, lParam);
		switch (msg)
		{
		case 135u:
			returnCode = (IntPtr)(returnCode.ToInt32() | 4);
			break;
		case 256u:
			KeyboardInput.KeyDown?.Invoke(null, new KeyEventArgs((Keys)(int)wParam));
			break;
		case 257u:
			KeyboardInput.KeyUp?.Invoke(null, new KeyEventArgs((Keys)(int)wParam));
			break;
		case 258u:
			KeyboardInput.CharEntered?.Invoke(null, new CharacterEventArgs((char)(int)wParam, lParam.ToInt32()));
			break;
		case 641u:
			if (wParam.ToInt32() == 1)
			{
				ImmAssociateContext(hWnd, hIMC);
			}
			break;
		case 81u:
			ImmAssociateContext(hWnd, hIMC);
			returnCode = (IntPtr)1;
			break;
		}
		return returnCode;
	}
}
