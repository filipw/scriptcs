﻿using ScriptCs.Contracts;
using ScriptCs.Contracts.Logging;

namespace ScriptCs
{
    public interface ILoggerConfigurator
    {
        void Configure(IConsole console);

        void Configure(IConsole console, ILog log);
        
        ILog GetLogger();
    }
}