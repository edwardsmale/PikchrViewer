using System.ComponentModel.Composition;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace PikchrViewer
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(ContentTypes.HTML)]
    [ContentType(ContentTypes.WebForms)]
    [ContentType(ContentTypes.Xml)]
    [ContentType("svg")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal sealed class SvgAdornmentProvider : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView t)
        {
            t.Properties.GetOrCreateSingletonProperty(() => new SvgAdornment(t));
        }
    }
}