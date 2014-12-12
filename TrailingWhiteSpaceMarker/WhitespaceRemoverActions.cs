using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrailingWhiteSpaceMarker
{
	internal class BaseAction : ISmartTagAction
	{
		public System.Windows.Media.ImageSource Icon
		{
			get { return null; }
		}

		public bool IsEnabled
		{
			get { return true; }
		}

		public System.Collections.ObjectModel.ReadOnlyCollection<SmartTagActionSet> ActionSets
		{
			get { return null; }
		}


		public virtual string DisplayText
		{
			get { return ""; }
		}

		public virtual void Invoke()
		{
			throw new NotImplementedException();
		}
	}

	internal class WhitespaceRemoverAction : BaseAction
	{
		ITrackingSpan _span;
		public WhitespaceRemoverAction(ITrackingSpan span)
		{
			_span = span;
		}

		public override string DisplayText
		{
			get { return Resources.RemoveWhitespace; }
		}

		public override void Invoke()
		{
			SnapshotSpan ss = _span.GetSpan(_span.TextBuffer.CurrentSnapshot);
			Span s = new Span(ss.Start, ss.End - ss.Start);
			_span.TextBuffer.Delete(s);
		}
	}

	internal class AllWhitespaceRemoverAction : BaseAction
	{
		Func<ITextView, IEnumerable<ITrackingSpan>> _getAllWhitespace;
		ITextView _textView;
		public AllWhitespaceRemoverAction(ITextView textView, Func<ITextView, IEnumerable<ITrackingSpan>> getAllWhitespace)
		{
			_getAllWhitespace = getAllWhitespace;
			_textView = textView;
		}

		public override string DisplayText
		{
			get { return Resources.RemoveAllWhitespace; }
		}

		public override void Invoke()
		{
			var spans = _getAllWhitespace(_textView);
			var edit = _textView.TextBuffer.CreateEdit();
			foreach (var span in spans) {
				var s = span.GetSpan(span.TextBuffer.CurrentSnapshot);
				edit.Delete(s);
			}
			edit.Apply();
		}
	}
}
