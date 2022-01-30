using System;
using System.Collections;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class ObjectDataSource : IDataSource
    {
        protected Type ObjectType { get; private set; }

        protected IServiceProvider ServiceProvider { get; private set; }

        public ObjectDataSource(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public Task OpenAsync(DataSourceDefinition dataSourceDefinition)
        {
            if (string.IsNullOrWhiteSpace(dataSourceDefinition?.ConnectionString))
            {
                throw new ArgumentException(nameof(dataSourceDefinition.ConnectionString), "Connection string must point to a valid type.");
            }

            this.ObjectType = Type.GetType(dataSourceDefinition.ConnectionString, false, true);

            if (null == this.ObjectType)
            {
                throw new InvalidOperationException($"Specified data source '{dataSourceDefinition.ConnectionString}' could not be found or loaded.");
            }

            return Task.CompletedTask;
        }

        public virtual async Task<DataSet> GetDataAsync(ReportCommand command, FieldDefinition[] fields)
        {
            string commandMethodName = command.GetNameWithoutParameters();

            var methodCandidates = ReflectionHelpers.FindMethods(this.ObjectType, m =>
                                    {
                                        return
                                            commandMethodName.Equals(m.Name, StringComparison.OrdinalIgnoreCase)
                                            && m.GetParameters().Length == command.Parameters.Count;
                                    }).ToList();

            MethodInfo targetMethod = null;

            if (0 == methodCandidates.Count)
            {
                throw new InvalidOperationException($"Method '{command.GetNameWithoutParameters()}' with {command.Parameters.Count} arguments couldn't be found on type '{this.ObjectType.FullName}'.");
            }
            else if (1 == methodCandidates.Count)
            {
                targetMethod = methodCandidates[0];
            }
            else
            {
                var parameterNames = command.Parameters.Select(x => x.Name.TrimStart('@'));

                targetMethod = methodCandidates.FirstOrDefault(m =>
                                {
                                    return m.GetParameters()
                                            .Select(p => p.Name)
                                            .SequenceEqual(parameterNames);
                                });

                if (null == targetMethod)
                {
                    throw new InvalidOperationException($"Method '{command.GetNameWithoutParameters()}' with {command.Parameters.Count} arguments couldn't be found on type '{this.ObjectType.FullName}'.");
                }
            }

            object methodOutput = null;

            object objectInstance = targetMethod.IsStatic ? null : ActivatorUtilities.CreateInstance(this.ServiceProvider, this.ObjectType);

            try
            {
                methodOutput = targetMethod.Invoke(objectInstance, command.Parameters.Select(x => x.FilterValue.Value).ToArray());
            }
            finally
            {
                if (objectInstance is IAsyncDisposable asyncDisposableObject)
                {
                    await asyncDisposableObject.DisposeAsync();
                }
                else if (objectInstance is IDisposable disposableObject)
                {
                    disposableObject.Dispose();
                }
            }

            IEnumerable dataItems = null;
            DataSet dataSet = DataSet.FromFields(fields);

            if (methodOutput is Task methodTask)
            {
                await methodTask;

                if (typeof(Task<>).IsAssignableFrom(targetMethod.ReturnType.GetGenericTypeDefinition()))
                {
                    dataItems = ((dynamic)methodTask).Result as IEnumerable;
                }
            }
            else if (methodOutput is IEnumerable enumerableOutput)
            {
                dataItems = enumerableOutput;
            }

            if (null == dataItems)
            {
                throw new InvalidOperationException($"Data source method '{ReflectionHelpers.GetNormalizedName(targetMethod)}' returned an unrecognized type ({ReflectionHelpers.GetNormalizedName(methodOutput.GetType())}). A data source method should return IEnumerable, IEnumerable<T>, Task<IEnumerable> or Task<IEnumerable<T>>.");
            }

            foreach (var dataItem in dataItems)
            {
                if (null != dataItem)
                {
                    var jdataItem = JObject.FromObject(dataItem);

                    var dataRow = dataSet.CreateRow();

                    foreach (var field in fields)
                    {
                        JProperty property = jdataItem.Property(field.Name, StringComparison.OrdinalIgnoreCase);

                        if (null != property?.Value && property.Value is JValue jvalue)
                        {
                            dataRow.AddData(jvalue.Value);
                        }
                        else
                        {
                            dataRow.AddData(null);
                        }
                    }
                }
            }

            return dataSet;
        }

        public Task CloseAsync()
        {
            this.ObjectType = null;
            return Task.CompletedTask;
        }
    }

    public class ObjectCommandOptions
    {

    }
}