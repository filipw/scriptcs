namespace ScriptCs.SyntaxTreeParser.Visitors
{
    using System.Collections.Generic;

    using ICSharpCode.NRefactory.CSharp;

    internal class ClassTypeVisitor : DepthFirstAstVisitor
    {
        private readonly List<TypeDeclaration> classes;

        internal ClassTypeVisitor()
        {
            this.classes = new List<TypeDeclaration>();
        }

        internal List<TypeDeclaration> GetClassDeclarations()
        {
            return this.classes;
        }

        public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        {
            this.classes.Add(typeDeclaration);
            base.VisitTypeDeclaration(typeDeclaration);
        }
    }
}
