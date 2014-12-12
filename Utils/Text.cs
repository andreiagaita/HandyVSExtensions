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
	public class Text
	{
		public static IWpfTextViewHost GetCurrentViewHost()
		{
			var txtMgr = SpoiledCat.Utils.Services.TextManager;
			IVsTextView tv = null;
			txtMgr.GetActiveView(1, null, out tv);
			IVsUserData userData = tv as IVsUserData;

			if (userData == null)
				return null;

			IWpfTextViewHost viewHost;
			object holder;
			Guid guidViewHost = Microsoft.VisualStudio.Editor.DefGuidList.guidIWpfTextViewHost;
			userData.GetData(ref guidViewHost, out holder);
			viewHost = (IWpfTextViewHost)holder;
			return viewHost;
		}

		public static ITextDocument GetTextDocumentForView(ITextDocumentFactoryService factory, IWpfTextViewHost viewHost)
		{

			ITextView textView = viewHost.TextView;
			ITextDataModel textDataModel = textView != null ? textView.TextDataModel : null;
			ITextBuffer documentBuffer = textDataModel != null ? textDataModel.DocumentBuffer : null;
			if (documentBuffer == null)
				return null;

			ITextDocument doc;
			if (!factory.TryGetTextDocument(documentBuffer, out doc))
				return null;
			return doc;
		}

	}
}
