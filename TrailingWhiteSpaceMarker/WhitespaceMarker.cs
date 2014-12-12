using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Data;
using System.Windows.Resources;

using System.Windows.Markup;
using System.Reflection;
using SpoiledCat.Utils;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Language.Intellisense;
using System.Linq;
using System.Windows.Shapes;

namespace TrailingWhiteSpaceMarker
{

	///<summary>
	///TrailingWhiteSpaceMarker surrounds trailing whitespace with a box
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
			var b = view.Background as SolidColorBrush;
			_backgroundColor = b.Color;
			var color = b.Color.ToComplement();
			var pathBrush = new SolidColorBrush(color);

			_visualBrush = new VisualBrush();
			using (var s = System.IO.File.OpenRead(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "images", "trollface.xaml"))) {
				var xamlImg = XamlReader.Load(s) as Page;
				if (xamlImg != null && xamlImg.Content is Canvas) {
					foreach (var child in (xamlImg.Content as Canvas).Children) {
						var path = child as System.Windows.Shapes.Path;
						if (path == null)
							continue;
						path.Stroke = pathBrush;
					}
					_visualBrush.Visual = xamlImg;
				}
			}

			Brush penBrush = new SolidColorBrush(Colors.Red);
			penBrush.Freeze();
			Pen pen = new Pen(penBrush, 0.5);
			pen.Freeze();

			_brush = _visualBrush;
			_pen = pen;
		}


		/// <summary>
		/// On layout change add the adornment to any reformatted lines
		/// </summary>
		private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
		{
			var spans = UpdateWhitespace(e.NewOrReformattedLines);
			if (TagsChanged != null) {
				foreach (var span in spans)
					TagsChanged(this, new SnapshotSpanEventArgs(span));
			}
			UpdateVisuals(spans, e.NewSnapshot);
		}


		private IEnumerable<SnapshotSpan> UpdateWhitespace(IEnumerable<ITextViewLine> lines)
		{
			foreach (ITextViewLine line in lines) {
				int start, end;
				if (!WhitespaceParser.GetBounds(_textView, line.Start, line.End, out start, out end)) {
					//no trailing whitespace on this line, remove it from the provider list
					_wsProvider.Remove(_textView.TextSnapshot.GetLineNumberFromPosition(line.Start));
					continue;
				}

				SnapshotSpan span = new SnapshotSpan(_textView.TextSnapshot, Span.FromBounds(start, end));
				_wsProvider.Update(span.Snapshot.GetLineNumberFromPosition(line.Start), span);
				yield return span;
			}
		}

		private void UpdateVisuals(IEnumerable<SnapshotSpan> spans, ITextSnapshot snapshot)
		{
			//grab a reference to the lines in the current TextView 
			IWpfTextViewLineCollection textViewLines = _textView.TextViewLines;

			foreach (var span in spans) {

				Geometry g = textViewLines.GetMarkerGeometry(span);

				var b = _textView.Background as SolidColorBrush;
				// the very first time we do this, get the size of the whitespace box and render the image to that so we can tile it and it looks good
				// update if font size changes or background color changes
				var rect = textViewLines.GetMarkerGeometry(new SnapshotSpan(_textView.TextSnapshot, Span.FromBounds(span.Start, span.Start + 1))).Bounds;
				if (_brush is VisualBrush || b.Color != _backgroundColor || _glyphRect.Width != rect.Width || _glyphRect.Height != rect.Height) {
					var xamlImg = _visualBrush.Visual as Page;
					_backgroundColor = b.Color;
					var color = _backgroundColor.ToComplement();
					var pathBrush = new SolidColorBrush(color);

					if (xamlImg != null && xamlImg.Content is Canvas) {
						foreach (var child in (xamlImg.Content as Canvas).Children) {
							var path = child as System.Windows.Shapes.Path;
							if (path == null)
								continue;
							path.Stroke = pathBrush;
						}
					}

					_glyphRect = rect;
					int padding = 0;
					var source = (_visualBrush.Visual as UIElement).Render(Brushes.Transparent, _glyphRect.Height+padding, _glyphRect.Height+padding, new Rect(0, 0, padding, padding));
					ImageBrush brush = new ImageBrush(source);
					brush.TileMode = TileMode.Tile;
					brush.Stretch = Stretch.None;
					brush.Viewbox = new Rect(0, 0, _glyphRect.Height+padding, _glyphRect.Height+padding);
					brush.ViewboxUnits = BrushMappingMode.Absolute;
					brush.Viewport = new Rect(0, 0, _glyphRect.Height, _glyphRect.Height);
					brush.ViewportUnits = BrushMappingMode.Absolute;
					_brush = brush;
					_brush.Freeze();
				}

				if (g != null) {
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

		public IEnumerable<ITagSpan<WhitespaceTag>> GetTags(NormalizedSnapshotSpanCollection changedSpans)
		{
			ITextSnapshot snapshot = _textView.TextSnapshot;
			if (snapshot.Length == 0)
				yield break; //don't do anything if the buffer is empty 

			var cache = _wsProvider.GetCache();
			var spans = cache.Where(x => changedSpans.Contains(x.Span)); // get only the ones being requested

			foreach (var span in spans) {
				yield return new TagSpan<WhitespaceTag>(span.Span, new WhitespaceTag(GetSmartTagActions(span.Span)));
			}
		}

		private ReadOnlyCollection<SmartTagActionSet> GetSmartTagActions(SnapshotSpan span)
		{
			List<SmartTagActionSet> actionSetList = new List<SmartTagActionSet>();
			List<ISmartTagAction> actionList = new List<ISmartTagAction>();

			ITrackingSpan trackingSpan = span.Snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);
			actionList.Add(new WhitespaceRemoverAction(trackingSpan));
			actionList.Add(new AllWhitespaceRemoverAction(_textView, WhitespaceParser.CreateWhitespaceTrackers));
			SmartTagActionSet actionSet = new SmartTagActionSet(actionList.AsReadOnly());
			actionSetList.Add(actionSet);
			return actionSetList.AsReadOnly();
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
