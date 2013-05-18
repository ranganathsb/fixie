﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie
{
    public class Runner
    {
        readonly Listener listener;

        public Runner(Listener listener)
        {
            this.listener = listener;
        }

        public Result RunAssembly(Assembly assembly)
        {
            return RunTypes(assembly, assembly.GetTypes());
        }

        public Result RunNamespace(Assembly assembly, string ns)
        {
            return RunTypes(assembly, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public Result RunType(Assembly assembly, Type type)
        {
            return RunTypes(type.Assembly, type);
        }

        public Result RunMethod(Assembly assembly, MethodInfo method)
        {
            var conventions = GetConventions(assembly);

            foreach (var convention in conventions)
                convention.Cases.Where(m => m == method);

            var type = method.DeclaringType;

            return Run(assembly, conventions, type);
        }

        private Result RunTypes(Assembly assembly, params Type[] types)
        {
            return Run(assembly, GetConventions(assembly), types);
        }

        static Convention[] GetConventions(Assembly assembly)
        {
            var customConventions = assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Convention)))
                .Select(t => (Convention)Activator.CreateInstance(t))
                .ToArray();

            if (customConventions.Any())
                return customConventions;

            return new[] { (Convention) new DefaultConvention() };
        }

        Result Run(Assembly assembly, IEnumerable<Convention> conventions, params Type[] candidateTypes)
        {
            var combinedResult = new Result(0, 0);

            foreach (var convention in conventions)
            {
                var resultListener = new ResultListener(listener);

                resultListener.RunStarted(assembly);

                convention.Execute(resultListener, candidateTypes);

                var result = resultListener.Result;

                resultListener.RunComplete(result);

                combinedResult = new Result(combinedResult.Passed + result.Passed,
                                            combinedResult.Failed + result.Failed);
            }

            return combinedResult;
        }

        class ResultListener : Listener
        {
            int passed;
            int failed;
            readonly Listener inner;

            public ResultListener(Listener inner)
            {
                this.inner = inner;
            }

            public void RunStarted(Assembly context)
            {
                passed = 0;
                failed = 0;
                inner.RunStarted(context);
            }

            public void CasePassed(string @case)
            {
                passed++;
                inner.CasePassed(@case);
            }

            public void CaseFailed(string @case, Exception[] exceptions)
            {
                failed++;
                inner.CaseFailed(@case, exceptions);
            }

            public void RunComplete(Result result)
            {
                inner.RunComplete(result);
            }

            public Result Result
            {
                get { return new Result(passed, failed); }
            }
        }
    }
}