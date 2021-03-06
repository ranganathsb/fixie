﻿namespace Fixie
{
    using System;
    using System.Reflection;

    /// <summary>
    /// The context in which a test class is running.
    /// </summary>
    public class TestClass
    {
        readonly bool isStatic;
        internal TestClass(Type type) : this(type, null) { }

        internal TestClass(Type type, MethodInfo targetMethod)
        {
            Type = type;
            TargetMethod = targetMethod;
            isStatic = Type.IsStatic();
        }

        /// <summary>
        /// The test class to execute.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the target MethodInfo identified by the
        /// test runner as the sole method to be executed.
        /// Null under normal test execution.
        /// </summary>
        public MethodInfo TargetMethod { get; }

        /// <summary>
        /// Constructs an instance of the test class type, using its default constructor.
        /// If the class is static, no action is taken and null is returned.
        /// </summary>
        public object Construct()
        {
            if (isStatic)
                return null;

            try
            {
                return Activator.CreateInstance(Type);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception.InnerException);
            }
        }
    }
}