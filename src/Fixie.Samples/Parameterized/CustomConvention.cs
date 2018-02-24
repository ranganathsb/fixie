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
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            ClassExecution
                .Lifecycle<CreateInstancePerClass>();

            Parameters
                .Add<InputAttributeParameterSource>();
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