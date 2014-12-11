﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fixie.Discovery;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Fixie.VisualStudio.TestAdapter
{
    [ExtensionUri(Id)]
    public class VsTestExecutor : ITestExecutor
    {
        public const string Id = "executor://Fixie.VisualStudio";
        public static Uri Uri = new Uri(Id);

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            HandlePoorVisualStudioImplementationDetails(runContext, frameworkHandle);

            IMessageLogger log = frameworkHandle;

            foreach (var source in sources)
            {
                log.Info("Processing " + source);

                try
                {
                    var assemblyFullPath = Path.GetFullPath(source);

                    var listener = new VisualStudioListener(frameworkHandle, source);

                    using (var environment = new ExecutionEnvironment(assemblyFullPath))
                        environment.RunAssembly(new Lookup(), listener);
                }
                catch (Exception exception)
                {
                    log.Error(exception);
                }
            }
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            HandlePoorVisualStudioImplementationDetails(runContext, frameworkHandle);

            IMessageLogger log = frameworkHandle;

            var assemblyGroups = tests.GroupBy(tc => tc.Source);

            foreach (var assemblyGroup in assemblyGroups)
            {
                var source = assemblyGroup.Key;

                log.Info("Processing " + source);

                try
                {
                    var assemblyFullPath = Path.GetFullPath(source);

                    var methodGroups = assemblyGroup.Select(x => new MethodGroup(x.FullyQualifiedName)).ToArray();
                    var listener = new VisualStudioListener(frameworkHandle, source);

                    using (var environment = new ExecutionEnvironment(assemblyFullPath))
                    {
                        environment.RunMethods(new Lookup(), listener, methodGroups);
                    }
                }
                catch (Exception exception)
                {
                    log.Error(exception);
                }
            }
        }

        public void Cancel() { }

        static void HandlePoorVisualStudioImplementationDetails(IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            RemotingUtility.CleanUpRegisteredChannels();

            if (runContext.KeepAlive)
                frameworkHandle.EnableShutdownAfterTestRun = true;
        }
    }
}