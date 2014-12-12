using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpoiledCat.Utils
{
	public class VsOutputLogger
	{
		private static Lazy<Action<string>> _Logger = new Lazy<Action<string>>(() => GetWindow().OutputString);

		private static Action<string> Logger
		{
			get { return _Logger.Value; }
		}

		public static void SetLogger(Action<string> logger)
		{
			_Logger = new Lazy<Action<string>>(() => logger);
		}

		public static void Write(string format, params object[] args)
		{
			var message = string.Format(format, args);
			Write(message);
		}

		public static void Write(string message)
		{
			Logger(message + Environment.NewLine);
		}

		private static OutputWindowPane GetWindow()
		{
			var dte = Services.Dte2;
			return dte.ToolWindows.OutputWindow.ActivePane;
		}

		public static void LogToGeneralOutput(string msg)
		{
			Guid generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane; // P.S. There's also the GUID_OutWindowDebugPane available.
			IVsOutputWindowPane generalPane;

			Services.OutputWindow.GetPane(ref generalPaneGuid, out generalPane);
			if (generalPane == null) {
				Services.OutputWindow.CreatePane(ref generalPaneGuid, "Output", 1, 0);
				Services.OutputWindow.GetPane(ref generalPaneGuid, out generalPane);
			}

			generalPane.OutputString(msg);
			generalPane.Activate(); // Brings this pane into view
		}


	}
}
