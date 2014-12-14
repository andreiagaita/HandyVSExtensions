using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;

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

		/// <summary>
		/// Go through all the text and create tracking spans for all trailing whitespace
		/// and any empty lines at the end of the document
		/// </summary>
		/// <param name="textView"></param>
		/// <returns></returns>
		public static IEnumerable<ITrackingSpan> CreateWhitespaceTrackers(ITextView textView)
		{
			var snapshot = textView.TextSnapshot;
			bool emptyLinesAtEnd = true;
			int start, end;
			for (int i = snapshot.LineCount - 1; i >= 0; --i) {
				var line = snapshot.GetLineFromLineNumber(i);
				// when removing all trailing whitespace, clear empty lines at end too,
				// but not empty/whitespace only lines in the middle of code
				if (emptyLinesAtEnd && line.Start == line.End) {
					yield return snapshot.CreateTrackingSpan(Span.FromBounds(line.Start, line.EndIncludingLineBreak), SpanTrackingMode.EdgeInclusive);
				} else if (GetBounds(textView, line.Start, line.End, out start, out end)) {
					if (line.Start == start && emptyLinesAtEnd)
						yield return snapshot.CreateTrackingSpan(Span.FromBounds(line.Start, line.EndIncludingLineBreak), SpanTrackingMode.EdgeInclusive);
					else
						yield return snapshot.CreateTrackingSpan(Span.FromBounds(start, end), SpanTrackingMode.EdgeInclusive);
				} else
					emptyLinesAtEnd = false;
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
