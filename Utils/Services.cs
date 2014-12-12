using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpoiledCat.Utils
{
	public static class Services
	{
		public static IComponentModel ComponentModel
		{
			get { return Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel; }
		}

		public static IVsWebBrowsingService WebBrowsingService
		{
			get { return Package.GetGlobalService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService; }
		}

		public static IVsTextManager TextManager
		{
			get { return Package.GetGlobalService(typeof(SVsTextManager)) as IVsTextManager; }
		}

		public static IVsOutputWindow OutputWindow
		{
			get { return Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow; }
		}

		public static IVsSolution Solution
		{
			get { return Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution; }
		}

		public static IVsUIShell Shell
		{
			get { return Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell; }
		}

		public static DTE Dte
		{
			get {  return Package.GetGlobalService(typeof(DTE)) as DTE; }
		}

		public static DTE2 Dte2
		{
			get { return Dte as DTE2; }
		}

	}

}
