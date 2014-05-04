namespace ScriptCs.SyntaxTreeParser.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ICSharpCode.NRefactory.CSharp;

    public class MethodVisitor : DepthFirstAstVisitor
	{
        private readonly List<Tuple<MethodDeclaration, FieldDeclaration>> methods;

        internal MethodVisitor()
        {
            this.methods = new List<Tuple<MethodDeclaration, FieldDeclaration>>();
        }

        internal List<Tuple<MethodDeclaration, FieldDeclaration>> GetMethodDeclarations()
        {
            return this.methods;
        }

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
				.Select(x => this.GetKeywordAsPrimitiveType(x)));

			//add result type
			methodType
				.TypeArguments
				.Add(this.GetKeywordAsPrimitiveType(methodDeclaration));

			//get method body
			var methodBody = (BlockStatement)methodDeclaration
				.GetChildrenByRole(Roles.Body)
				.FirstOrDefault()
				.Clone();

			//get methodName
			var methodName = this.GetIdentifierName(methodDeclaration);

			//create named method expression
			var methodExpression = new VariableInitializer(methodName, 
				new AnonymousMethodExpression(methodBody, parameters));

			//compose type and expression
			var namedMethodExpr = new FieldDeclaration { ReturnType = methodType };
	        namedMethodExpr.Variables.Add(methodExpression);

	        this.methods.Add(new Tuple<MethodDeclaration, FieldDeclaration>(methodDeclaration, namedMethodExpr));
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

