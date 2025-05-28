namespace FlowExplainer
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public bool TakesRawInput { get; set; }

        public CommandAttribute(bool takesRawInput = false)
        {
            TakesRawInput = takesRawInput;
        }
    }
}