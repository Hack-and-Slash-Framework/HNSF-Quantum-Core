using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SystemOrderBeforeAttribute : Attribute {

    public Type[] BeforeSystems { get; private set; }

    public SystemOrderBeforeAttribute (params Type[] beforeSystems)
    {
        this.BeforeSystems = beforeSystems;
    }
}