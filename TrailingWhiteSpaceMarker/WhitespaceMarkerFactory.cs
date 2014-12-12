using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace TrailingWhiteSpaceMarker
{
	#region Adornment Factory
	/// <summary>
	/// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
	/// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
	/// </summary>

	[Export(typeof(IViewTaggerProvider))]
	[Export(typeof(IWpfTextViewCreationListener))]
	[Order(Before = "default")]
	[TagType(typeof(SmartTag))]
	[ContentType("code")]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	internal sealed class WhitespaceMarkerFactory : IWpfTextViewCreationListener, IViewTaggerProvider
	{
		/// <summary>
		/// Defines the adornment layer for the adornment. This layer is ordered
		/// after the selection layer in the Z-order
		/// </summary>
		[Export(typeof(AdornmentLayerDefinition))]
		[Name("TrailingWhiteSpaceMarker")]
		[Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text)]
		public AdornmentLayerDefinition editorAdornmentLayer = null;

		[Import]
		WhitespaceProvider _wsProvider = null;

		/// <summary>
		/// Instantiates a TrailingWhiteSpaceMarker manager when a textView is created.
		/// </summary>
		/// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
		public void TextViewCreated(IWpfTextView textView)
		{
			//new WhitespaceMarker(textView, wsProvider);
		}

		public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
		{
			if (buffer == null || textView == null) {
				return null;
			}

			//make sure we are tagging only the top buffer
			if (buffer == textView.TextBuffer && textView is IWpfTextView) {
				//return new WhitespaceTagger(buffer, textView, this, _wsProvider) as ITagger<T>;
				return new WhitespaceMarker(textView as IWpfTextView, _wsProvider) as ITagger<T>;
			} else return null;
		}
	}
	#endregion //Adornment Factory
}
