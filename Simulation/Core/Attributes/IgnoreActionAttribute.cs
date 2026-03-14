using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class IgnoreActionAttribute : Attribute {
    public IgnoreActionAttribute (params Type[] afterSystems)
    {
    }
}