using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Adriva.Common.Core;
using FastExpressionCompiler;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.WorkflowEngine
{
    public sealed class WorkflowEngine : IWorkflowEngine
    {
        internal const string ContextPropertyName = "Context";

        private readonly IServiceProvider ServiceProvider;
        private readonly ILogger Logger;
        private readonly MemoryCache MemoryCache;
        private readonly ParsingConfig ParsingConfig = ParsingConfig.Default;
        private readonly WorkflowEngineOptions Options;

        public WorkflowEngine(IServiceProvider serviceProvider, string name, IOptionsMonitor<WorkflowEngineOptions> optionsAccessor)
        {
            this.ServiceProvider = serviceProvider;
            this.Options = optionsAccessor.Get(name);
            this.Logger = serviceProvider.GetRequiredService<ILogger<WorkflowEngine>>();
            this.MemoryCache = new MemoryCache(new MemoryCacheOptions());

            if (null != this.Options.CustomTypes)
            {
                this.ParsingConfig.CustomTypeProvider = new WorkflowTypeProvider(this.Options.CustomTypes);
            }
        }

        private void ValidateWorkflow(Workflow workflow, InputParameter[] inputParameters)
        {
            var workflowValidator = new Validators.WorkflowValidator();
            var workflowValidationResult = workflowValidator.Validate(workflow);

            if (!workflowValidationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(workflowValidationResult.Errors);
            }

            var inputParameterValidator = new Validators.InputParametersValidator();
            var inputParameterValidatorResult = inputParameterValidator.Validate(inputParameters);

            if (!inputParameterValidatorResult.IsValid)
            {
                throw new FluentValidation.ValidationException(inputParameterValidatorResult.Errors);
            }

            Queue<WorkflowStep> stepQueue = new Queue<WorkflowStep>();
            workflow.Steps.ForEach((index, step) => stepQueue.Enqueue(step));

            while (0 < stepQueue.Count)
            {
                var currentStep = stepQueue.Dequeue();
                currentStep.Properties ??= new Dictionary<string, object>();

                if (null != currentStep.Steps)
                {
                    foreach (var childStep in currentStep.Steps)
                    {
                        stepQueue.Enqueue(childStep);
                    }
                }
            }
        }

        private async Task<WorkflowCompilation> CompileWorkflowAsync(Workflow workflow, InputParameter[] inputParameters)
        {
            string cacheKey = $"workflow_{workflow.Name}";
            string hash = workflow.CalculateHash();

            var workflowCompilation = await this.MemoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                await Task.CompletedTask;

                List<DynamicProperty> dynamicProperties = new List<DynamicProperty>();

                dynamicProperties.Add(new DynamicProperty(WorkflowEngine.ContextPropertyName, typeof(WorkflowContext)));

                foreach (var inputParameter in inputParameters)
                {
                    dynamicProperties.Add(new DynamicProperty(inputParameter.Name, inputParameter.Value?.GetType() ?? typeof(object)));
                }

                Type argumentType = DynamicClassFactory.CreateType(dynamicProperties, false);
                var argument = Expression.Parameter(argumentType, "argument");

                WorkflowCompilation workflowCompilation = new WorkflowCompilation(argumentType, hash);

                foreach (var step in workflow.Steps)
                {
                    workflowCompilation.Children.Add(this.CompileWorkflowStep(step, argument));
                }

                return workflowCompilation;
            });

            if (0 == string.Compare(workflowCompilation.ChangeToken, hash, StringComparison.Ordinal))
            {
                return workflowCompilation;
            }

            this.MemoryCache.Remove(cacheKey);
            return await this.CompileWorkflowAsync(workflow, inputParameters);
        }

        private StepCompilation CompileWorkflowStep(WorkflowStep step, ParameterExpression parameterExpression)
        {
            Func<object, bool> stepDelegate = null;
            Type stepActionType = null;

            if (!string.IsNullOrWhiteSpace(step.Predicate))
            {
                LambdaExpression stepLambda = DynamicExpressionParser.ParseLambda(this.ParsingConfig, false, new[] { parameterExpression }, typeof(bool), step.Predicate);

                var outerParameter = Expression.Parameter(typeof(object), "input");
                var outerToInnerConversion = Expression.Convert(outerParameter, parameterExpression.Type);

                stepLambda = Expression.Lambda<Func<object, bool>>(Expression.Invoke(stepLambda, outerToInnerConversion), outerParameter);
                stepDelegate = stepLambda.CompileFast<Func<object, bool>>(false, CompilerFlags.NoInvocationLambdaInlining);
            }

            if (null != step.Action)
            {
                stepActionType = ReflectionHelpers.FindTypes(type =>
                                                        type.IsClass &&
                                                        !type.IsAbstract &&
                                                        !type.IsSpecialName &&
                                                        typeof(IStepAction).IsAssignableFrom(type) &&
                                                        step.Action.Target.Equals(type.FullName, StringComparison.Ordinal))
                                                        .FirstOrDefault();
            }

            StepCompilation stepCompilation = new StepCompilation(step, stepDelegate, stepActionType);

            if (null != step.Steps && step.Steps.Any())
            {
                foreach (var childStep in step.Steps)
                {
                    var childStepCompilation = this.CompileWorkflowStep(childStep, parameterExpression);
                    stepCompilation.Children.Add(childStepCompilation);
                }
            }

            return stepCompilation;
        }

        public Task<WorkflowResults> RunAsync(string json, params InputParameter[] inputParameters)
        {
            this.Logger.LogInformation("Loading workflow from json.");
            var workflow = Utilities.SafeDeserialize<Workflow>(json);
            this.Logger.LogInformation($"Loaded workflow '{workflow.Name}' from json.");

            return this.RunAsync(workflow, inputParameters);
        }

        public async Task<WorkflowResults> RunAsync(Workflow workflow, params InputParameter[] inputParameters)
        {
            inputParameters = inputParameters ?? Array.Empty<InputParameter>();

            this.Logger.LogInformation($"Validating workflow '{workflow.Name}'.");

            try
            {
                this.ValidateWorkflow(workflow, inputParameters);
            }
            catch (FluentValidation.ValidationException validationException)
            {
                this.Logger.LogError(validationException, $"Workflow '{workflow.Name ?? "<empty>"}' failed validation.");
                throw;
            }

            this.Logger.LogDebug($"Validated workflow '{workflow.Name}'.");

            this.Logger.LogInformation($"Compiling workflow '{workflow.Name}'.");

            var workflowCompilation = await this.CompileWorkflowAsync(workflow, inputParameters);

            this.Logger.LogDebug($"Compiled workflow '{workflow.Name}'.");

            this.Logger.LogInformation($"Creating dynamic argument for predicates of workflow '{workflow.Name}'.");

            var argumentInstance = ActivatorUtilities.CreateInstance(this.ServiceProvider, workflowCompilation.ArgumentType);

            foreach (var inputParameter in inputParameters)
            {
                workflowCompilation.ArgumentType.GetProperty(inputParameter.Name).SetValue(argumentInstance, inputParameter.Value);
            }

            this.Logger.LogDebug($"Created dynamic argument for predicates of workflow '{workflow.Name}'.");

            using (var scope = this.ServiceProvider.CreateScope())
            {
                using (var workflowContext = new WorkflowContext(workflow, workflowCompilation.ArgumentType, argumentInstance))
                {
                    foreach (var stepCompilation in workflowCompilation.Children)
                    {
                        StepContext stepContext = workflowContext.EnterStep(stepCompilation);

                        try
                        {
                            await this.RunStepAsync(scope, workflowContext, stepContext, inputParameters);
                        }
                        catch (Exception fatalError)
                        {
                            this.Logger.LogError(fatalError, $"Workflow '{workflow.Name}' completed with errors.");

                            return new WorkflowResults(workflowContext.Results)
                            {
                                HasError = true
                            };
                        }
                        finally
                        {
                            workflowContext.ExitStep();
                        }
                    }
                    this.Logger.LogInformation($"Workflow '{workflow.Name}' completed successfully.");
                    return new WorkflowResults(workflowContext.Results);
                }
            }
        }

        private async ValueTask<bool> RunStepAsync(IServiceScope scope, WorkflowContext workflowContext, StepContext stepContext, InputParameter[] inputParameters)
        {
            var stepCompilation = stepContext.StepCompilation;

            if (!stepCompilation.Step.IsEnabled)
            {
                this.Logger.LogInformation($"Workflow step '{stepContext.Step.Name}' is disabled and will be skipped.");
                return false;
            }

            if (null == stepCompilation.Predicate || stepCompilation.Predicate(workflowContext.Argument))
            {
                if (null == stepCompilation.Predicate)
                {
                    this.Logger.LogInformation($"[{workflowContext.Workflow.Name} -> {stepContext.Step.Name}] predicate is not set and is ignored.");
                }
                else
                {
                    this.Logger.LogInformation($"[{workflowContext.Workflow.Name} -> {stepContext.Step.Name}] ({stepContext.Step.Predicate}) => true");
                }

                stepContext.Result.PredicateResult = true;

                if (null != stepCompilation.ActionType)
                {
                    this.Logger.LogDebug($"[{workflowContext.Workflow.Name} -> {stepContext.Step.Name}] has target '{stepContext.Step.Action.Target}'.");

                    DynamicItem dynamicStepActionProperties = new DynamicItem(stepCompilation.Step.Properties);
                    var stepAction = (IStepAction)ActivatorUtilities.CreateInstance(scope.ServiceProvider, stepCompilation.ActionType);

                    try
                    {
                        this.Logger.LogInformation($"[{workflowContext.Workflow.Name} -> {stepContext.Step.Name}] running target '{stepContext.Step.Action.Target}'.");
                        stepContext.Result.ActionResult = await stepAction.RunAsync(workflowContext, inputParameters, dynamicStepActionProperties);
                        this.Logger.LogInformation($"[{workflowContext.Workflow.Name} -> {stepContext.Step.Name}] ran target '{stepContext.Step.Action.Target}'.");
                    }
                    catch (Exception stepFatalError)
                    {
                        this.Logger.LogError(stepFatalError, $"[{workflowContext.Workflow.Name} -> {stepContext.Step.Name}] error running target '{stepContext.Step.Action.Target}'.");
                        stepContext.Result.Exception = stepFatalError;
                        throw;
                    }

                    if (!string.IsNullOrWhiteSpace(stepContext.StepCompilation.Step.Action.Name))
                    {
                        this.Logger.LogInformation($"[{workflowContext.Workflow.Name} -> {stepContext.Step.Name}] has action name set to '{stepContext.Step.Action.Name}' and the output value will be stored in the workflow context.");
                        workflowContext.TrySetValue(stepContext.StepCompilation.Step.Action.Name, stepContext.Result.ActionResult);
                    }
                }

                if (0 < stepCompilation.Children.Count)
                {
                    for (var loop = 0; loop < stepCompilation.Children.Count; loop++)
                    {
                        StepContext childStepContext = workflowContext.EnterStep(stepCompilation.Children[loop]);

                        try
                        {
                            bool childPredicateResult = await this.RunStepAsync(scope, workflowContext, childStepContext, inputParameters);

                            switch (stepCompilation.Step.Operator)
                            {
                                case ActionOperator.None:
                                    break;
                                case ActionOperator.And:
                                    if (!childPredicateResult)
                                    {
                                        this.Logger.LogDebug($"[{workflowContext.Workflow.Name} -> {childStepContext.Step.Name}] predicate returned false and operator is set to AND. Will stop processing further child steps.");
                                        return false;
                                    }
                                    break;
                                case ActionOperator.Or:
                                    if (childPredicateResult)
                                    {
                                        this.Logger.LogDebug($"[{workflowContext.Workflow.Name} -> {childStepContext.Step.Name}] predicate returned true and operator is set to OR. Will stop processing further child steps.");
                                        return true;
                                    }
                                    break;
                            }
                        }
                        finally
                        {
                            workflowContext.ExitStep();
                        }
                    }
                }

                return true;
            }
            else
            {
                this.Logger.LogInformation($"[{workflowContext.Workflow.Name} -> {stepContext.Step.Name}] ({stepContext.Step.Predicate}) => false");
                stepContext.Result.PredicateResult = false;

                return false;
            }
        }
    }
}
