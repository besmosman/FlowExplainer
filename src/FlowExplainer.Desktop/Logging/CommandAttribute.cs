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
    
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAutocomplete : Attribute
    {
    }
}