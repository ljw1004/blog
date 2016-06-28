// This code is explained here:
// https://blogs.msdn.microsoft.com/lucian/2016/06/27/visual-studio-text-adornment-vsix-using-roslyn/

using System.Linq;

namespace MefRegistration
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Utilities;
    using Microsoft.VisualStudio.Text.Editor;

    [Export(typeof(IWpfTextViewCreationListener)), ContentType("text"), TextViewRole(PredefinedTextViewRoles.Document)]
    public sealed class TextAdornment1TextViewCreationListener : IWpfTextViewCreationListener
    {
        // This class will be instantiated the first time a text document is opened. (That's because our VSIX manifest
        // lists this project, i.e. this DLL, and VS scans all such listed DLLs to find all types with the right attributes).
        // The TextViewCreated event will be raised each time a text document tab is created. It won't be
        // raised for subsequent re-activation of an existing document tab.
        public void TextViewCreated(IWpfTextView textView) => new TextAdornment1(textView);

#pragma warning disable CS0169 // C# warning "the field editorAdornmentLayer is never used" -- but it is used, by MEF!
        [Export(typeof(AdornmentLayerDefinition)), Name("TextAdornment1"), Order(After = PredefinedAdornmentLayers.Selection, Before = Microsoft.VisualStudio.Text.Editor.PredefinedAdornmentLayers.Text)]
        private AdornmentLayerDefinition editorAdornmentLayer;
#pragma warning restore CS0169
    }
}


public sealed class TextAdornment1
{
    private readonly Microsoft.VisualStudio.Text.Editor.IWpfTextView View;
    private Microsoft.CodeAnalysis.Workspace Workspace;
    private Microsoft.CodeAnalysis.DocumentId DocumentId;
    private System.Windows.Controls.TextBlock Adornment;

    public TextAdornment1(Microsoft.VisualStudio.Text.Editor.IWpfTextView view)
    {
        var componentModel = (Microsoft.VisualStudio.ComponentModelHost.IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(Microsoft.VisualStudio.ComponentModelHost.SComponentModel));
        Workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

        View = view;
        View.LayoutChanged += OnLayoutChanged;
    }


    internal void OnLayoutChanged(object sender, Microsoft.VisualStudio.Text.Editor.TextViewLayoutChangedEventArgs e)
    {
        // Raised whenever the rendered text displayed in the ITextView changes - whenever the view does a layout
        // (which happens when DisplayTextLineContainingBufferPosition is called or in response to text or classification
        // changes), and also when the view scrolls or when its size changes.
        // Responsible for adding the adornment to any reformatted lines.

        // This code overlays the document version on line 0 of the file
        if (DocumentId == null)
        {
            var dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            var activeDocument = dte?.ActiveDocument; // sometimes we're constructed/invoked before ActiveDocument has been set
            if (activeDocument != null) DocumentId = Workspace.CurrentSolution.GetDocumentIdsWithFilePath(activeDocument.FullName).FirstOrDefault();
        }

        if (Adornment == null)
        {
            var line = e.NewOrReformattedLines.SingleOrDefault(l => l.Start.GetContainingLine().LineNumber == 0);
            if (line == null) return;
            var geometry = View.TextViewLines.GetMarkerGeometry(line.Extent);
            if (geometry == null) return;
            Adornment = new System.Windows.Controls.TextBlock { Width = 400, Height = geometry.Bounds.Height, Background = System.Windows.Media.Brushes.Yellow, Opacity = 0.5 };
            System.Windows.Controls.Canvas.SetLeft(Adornment, 300);
            System.Windows.Controls.Canvas.SetTop(Adornment, geometry.Bounds.Top);
            View.GetAdornmentLayer("TextAdornment1").AddAdornment(Microsoft.VisualStudio.Text.Editor.AdornmentPositioningBehavior.TextRelative, line.Extent, null, Adornment, (tag, ui) => Adornment = null);
        }

        if (DocumentId != null)
        {
            var document = Workspace.CurrentSolution.GetDocument(DocumentId);
            if (document == null) return;
            Microsoft.CodeAnalysis.VersionStamp version;
            if (!document.TryGetTextVersion(out version)) version = Microsoft.CodeAnalysis.VersionStamp.Default;
            Adornment.Text = version.ToString();
        }
    }
}
