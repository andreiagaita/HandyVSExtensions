using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace TrailingWhiteSpaceMarker
{
	/// <summary>
	/// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IViewTaggerProvider"/>
	/// that instantiates the adornment and tags it
	/// </summary>
	[Export(typeof(IViewTaggerProvider))]
	[TagType(typeof(SmartTag))]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	[ContentType("code")]
	[Order(Before = "default")]
	internal sealed class WhitespaceMarkerFactory : IViewTaggerProvider
	{
		/// <summary>
		/// Defines the adornment layer for the adornment. This layer is ordered
		/// after the selection layer in the Z-order
		/// </summary>
		[Export(typeof(AdornmentLayerDefinition))]
		[Name("TrailingWhiteSpaceMarker")]
		[Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text)]
		public AdornmentLayerDefinition editorAdornmentLayer = null;

		/// <summary>
		/// Keeps a cache of all the tagged and adorned whitespace we've seen so far
		/// (usually only what has been shown on the viewport so far)
		/// </summary>
		[Import]
		WhitespaceProvider _wsProvider = null;

		public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
		{
			if (buffer == null || textView == null) {
				return null;
			}

			//make sure we are tagging only the top buffer
			if (buffer == textView.TextBuffer && textView is IWpfTextView)
				return new WhitespaceMarker(textView as IWpfTextView, _wsProvider) as ITagger<T>;
			return null;
		}
	}
}
