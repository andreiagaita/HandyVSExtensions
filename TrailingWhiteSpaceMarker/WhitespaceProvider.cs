using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading.Tasks;
using SpoiledCat.Utils.Collections;
using System.Linq;

namespace TrailingWhiteSpaceMarker
{
	[Export]
	internal class WhitespaceProvider
	{
		HashTable<int, SpanCache> _spanCache = new HashTable<int, SpanCache>();

		public IEnumerable<SpanCache> GetCache()
		{
			return _spanCache;
		}

		internal IEnumerable<SnapshotSpan> GetSpans()
		{
			return _spanCache.Cast<SnapshotSpan>();
		}

		internal bool Contains(int lineNumber)
		{
			return _spanCache.Contains(lineNumber);
		}

		internal SnapshotSpan GetSpanFromLineNumber(int lineNumber)
		{
			if (!Contains(lineNumber))
				throw new ArgumentException("Invalid line number", "lineNumber");
			return _spanCache[lineNumber].Span;
		}

		internal IEnumerable<ITrackingSpan> GetTrackingSpans()
		{
			return _spanCache.Select(x => x.TrackingSpan);
		}

		internal SpanCache Remove(int lineNumber)
		{
			return _spanCache.Remove(lineNumber);
		}

		internal void Update(int lineNumber, SnapshotSpan span, ITrackingSpan trackingSpan = null)
		{
			var val = new SpanCache(lineNumber, span, trackingSpan);
			_spanCache.Add(val);
		}

		internal void Update(ITextView textView, int lineNumber, ITrackingSpan span)
		{
			_spanCache[lineNumber].TrackingSpan = span;
		}

		internal void UpdateSpans(ITextView textView, ITextSnapshot snapshot)
		{
			foreach (var line in _spanCache) {
				line.Span = line.Span.TranslateTo(snapshot, SpanTrackingMode.EdgeInclusive);
			}
		}
	}
}
