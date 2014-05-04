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

	        var isVoid = false;
            AstType returnType = methodDeclaration.GetChildByRole(Roles.Type).Clone();
	        var type = returnType as PrimitiveType;
	        if (type != null)
	        {
                isVoid = string.Compare(
                    type.Keyword, "void", StringComparison.OrdinalIgnoreCase) == 0;
	        }

			//create new method type 
			var methodType = new SimpleType(Identifier.Create( isVoid ? "Action" : "Func"));


	        IEnumerable<AstType> types = parameters.Select(
	            x => x.GetChildByRole(Roles.Type).Clone());
            
			//add parameter types
			methodType
				.TypeArguments
				.AddRange(types);

			//add result type
	        if (!isVoid)
	        {
	            methodType.TypeArguments.Add(returnType);
	        }
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
	}
}

