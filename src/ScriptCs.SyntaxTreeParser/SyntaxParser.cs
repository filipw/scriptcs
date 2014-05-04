namespace ScriptCs.SyntaxTreeParser
{
    using System;
    using System.Linq;
    using System.Text;

    using ICSharpCode.NRefactory.CSharp;
    using ICSharpCode.NRefactory.Editor;
    using ICSharpCode.NRefactory.CSharp.Refactoring;

    using ScriptCs.SyntaxTreeParser.Visitors;

    public class SyntaxParser
    {
        public ParseResult Parse(string code)
        {
            var result = new ParseResult
                             {
                                 Evaluations = this.WrapClass(code)
                             };
            result = this.ExtractClassDeclarations(result);
            result = this.ExtractMethodDeclaration(result);

            result.Evaluations = this.UnWrapClass(result.Evaluations);

			return result;
		}

        private ParseResult ExtractClassDeclarations(ParseResult code)
        {
            var visitor = new ClassTypeVisitor();
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(code.Evaluations);
            syntaxTree.AcceptVisitor(visitor);
            syntaxTree.Freeze();

            var document = new StringBuilderDocument(code.Evaluations);
            using (
                var script = new DocumentScript(
                    document,
                    FormattingOptionsFactory.CreateAllman(),
                    new TextEditorOptions()))
            {
                foreach (var klass in visitor.GetClassDeclarations())
                {
                    var src = klass.GetText();
                    if (src.StartsWith("class Temp_"))
                    {
                        continue;
                    }
                    code.TypeDeclarations += src;
                    var offset = script.GetCurrentOffset(klass.GetRegion().Begin);
                    script.Replace(klass, new TypeDeclaration());
                    script.Replace(offset, new TypeDeclaration().GetText().Length, "");
                }
                code.Evaluations = document.Text;
            }
            return code;
        }

        public ParseResult ExtractMethodDeclaration(ParseResult code)
		{
			var visitor = new MethodVisitor();
			var par = new CSharpParser();
            var syntaxTree = par.Parse(code.Evaluations);
			syntaxTree.AcceptVisitor(visitor);
			syntaxTree.Freeze();

		    var document = new StringBuilderDocument(code.Evaluations);
		    using (var script = new DocumentScript(
                document, 
                FormattingOptionsFactory.CreateAllman(), 
                new TextEditorOptions()))
		    {
                foreach (var method in visitor.GetMethodDeclarations())
				{
					var oldMethod = method.Item1;
					var newMethod = method.Item2;
                    code.MethodDeclarations += newMethod.GetText();

                    var offset = script.GetCurrentOffset(oldMethod.GetRegion().Begin);
                    script.Replace(oldMethod, new MethodDeclaration());
                    script.Replace(offset, new MethodDeclaration().GetText().Length, "");
				}
			}
            code.Evaluations = document.Text;
            return code;
		}

        private string WrapClass(string code)
        {
            var sb = new StringBuilder();
            sb.Append("class ");
            sb.Append("Temp_");
            sb.Append(Guid.NewGuid().ToString().Replace('-', '_'));
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append(code);
            sb.Append(Environment.NewLine);
            sb.Append("}");

            return sb.ToString();
        }

        private string UnWrapClass(string code)
        {
            var evalFull = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            return string.Join(Environment.NewLine, evalFull.Skip(1).Take(evalFull.Count() - 2));
        }
    }
}
