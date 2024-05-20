using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.CodeDom;
using System.Runtime.Caching;

namespace SetPasswordOnPopup
{
	internal class Program
	{
		delegate bool EnumWindowProc(IntPtr hWnd, IntPtr lParam);

		private static readonly MemoryCache cache = MemoryCache.Default;

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
		[DllImport("user32.dll")]
		static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowProc lpEnumFunc, IntPtr lParam);
		[DllImport("user32.dll")]
		static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, StringBuilder lParam);

		const int WM_SETTEXT = 0x000C;
		const int WM_GETTEXT = 0x000D;

		private class GlobalVars
		{
			public string PreviousWindow { get; set; }
			public string UserName { get; set; }
		}
		private class ResourceHandle
		{
			public GlobalVars Vars { get; set; }
			public GCHandle GCHandle { get; set; }
		}

		static void Main(string[] args)
		{
			Process process = Process.GetCurrentProcess();
			process.PriorityClass = ProcessPriorityClass.Idle;

			IntPtr previousForegroundWindowHandle = IntPtr.Zero;

			while (true)
			{
				IntPtr currentForegroundWindowHandle = GetForegroundWindow();
				if (currentForegroundWindowHandle != previousForegroundWindowHandle)
				{
					previousForegroundWindowHandle = currentForegroundWindowHandle;

					string windowTitle = GetWindowTitle(currentForegroundWindowHandle);
					Console.WriteLine(windowTitle);

					if (windowTitle.Contains("Log In to M-Files"))
					{
						SetPassword(currentForegroundWindowHandle);
					}
				}
				System.Threading.Thread.Sleep(500);
			}
		}
		static string GetWindowTitle(IntPtr windowHandle)
		{
			const int nChars = 256;
			StringBuilder sb = new StringBuilder(nChars);
			if (GetWindowText(windowHandle, sb, nChars) > 0)
			{
				return sb.ToString();
			}
			return "unkown";
		}
		static void SetPassword(IntPtr windowHandle)
		{
			GlobalVars globalVars = new GlobalVars();

			GCHandle gCHandle = GCHandle.Alloc(globalVars);
			IntPtr lParam = (IntPtr)gCHandle;

			EnumChildWindows(windowHandle, GetChildWindowsCallBack, lParam);
			EnumChildWindows(windowHandle, SetChildWindowsCallBack, lParam);

			gCHandle.Free();
		}
		static bool GetChildWindowsCallBack(IntPtr hWnd, IntPtr lParam)
		{
			ResourceHandle resHandle = IntPtrsToVars(lParam);

			string previousWindow = resHandle.Vars.PreviousWindow;
			StringBuilder windowText = new StringBuilder(256);
			GetWindowText(hWnd, windowText, 256);
			Console.WriteLine(windowText.ToString());

			if (previousWindow == "&Username:")
			{
				StringBuilder sb = new StringBuilder(2048);
				InputFieldHandler(hWnd, WM_GETTEXT, new IntPtr(sb.Capacity), sb);
				resHandle.Vars.UserName = sb.ToString();
				resHandle.Vars.PreviousWindow = "";
				return true;
			}
			else
			{
				resHandle.Vars.PreviousWindow = windowText.ToString();
			}
			return true;
		}
		static bool SetChildWindowsCallBack(IntPtr hWnd, IntPtr lParam)
		{
			ResourceHandle resHandle = IntPtrsToVars(lParam);
			if (resHandle.Vars == null)
				return true;

			string previousWindow = resHandle.Vars.PreviousWindow;
			string userName = resHandle.Vars.UserName;

			StringBuilder windowText = new StringBuilder(256);
			GetWindowText(hWnd, windowText, 256);

			string mfilesUser = $"{Environment.GetEnvironmentVariable("MFILES_TEST_USERNAME")}@{Environment.GetEnvironmentVariable("MFILES_TEST_DOMAIN")}";
			if (previousWindow == "&Password:" && userName == mfilesUser)
			{
				InputFieldHandler(hWnd, WM_SETTEXT, IntPtr.Zero, new StringBuilder(Environment.GetEnvironmentVariable("MFILES_TEST_PASSWORD")));
				resHandle.GCHandle.Free();
				return true;
			}
			else
			{
				resHandle.Vars.PreviousWindow = windowText.ToString();
			}
			return true;
		}
		static void InputFieldHandler(IntPtr inputFieldHandle, int msg, IntPtr wParam, StringBuilder value)
		{
			if (inputFieldHandle != IntPtr.Zero)
				SendMessage(inputFieldHandle, msg, wParam, value);
		}

		static ResourceHandle IntPtrsToVars(IntPtr ptr)
		{
			GCHandle gCHandle = (GCHandle)ptr;
			return new ResourceHandle()
			{
				Vars = (gCHandle.Target as GlobalVars),
				GCHandle = gCHandle
			};
		}
	}
}
