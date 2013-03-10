﻿using Roslyn.Scripting.CSharp;

namespace ScriptCs.Wrappers
{
    using Roslyn.Scripting;

    internal class ScriptEngineWrapper : IScriptEngine
    {
        private CommonScriptEngine scriptEngine;

        public ScriptEngineWrapper(CommonScriptEngine engine = null)
        {
            this.scriptEngine = engine ?? new ScriptEngine();
        }

        public string BaseDirectory
        {
            get
            {
                return this.scriptEngine.BaseDirectory;
            }

            set
            {
                this.scriptEngine.BaseDirectory = value;
            }
        }

        public void AddReference(string assemblyDisplayNameOrPath)
        {
            this.scriptEngine.AddReference(assemblyDisplayNameOrPath);
        }

        public ISession CreateSession()
        {
            return new SessionWrapper(this.scriptEngine.CreateSession());
        }

        public ISession CreateSession<THostObject>(THostObject hostObject) where THostObject : class
        {
            return new SessionWrapper(this.scriptEngine.CreateSession(hostObject));
        }

        public ISession CreateSession(object hostObject, System.Type hostObjectType = null)
        {
            return new SessionWrapper(this.scriptEngine.CreateSession(hostObject, hostObjectType));
        }
    }
}