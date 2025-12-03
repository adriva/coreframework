using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Adriva.Extensions.WorkflowEngine
{
    public sealed class WorkflowContext : IDisposable
    {
        private readonly ConcurrentDictionary<string, object> ContextValues = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<StepContext> StepContextStack = new Stack<StepContext>();
        private readonly Stack<StepResult> StepResultStack = new Stack<StepResult>();
        private readonly List<StepResult> ResultList = new List<StepResult>();
        private readonly Type ArgumentType;

        private bool IsArgumentInitialized = false;

        public Workflow Workflow { get; private set; }

        public object Argument { get; private set; }

        internal IList<StepResult> Results => this.ResultList.AsReadOnly();

        public StepContext StepContext
        {
            get
            {
                if (!this.StepContextStack.TryPeek(out StepContext currentStepContext))
                {
                    return null;
                }

                return currentStepContext;
            }
        }

        public object this[string variableName]
        {
            get
            {
                if (!this.TryGetValue(variableName, out object value))
                {
                    return Type.Missing;
                }

                return value;
            }
        }

        internal WorkflowContext(Workflow workflow, Type argumentType, object argumentInstance)
        {
            this.Workflow = workflow.Clone();
            this.Argument = argumentInstance;
            this.ArgumentType = argumentType;
        }

        public bool TrySetValue(string name, object value) => this.ContextValues.TryAdd(name, value);

        public bool TryGetValue<T>(string name, out T value)
        {
            value = default;

            if (!this.ContextValues.TryGetValue(name, out object objectValue))
            {
                return false;
            }

            value = (T)objectValue;
            return true;
        }

        internal StepContext EnterStep(StepCompilation stepCompilation)
        {
            if (!this.IsArgumentInitialized)
            {
                lock (this.ResultList)
                {
                    if (!this.IsArgumentInitialized)
                    {
                        this.ArgumentType.GetProperty(WorkflowEngine.ContextPropertyName)?.SetValue(this.Argument, this);
                        this.IsArgumentInitialized = true;
                    }
                }
            }

            StepContext stepContext = new StepContext(stepCompilation);

            if (0 == this.StepContextStack.Count)
            {
                this.ResultList.Add(stepContext.Result);
            }
            else
            {
                var lastResult = this.StepResultStack.Peek();
                lastResult.AddChildResult(stepContext.Result);

                var parentStepContext = this.StepContextStack.Peek();

                foreach (var parentPair in parentStepContext.Step.Properties)
                {
                    if (!stepContext.Step.Properties.ContainsKey(parentPair.Key))
                    {
                        stepContext.Step.Properties[parentPair.Key] = parentPair.Value;
                    }
                }
            }

            this.StepResultStack.Push(stepContext.Result);
            this.StepContextStack.Push(stepContext);
            return stepContext;
        }

        public void ExitStep()
        {
            this.StepContextStack.TryPop(out _);
            this.StepResultStack.TryPop(out _);
        }

        public void Dispose()
        {
            foreach (var pair in this.ContextValues)
            {
                if (pair.Value is IDisposable disposableValue)
                {
                    disposableValue.Dispose();
                }
            }

            this.ContextValues.Clear();
        }
    }
}
