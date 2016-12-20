﻿namespace Fixie.Execution
{
    using System;

    [Serializable]
    public class SkipResult : CaseCompleted
    {
        public SkipResult(Case @case, string skipReason)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            SkipReason = skipReason;
        }

        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string SkipReason { get; private set; }

        CaseStatus CaseCompleted.Status { get { return CaseStatus.Skipped; } }
        string CaseCompleted.Output { get { return null; } }
        TimeSpan CaseCompleted.Duration { get { return TimeSpan.Zero; } }
        CompoundException CaseCompleted.Exceptions { get { return null; } }
    }
}