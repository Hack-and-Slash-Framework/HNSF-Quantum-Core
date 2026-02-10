using System;

public interface IServiceProviderEx
{
    object GetService(Type serviceType);
    T GetService<T>() where T : class;
    bool ServiceExists(Type serviceType);
}