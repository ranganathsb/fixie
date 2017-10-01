﻿namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Cli;
    using Listeners;

    class ExecutionProxy
    {
        readonly List<Listener> customListeners = new List<Listener>();

        public void Subscribe<TListener>(TListener listener) where TListener : Listener
        {
            customListeners.Add(listener);
        }

        public void DiscoverMethods(Assembly assembly, string[] arguments)
        {
            var options = CommandLine.Parse<Options>(arguments, out string[] conventionArguments);

            var listeners = customListeners;
            var bus = new Bus(listeners);
            var discoverer = new Discoverer(bus, Filter(options), conventionArguments);

            discoverer.DiscoverMethods(assembly);
        }

        public int RunAssembly(Assembly assembly, string[] arguments)
        {
            var summary = Run(arguments, runner => runner.RunAssembly(assembly));

            return summary.Failed;
        }

        public int RunMethods(Assembly assembly, string[] arguments, string[] methods)
        {
            var methodGroups = methods.Select(x => new MethodGroup(x)).ToArray();

            var summary = Run(arguments, r => r.RunMethods(assembly, methodGroups));

            return summary.Failed;
        }

        ExecutionSummary Run(string[] arguments, Func<Runner, ExecutionSummary> run)
        {
            var options = CommandLine.Parse<Options>(arguments, out string[] conventionArguments);

            var listeners = GetExecutionListeners(options);
            var bus = new Bus(listeners);
            var runner = new Runner(bus, Filter(options), conventionArguments);

            return run(runner);
        }

        static Filter Filter(Options options)
        {
            var filter = new Filter();
            filter.ByPatterns(options.Patterns);
            return filter;
        }

        List<Listener> GetExecutionListeners(Options options)
        {
            return customListeners.Any() ? customListeners : DefaultExecutionListeners(options).ToList();
        }

        static IEnumerable<Listener> DefaultExecutionListeners(Options options)
        {
            if (ShouldUseTeamCityListener(options))
                yield return new TeamCityListener();
            else
                yield return new ConsoleListener();

            if (ShouldUseAppVeyorListener())
                yield return new AppVeyorListener();

            if (options.Report != null)
                yield return new ReportListener(SaveReport(options));
        }

        static Action<XDocument> SaveReport(Options options)
        {
            return report => ReportListener.Save(report, FullPath(options.Report));
        }

        static string FullPath(string absoluteOrRelativePath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), absoluteOrRelativePath);
        }

        static bool ShouldUseTeamCityListener(Options options)
        {
            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            return options.TeamCity ?? runningUnderTeamCity;
        }

        static bool ShouldUseAppVeyorListener()
        {
            return Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        }
    }
}
