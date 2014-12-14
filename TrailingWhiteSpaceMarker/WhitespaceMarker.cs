using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using SpoiledCat.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace TrailingWhiteSpaceMarker
{

	///<summary>
	///TrailingWhiteSpaceMarker surrounds trailing whitespace with a box and actions for removing it
	///</summary>
	internal class WhitespaceMarker : ITagger<WhitespaceTag>, IDisposable
	{
		private IAdornmentLayer _layer;
		private IWpfTextView _textView;
		private Brush _brush;
		private Pen _pen;
		private bool _disposed;
		private Rect _glyphRect;
		private VisualBrush _visualBrush;
		private Color _backgroundColor;

		private WhitespaceProvider _wsProvider;

		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

		public WhitespaceMarker(IWpfTextView view, WhitespaceProvider wsProvider)
		{
			_textView = view;
			_layer = view.GetAdornmentLayer("TrailingWhiteSpaceMarker");
			_wsProvider = wsProvider;
			_textView.LayoutChanged += OnLayoutChanged;
			// set color of image to contrast to background
			_backgroundColor = (view.Background as SolidColorBrush).Color;

			_visualBrush = new VisualBrush();
			using (var s = System.IO.File.OpenRead(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "images", "trollface.xaml"))) {
				_visualBrush.Visual = XamlReader.Load(s) as Page;
			}

			Brush penBrush = new SolidColorBrush(Colors.Red);
			penBrush.Freeze();
			Pen pen = new Pen(penBrush, 0.5);
			pen.Freeze();

			_brush = _visualBrush;
			_pen = pen;
		}


		/// <summary>
		/// On layout change add the adornment and tags to any reformatted lines
		/// </summary>
		private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
		{
			var spans = GetWhitespace(e.NewOrReformattedLines);
			RenderVisuals(spans, e.NewSnapshot);

			// signal that tags need changing, with the old (removed) and the new (added) spans
			if (TagsChanged != null) {
				foreach (var span in spans)
					TagsChanged(this, new SnapshotSpanEventArgs(span));
			}
		}

		private IEnumerable<SnapshotSpan> GetWhitespace(IEnumerable<ITextViewLine> lines)
		{
			List<SnapshotSpan> spans = new List<SnapshotSpan>();
			foreach (ITextViewLine line in lines) {
				int start, end;
				SnapshotSpan span;
				if (line.Start == line.End || !WhitespaceParser.GetBounds(_textView, line.Start, line.End, out start, out end)) {
					//no trailing whitespace on this line, remove it from the provider list and create an empty span so
					// we can remove existing tags later on
					var old = _wsProvider.Remove(_textView.TextSnapshot.GetLineNumberFromPosition(line.Start));
					if (old == null)
						continue;
					span = old.Span;
				} else {
					span = new SnapshotSpan(_textView.TextSnapshot, Span.FromBounds(start, end));
					_wsProvider.Update(span.Snapshot.GetLineNumberFromPosition(line.Start), span);
				}
				spans.Add(span);
			}
			return spans;
		}

		private void RenderVisuals(IEnumerable<SnapshotSpan> spans, ITextSnapshot snapshot)
		{
			//grab a reference to the lines in the current TextView
			IWpfTextViewLineCollection textViewLines = _textView.TextViewLines;

			bool updatedBrushes = false;

			foreach (var span in spans) {
				// only draw the existing trailing whitespace spans
				if (!_wsProvider.Contains(snapshot.GetLineNumberFromPosition(span.Start)))
					continue;

				Geometry g = textViewLines.GetMarkerGeometry(span);
				if (g != null) {

					// the first time we render, get the size of a whitespace character
					// and render the glyph to that size so we can tile it and it looks good
					// also do it we font size changes or background color changes
					if (!updatedBrushes) {
						var rect = textViewLines.GetMarkerGeometry(new SnapshotSpan(_textView.TextSnapshot, Span.FromBounds(span.Start, span.Start + 1))).Bounds;
						UpdateGlyphBrush(rect);
					}

					GeometryDrawing drawing = new GeometryDrawing(_brush, _pen, g);
					drawing.Freeze();

					DrawingImage drawingImage = new DrawingImage(drawing);
					drawingImage.Freeze();

					Image image = new Image();
					image.Source = drawingImage;

					//Align the image with the top of the bounds of the text geometry
					Canvas.SetLeft(image, g.Bounds.Left);
					Canvas.SetTop(image, g.Bounds.Top);

					_layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
				}
			}
		}

		// render the glyph to the appropriate size and created a tiled brush if we haven't done it yet
		// or if the font or background color changes.
		private void UpdateGlyphBrush(Rect rect)
		{
			var b = _textView.Background as SolidColorBrush;
			if (_brush is VisualBrush || b.Color != _backgroundColor || _glyphRect.Width != rect.Width || _glyphRect.Height != rect.Height) {
				_backgroundColor = b.Color;

				var glyph = _visualBrush.Visual as Page;
				if (glyph == null) {
					_brush = new SolidColorBrush(Colors.Yellow.ModifyBrightness(1.4));
					return;
				}

				var color = _backgroundColor.ToComplement();
				var pathBrush = new SolidColorBrush(color);

				if (glyph != null && glyph.Content is Canvas) {
					foreach (var child in (glyph.Content as Canvas).Children) {
						var path = child as System.Windows.Shapes.Path;
						if (path == null)
							continue;
						path.Stroke = pathBrush;
					}
				}

				_glyphRect = rect;

				int padding = 0;
				var source = glyph.Render(Brushes.Transparent,
					_glyphRect.Height + padding,
					_glyphRect.Height + padding,
					new Rect(0, 0, padding, padding));

				ImageBrush brush = new ImageBrush(source);
				brush.TileMode = TileMode.Tile;
				brush.Stretch = Stretch.None;
				brush.Viewbox = new Rect(0, 0, _glyphRect.Height + padding, _glyphRect.Height + padding);
				brush.ViewboxUnits = BrushMappingMode.Absolute;
				brush.Viewport = new Rect(0, 0, _glyphRect.Height, _glyphRect.Height);
				brush.ViewportUnits = BrushMappingMode.Absolute;
				_brush = brush;
				_brush.Freeze();
			}

		}

		/// <summary>
		/// Callback to update tags
		/// </summary>
		/// <param name="changedSpans">The spans to update</param>
		/// <returns></returns>
		public IEnumerable<ITagSpan<WhitespaceTag>> GetTags(NormalizedSnapshotSpanCollection changedSpans)
		{
			ITextSnapshot snapshot = _textView.TextSnapshot;
			if (snapshot.Length == 0)
				yield break; //don't do anything if the buffer is empty

			foreach (var span in changedSpans) {
				int key = snapshot.GetLineNumberFromPosition(span.Start);
				if (_wsProvider.Contains(key))
					yield return new TagSpan<WhitespaceTag>(span, new WhitespaceTag(GetSmartTagActions(span)));
			}
		}

		private ReadOnlyCollection<SmartTagActionSet> GetSmartTagActions(SnapshotSpan span)
		{
			ITrackingSpan trackingSpan = span.Snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);

			List<ISmartTagAction> actionList = new List<ISmartTagAction>();
			actionList.Add(new WhitespaceRemoverAction(trackingSpan));
			actionList.Add(new AllWhitespaceRemoverAction(_textView, WhitespaceParser.CreateWhitespaceTrackers));

			return new List<SmartTagActionSet>() {
				new SmartTagActionSet(actionList.AsReadOnly())
			}.AsReadOnly();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this._disposed) {
				if (disposing) {
					_textView.LayoutChanged -= OnLayoutChanged;
					_textView = null;
				}
				_disposed = true;
			}
		}
	}
}
