﻿using Common.Logging;
using Moq;
using ScriptCs.Argument;
using ScriptCs.Command;
using ScriptCs.Package;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class CommandFactoryTests
    {
        public class CreateCommandMethod
        {
            private static ScriptServiceRoot CreateRoot(ScriptCsArgs args, bool packagesFileExists = true, bool packagesFolderExists = true)
            {
                const string CurrentDirectory = "C:\\";
                const string PackagesFile = "C:\\packages.config";
                const string PackagesFolder = "C:\\packages";

                var fs = new Mock<IFileSystem>();
                fs.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fs.Setup(x => x.FileExists(PackagesFile)).Returns(packagesFileExists);
                fs.Setup(x => x.DirectoryExists(PackagesFolder)).Returns(packagesFolderExists);

                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var assemblyName = new Mock<IAssemblyResolver>();
                var argumentHandler = new Mock<IArgumentHandler>();

                var argsParseResult = new ArgumentParseResult(new string[0], args, new string[0]);
                argumentHandler.Setup(i => i.GetParsedArguments()).Returns(argsParseResult);

                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object, argumentHandler.Object);
                return root;
            }

            [Fact]
            public void ShouldInstallAndRestoreWhenInstallFlagIsOn()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = string.Empty,
                    ScriptName = null
                };

                var factory = new CommandFactory(CreateRoot(args));
                var result = factory.CreateCommand();

                result.ShouldImplement<IInstallCommand>();
            }

            [Fact]
            public void ShouldInstallAndSaveWhenInstallFlagIsOnAndNoPackagesFileExists()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = string.Empty,
                    ScriptName = null
                };

                var factory = new CommandFactory(CreateRoot(args, packagesFileExists: false));
                var result = factory.CreateCommand();

                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<IInstallCommand>();
                compositeCommand.Commands[1].ShouldImplement<ISaveCommand>();
            }

            [Fact]
            public void ShouldExecuteWhenScriptNameIsPassed()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = "test.csx"
                };

                var factory = new CommandFactory(CreateRoot(args));
                var result = factory.CreateCommand();

                result.ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldInstallAndExecuteWhenScriptNameIsPassedAndPackagesFolderDoesNotExist()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = "test.csx"
                };

                var root = CreateRoot(args, packagesFileExists: true, packagesFolderExists: false);
                var factory = new CommandFactory(root);
                var result = factory.CreateCommand();

                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<IInstallCommand>();
                compositeCommand.Commands[1].ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldExecuteWhenBothNameAndInstallArePassed()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = string.Empty,
                    ScriptName = "test.csx"
                };

                var factory = new CommandFactory(CreateRoot(args));
                var result = factory.CreateCommand();

                result.ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldSaveAndCleanWhenCleanFlagIsPassed()
            {
                var args = new ScriptCsArgs { Clean = true, ScriptName = null };

                var factory = new CommandFactory(CreateRoot(args));
                var result = factory.CreateCommand();

                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<ISaveCommand>();
                compositeCommand.Commands[1].ShouldImplement<ICleanCommand>();
            }

            [Fact]
            public void ShouldSaveWhenSaveFlagIsPassed()
            {
                var args = new ScriptCsArgs { Save = true, ScriptName = null };

                var factory = new CommandFactory(CreateRoot(args));
                var result = factory.CreateCommand();

                result.ShouldNotBeNull();
                result.ShouldImplement<ISaveCommand>();
            }

            [Fact]
            public void ShouldReturnInvalidWhenNoNameOrInstallSet()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = null
                };

                var factory = new CommandFactory(CreateRoot(args));
                var result = factory.CreateCommand();

                result.ShouldImplement<IInvalidCommand>();
            }

            [Fact]
            public void ShouldReturnHelpCommandWhenHelpIsPassed()
            {
                var args = new ScriptCsArgs
                    {
                        Help = true
                    };

                var factory = new CommandFactory(CreateRoot(args));
                var result = factory.CreateCommand();

                result.ShouldImplement<IHelpCommand>();
            }

            [Fact]
            public void ShouldPassScriptArgsToExecuteCommandConstructor()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = "test.csx"
                };

                var scriptArgs = new string[0];
                var factory = new CommandFactory(CreateRoot(args));
                var result = factory.CreateCommand() as IScriptCommand;

                result.ScriptArgs.ShouldEqual(scriptArgs);
            }
        }
    }
}
