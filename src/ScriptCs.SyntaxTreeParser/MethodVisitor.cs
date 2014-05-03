using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;

namespace ScriptCs.SyntaxTreeParser
{
	public class MethodVisitor : DepthFirstAstVisitor
	{
		public List<Tuple<MethodDeclaration, FieldDeclaration>> Methods 
			= new List<Tuple<MethodDeclaration, FieldDeclaration>>();

		public override void VisitMethodDeclaration (MethodDeclaration methodDeclaration)
		{
			//get method parameters
			IEnumerable<ParameterDeclaration> parameters = methodDeclaration
				.GetChildrenByRole(Roles.Parameter)
				.Select(x => (ParameterDeclaration)x.Clone());

			//create new method type 
			var methodType = new SimpleType(Identifier.Create("Func"));

			//add parameter types
			methodType
				.TypeArguments
				.AddRange(parameters
				.Select(x => GetKeywordAsPrimitiveType(x)));

			//add result type
			methodType
				.TypeArguments
				.Add(GetKeywordAsPrimitiveType(methodDeclaration));

			//get method body
			BlockStatement methodBody = (BlockStatement)methodDeclaration
				.GetChildrenByRole(Roles.Body)
				.FirstOrDefault()
				.Clone();

			//get methodName
			var methodName = GetIdentifierName(methodDeclaration);

			//create named method expression
			var methodExpression = new VariableInitializer(methodName, 
				new AnonymousMethodExpression(methodBody, parameters));

			//compose type and expression
			var namedMethodExpr = new FieldDeclaration();
			namedMethodExpr.ReturnType = methodType;
			namedMethodExpr.Variables.Add(methodExpression);

			this.Methods.Add(new Tuple<MethodDeclaration, FieldDeclaration>
				(methodDeclaration, namedMethodExpr));


			//replace method declaration with named method expresssion
			methodDeclaration.ReplaceWith(namedMethodExpr);
		}

		public string GetIdentifierName(AstNode node)
		{
			foreach(var child in node.GetChildrenByRole(Roles.Identifier))
			{
				foreach(var p in child
					.GetType()
					.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
					.Where(x => x.Name == "Name"))
				{
					var obj = p.GetValue(child, null);
					return obj.ToString();
				}
			}
			return string.Empty;
		}

		public PrimitiveType GetKeywordAsPrimitiveType(AstNode node)
		{
			foreach(var child in node.GetChildrenByRole(Roles.Type))
			{
				foreach(var p in child
					.GetType()
					.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
					.Where(x => x.Name == "Keyword"))
				{
					var obj = p.GetValue(child, null);
					return new PrimitiveType(obj.ToString());
				}
			}
			throw new MemberAccessException("Primitive type not found for AstNode");
		}
	}
}

