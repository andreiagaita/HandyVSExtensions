using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrailingWhiteSpaceMarker
{
	internal static class WhitespaceParser
	{
		public static bool GetBounds(ITextView textView, int lineStart, int lineEnd, out int start, out int end)
		{
			start = end = 0;
			if (textView.TextSnapshot.Length == lineEnd ||
				(textView.TextSnapshot.Length > lineEnd &&
				(textView.TextSnapshot[lineEnd] == '\r' || textView.TextSnapshot[lineEnd] == '\n'))) {

				end = lineEnd;
				start = end - 1;
				for (; start >= lineStart; --start) {
					char c = textView.TextSnapshot[start];
					if (!char.IsWhiteSpace(c))
						break;
				}
				++start;
				if (end == start) {
					return false;
				}
				return true;
			}
			return false;
		}

		public static IEnumerable<ITrackingSpan> CreateWhitespaceTrackers(ITextView textView)
		{
			var snapshot = textView.TextSnapshot;
			foreach (var line in snapshot.Lines) {

				int start, end;
				if (GetBounds(textView, line.Start, line.End, out start, out end)) {
					yield return snapshot.CreateTrackingSpan(Span.FromBounds(start, end), SpanTrackingMode.EdgeInclusive);
				}
			}
		}

		public static IEnumerable<SnapshotSpan> CreateWhitespaceSpans(ITextView textView)
		{
			var snapshot = textView.TextSnapshot;
			foreach (var line in snapshot.Lines) {

				int start, end;
				if (GetBounds(textView, line.Start, line.End, out start, out end)) {
					yield return new SnapshotSpan(textView.TextSnapshot, Span.FromBounds(start, end));
				}
			}
		}

	}
}
