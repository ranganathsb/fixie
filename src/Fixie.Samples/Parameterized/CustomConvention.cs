﻿namespace Fixie.Samples.Parameterized
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Methods
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            Classes
                .Where(x => x.IsInNamespace(GetType().Namespace))
                .Where(x => x.Name.EndsWith("Tests"));

            Parameters
                .Add<InputAttributeParameterSource>();
        }

        public override void Execute(TestClass testClass, Action<CaseAction> runCases)
        {
            var instance = testClass.Construct();

            runCases(@case => @case.Execute(instance));

            instance.Dispose();
        }

        class InputAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return method.GetCustomAttributes<InputAttribute>(true).Select(input => input.Parameters);
            }
        }
    }
}