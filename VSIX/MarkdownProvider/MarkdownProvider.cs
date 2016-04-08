using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.GraphModel.CodeSchema;
using Microsoft.VisualStudio.GraphModel.Schemas;
using Microsoft.VisualStudio.Shell;

namespace MarkdownSolutionExplorerProvider
{

    internal class MarkdownSchema
    {
        public static GraphSchema Schema = new GraphSchema("MdSchema");
        public static GraphCategory File = Schema.Categories.AddNewCategory("MdFile");
        public static GraphCategory File2HeadingLink = Schema.Categories.AddNewCategory("MdFile2HeadingLink");
        public static GraphCategory Heading = Schema.Categories.AddNewCategory("MdHeading");
        public static GraphNodeIdName MdValueName = GraphNodeIdName.Get("MdValueName", null, typeof(string), true);

        static MarkdownSchema()
        {
            File2HeadingLink.BasedOnCategory = CodeLinkCategories.Contains;
        }
    }


    [GraphProvider(Name = "MarkdownSolutionExplorerProvider")]
    public class MarkdownSolutionExplorerProvider : IGraphProvider
    {
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher; // in case we need to do async work
        [Import(typeof(SVsServiceProvider))] IServiceProvider ServiceProvider { get; set; }


        public void BeginGetGraphData(IGraphContext context)
        {
            if (context.Direction == GraphContextDirection.Self &&
                context.RequestedProperties.Contains(DgmlNodeProperties.ContainsChildren))
            {
                MarkThatNodesHaveChildren(context);
            }

            else if (context.Direction == GraphContextDirection.Self ||
                (context.Direction == GraphContextDirection.Contains && context.NodeCategories.Any(c => c.IsBasedOn(MarkdownSchema.Heading))))
            {
                PopulateChildrenOfNodes(context);
            }

            context.OnCompleted();
        }

        private static void MarkThatNodesHaveChildren(IGraphContext context)
        {
            using (var scope = new GraphTransactionScope())
            {
                foreach (var node in context.InputNodes.Where(IsMdFile))
                {
                    node.SetValue(DgmlNodeProperties.ContainsChildren, true);
                }
                scope.Complete();
            }
        }

        private void PopulateChildrenOfNodes(IGraphContext context)
        {
            using (var scope = new GraphTransactionScope())
            {
                foreach (var file in context.InputNodes.Where(IsMdFile))
                {
                    Graph graph = file.Owner;
                    var fn = file.Id.GetNestedValueByName<Uri>(CodeGraphNodeIdName.File).LocalPath;
                    foreach (var heading in GetMdHeadings(file))
                    {
                        GraphNodeId valueId = file.Id + GraphNodeId.GetPartial(MarkdownSchema.MdValueName, heading.Item1);
                        GraphNode node = graph.Nodes.GetOrCreate(valueId, heading.Item1, MarkdownSchema.Heading);
                        node.SetValue(CodeNodeProperties.SourceLocation, heading.Item2);
                        GraphLink link = graph.Links.GetOrCreate(file, node, null, MarkdownSchema.File2HeadingLink);
                        context.OutputNodes.Add(node);
                    }

                }
                scope.Complete();
            }
        }

        private static readonly string[] KnownMdFileExtensions = { ".md", ".markdown", ".spec", ".cpt" };

        private static bool IsMdFile(GraphNode node)
        {
            var localPath = node.Id.GetNestedValueByName<Uri>(CodeGraphNodeIdName.File).LocalPath;

            return node.HasCategory(CodeNodeCategories.ProjectItem) &&
               KnownMdFileExtensions.Any(knownMdFileExtension =>
                   localPath.EndsWith(knownMdFileExtension));
        }

        private IEnumerable<Tuple<string,SourceLocation>> GetMdHeadings(GraphNode file)
        {
            var path = file.Id.GetNestedValueByName<Uri>(CodeGraphNodeIdName.File).LocalPath;
            var lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].StartsWith("#")) continue;
                yield return Tuple.Create(lines[i], new SourceLocation(path, new Position(i + 1, 0)));
            }
        }

        public T GetExtension<T>(GraphObject graphObject, T previous) where T : class
            => typeof(T) == typeof(IGraphNavigateToItem) ? new GraphNodeNavigator() { serviceProvider = ServiceProvider } as T : null;

        class GraphNodeNavigator : IGraphNavigateToItem
        {
            public IServiceProvider serviceProvider;

            public int GetRank(GraphObject graphObject) => 0; // not sure what this is for

            public void NavigateTo(GraphObject graphObject)
            {
                var loc = graphObject.GetValue<SourceLocation>(CodeNodeProperties.SourceLocation);
                var dte = serviceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
                if (loc.FileName != null) dte?.ExecuteCommand("File.OpenFile", "\"" + loc.FileName.LocalPath + "\"");
                if (loc.StartPosition.Line > 0) dte?.ExecuteCommand("Edit.GoTo", loc.StartPosition.Line.ToString());
            }
        }

        public Graph Schema => null; // only for architecture explorer
        public IEnumerable<GraphCommand> GetCommands(IEnumerable<GraphNode> nodes) => new GraphCommand[] { }; // not sure what this is for
        public static Graph GetGraphResource(string name, params GraphSchema[] schemas) => null; // not sure what this is for
    }
}
