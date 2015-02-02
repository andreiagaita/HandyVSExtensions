using System;
using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace SpoiledCat.Utils
{
	public static class Images
	{
		public static BitmapSource MakeBitmapSource(this UIElement control, double width, double height)
		{
			// The control we have been passed is quite possibly the child of
			// another element and we have no idea of what its left/top properties
			// may be. So the first thing we have to do is take a visual copy of it.
			// This will ensure we have a relative 0,0 origin.
			Rectangle rect = PaintControl(control, width, height);

			// Create an in memory xps package to write to            
			using (var stream = new MemoryStream()) {
				Package package = Package.Open(stream, FileMode.CreateNew, FileAccess.ReadWrite);

				// A name (any name) is required when referencing the package.
				const string inMemoryPackageName = "memorystream://out.xps";
				var packageUri = new Uri(inMemoryPackageName);

				// A package, managing the memory stream, is held as a reference.
				PackageStore.AddPackage(packageUri, package);

				// We must keep the document open until we have finished with 
				// the resulting visual, otherwise it wont be able to access 
				// its resources.
				using (var doc = new XpsDocument(package, CompressionOption.Maximum, inMemoryPackageName)) {
					// Print the control using Xps printing.
					Visual capture = PrintToVisual(doc, rect);

					// We want to render the resulting visual into a RenderTarget
					// so that we can create an image from it.
					RenderTargetBitmap renderTarget =
						RenderVisual(capture, width, height);

					// Tidy up
					PackageStore.RemovePackage(packageUri);

					// Voila! The most complicated method of creating an image
					// from a control you've ever seen!
					return renderTarget;
				}
			}
		}
		/// <summary>
		/// Paints a control onto a rectangle. Gets around problems where
		/// the control maybe a child of another element or have a funny
		/// offset.
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		private static Rectangle PaintControl(UIElement control, double width, double height)
		{

			// Fill a rectangle with the illustration.
			var rect = new Rectangle {
				Fill = new VisualBrush(control) { TileMode = TileMode.None, Stretch = Stretch.Uniform },
				Width = width,
				Height = height
			};


			// Force the rectangle to re-size
			var szRect = new Size(rect.Width, rect.Height);
			rect.Measure(szRect);
			rect.Arrange(new Rect(szRect));
			rect.UpdateLayout();
			return rect;
		}

		/// <summary>
		/// Prints any UIElement to an xps document and gets the resulting Visual.
		/// This is the only fool proof way to copy the contents of a UIElement into
		/// a visual. Other methods may work well...but not with WindowsFormsHosts.
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="element"></param>
		/// <returns></returns>
		private static Visual PrintToVisual(XpsDocument doc, Visual element)
		{
			// Write the element to an XpsDocument
			XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);
			writer.Write(element);

			// Get the visual that was 'printed'
			var Sequence = doc.GetFixedDocumentSequence();
			if (Sequence != null) {
				Visual capture = Sequence.DocumentPaginator.GetPage(0).Visual;
				doc.Close();
				return capture;
			}
			return null;
		}

		public static RenderTargetBitmap Render(this UIElement element, Brush background, double width, double height, Rect padding)
		{
			var imageWidth = width - padding.Left - padding.Width;
			var imageHeight = height - padding.Top - padding.Height;
			Rectangle rect = PaintControl(element, imageWidth, imageHeight);

			// Default dpi settings
			const double dpiX = 96;
			const double dpiY = 96;

			var renderTarget = new RenderTargetBitmap(
				(int)width, (int)height, dpiX, dpiY, PixelFormats.Pbgra32);

			DrawingVisual drawingVisual = new DrawingVisual();
			using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
				var r = new Rect(new Point(), new Size(width, height));
				drawingContext.DrawRectangle(background, null, r);
				r = new Rect(new Point(padding.Left, padding.Top), new Size(imageWidth, imageHeight));
				VisualBrush visualBrush = new VisualBrush(rect);
				visualBrush.AlignmentX = AlignmentX.Center;
				visualBrush.AlignmentY = AlignmentY.Top;
				visualBrush.Stretch = Stretch.None;
				drawingContext.DrawRectangle(visualBrush, null, r);
			}
			renderTarget.Render(drawingVisual);

			return renderTarget;

		}

		/// <summary>
		/// Render a Visual to a render target of a fixed size. The visual is
		/// scaled uniformly to fit inside the specified size.
		/// </summary>
		/// <param name="visual"></param>
		/// <param name="height"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		private static RenderTargetBitmap RenderVisual(Visual visual, double height, double width)
		{
			// Default dpi settings
			const double dpiX = 96;
			const double dpiY = 96;

			// We can only render UIElements...ContentPrensenter to 
			// the rescue!
			var presenter = new ContentPresenter { Content = visual };

			// Ensure the final visual is of the known size by creating a viewbox 
			// and adding the visual as its child.
			var viewbox = new Viewbox {
				MaxWidth = width,
				MaxHeight = height,
				Stretch = Stretch.Uniform,
				Child = presenter
			};

			// Force the viewbox to re-size otherwise we wont see anything.
			var sFinal = new Size(viewbox.MaxWidth, viewbox.MaxHeight);
			viewbox.Measure(sFinal);
			viewbox.Arrange(new Rect(sFinal));
			viewbox.UpdateLayout();

			// Render the final visual to a render target 
			var renderTarget = new RenderTargetBitmap(
				(int)width, (int)height, dpiX, dpiY, PixelFormats.Pbgra32);
			renderTarget.Render(viewbox);

			// Return the render taget with the visual rendered on it.
			return renderTarget;
		}
	}
}
