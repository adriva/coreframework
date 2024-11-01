namespace Adriva.Extensions.WorkflowEngine
{
    public sealed class InputParameter
    {
        public string Name { get; private set; }

        public object Value { get; private set; }

        public InputParameter(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public static implicit operator InputParameter((string Name, object Value) parameter)
        {
            return new InputParameter(parameter.Name, parameter.Value);
        }
    }
}
