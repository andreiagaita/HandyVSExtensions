using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpoiledCat.Utils.Collections;

namespace TrailingWhiteSpaceMarker
{
	[Export]
	internal class WhitespaceProvider
	{
		HashTable<int, SpanCache> _spanCache = new HashTable<int, SpanCache>();

		IEnumerable<T> GetCache<T>()
		{
			if (typeof(T).IsInterface)
				return _spanCache.Select(x => (T)x.TrackingSpan);
			else
				return _spanCache.Cast<T>();
		}

		public IEnumerable<SpanCache> GetCache()
		{
			return _spanCache;
		}

		internal IEnumerable<SnapshotSpan> GetSpans()
		{
			return GetCache<SnapshotSpan>();
		}

		internal IEnumerable<ITrackingSpan> GetTrackingSpans()
		{
			return GetCache<ITrackingSpan>();
		}

		internal void Remove(int lineNumber)
		{
			_spanCache.Remove(lineNumber);
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
