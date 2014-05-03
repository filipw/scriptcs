using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.CSharp.Refactoring;

namespace ScriptCs.SyntaxTreeParser
{
    public class SyntaxParser
    {
        public ParseResult Parse(string code)
        {
			var parser = new CSharpParser();
			var syntaxTree = parser.Parse(code);

			var result = new ParseResult();
			foreach (var correct in syntaxTree.Members)
            {
                var element = correct.GetText();
                result.Declarations += element;
			}

			foreach(var error in syntaxTree.Errors)
			{
				var codeLines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
				result.Evaluations += string.Join(Environment.NewLine, codeLines.Skip(error.Region.BeginLine - 1));
			}
			return result;
		}

		public string ParseEval(string eval)
		{
			// create temp class around evaluation
			var sb = new StringBuilder();
			sb.Append("class ");
			sb.Append("Temp_");
			sb.Append(Guid.NewGuid().ToString().Replace('-','_'));
			sb.Append("{");
			sb.Append(Environment.NewLine);
			sb.Append(eval);
			sb.Append(Environment.NewLine);
			sb.Append("}");

			// collect methods as field def.
			var visit = new MethodVisitor();
			var par = new CSharpParser();
			var tree = par.Parse(sb.ToString());
			tree.AcceptVisitor(visit);
			tree.Freeze();

			// replace old method decl as named func expr
			var document = new StringBuilderDocument(sb.ToString());
			using (var script = new DocumentScript(document, FormattingOptionsFactory.CreateAllman(), new TextEditorOptions())) {

				foreach(var method in visit.Methods)
				{
					var oldMethod = method.Item1;
					var newMethod = method.Item2;

					//options 1
					//var offset = script.GetCurrentOffset(oldMethod.GetRegion().Begin);
					//var length = oldMethod.GetRegion().End.Column - oldMethod.GetRegion().Begin.Column;
					//script.Replace(offset, length, newMethod.GetText());

					//options 2
					script.Replace(oldMethod, newMethod);
				}
			}

			// remove temp class
			var evalFull = document.Text
				.Split(new [] { Environment.NewLine }, StringSplitOptions.None);
			return string.Join(Environment.NewLine, evalFull
				.Skip(1)
				.Take(evalFull.Count() - 2));
		}
    }
}
