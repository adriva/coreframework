using System;
using System.Collections.Generic;

namespace Adriva.Extensions.WorkflowEngine
{
    internal sealed class WorkflowCompilation
    {
        public Type ArgumentType { get; private set; }

        public IList<StepCompilation> Children { get; private set; }

        public string ChangeToken { get; private set; }

        public WorkflowCompilation(Type argumentType, string changeToken)
        {
            this.ArgumentType = argumentType;
            this.ChangeToken = changeToken;
            this.Children = new List<StepCompilation>();
        }
    }
}
