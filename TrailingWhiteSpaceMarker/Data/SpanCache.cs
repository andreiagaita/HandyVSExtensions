using Microsoft.VisualStudio.Text;
using SpoiledCat.Utils.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrailingWhiteSpaceMarker
{
	internal class SpanCache<T> : IKeyedObject<T>
	{
		public T LineNumber;
		public SnapshotSpan Span;
		public ITrackingSpan TrackingSpan;

		public SpanCache(T line)
		{
			this.LineNumber = line;
		}

		public SpanCache(T line, SnapshotSpan span, ITrackingSpan trackingSpan = null)
		{
			this.LineNumber = line;
			this.Span = span;
			this.TrackingSpan = trackingSpan;
		}

		public override int GetHashCode()
		{
			return Convert.ToInt32(LineNumber);
		}

		public static implicit operator SnapshotSpan(SpanCache<T> obj)
		{
			return obj.Span;
		}

		public static implicit operator int(SpanCache<T> obj)
		{
			return Convert.ToInt32(obj.LineNumber);
		}

		public T Key
		{
			get
			{
				return LineNumber;
			}
			private set
			{
				LineNumber = value;
			}
		}
	}

	internal class SpanCache : SpanCache<int>
	{
		public SpanCache(int line, SnapshotSpan span, ITrackingSpan trackingSpan = null) : base(line, span, trackingSpan) { }
	}
}
