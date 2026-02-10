using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SystemOrderAfterAttribute : Attribute {
    public Type[] AfterSystems { get; private set; }
    
    public SystemOrderAfterAttribute (params Type[] afterSystems)
    {
        AfterSystems = afterSystems;
    }
}