﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Registration;
using Common.Logging;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;
using Xunit;
using Should;

namespace ScriptCs.Tests
{
    public class ScriptServicesBuilderTests
    {
        public class TheBuildMethod
        {
            private Mock<ILog> _mockLogger = new Mock<ILog>();
            private ScriptServices _scriptServices = new ScriptServices(null, null, null, null,null,null,null,null,null,null);
            private Mock<IRuntimeContainerFactory> _mockFactory = new Mock<IRuntimeContainerFactory>();
            private Mock<IConsole> _mockConsole = new Mock<IConsole>();
            private ScriptServicesBuilder _builder = null;

            public TheBuildMethod()
            {
                _mockFactory.Setup(f => f.GetScriptServices()).Returns(_scriptServices);
                _builder = new ScriptServicesBuilder(_mockConsole.Object, _mockLogger.Object, _mockFactory.Object);
            }

            [Fact]
            public void ShouldResolveScriptServices()
            {
                _builder.Build().ShouldEqual(_scriptServices);

            }

        }
    }
}
