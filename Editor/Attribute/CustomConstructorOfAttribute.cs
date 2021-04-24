using System;

[AttributeUsage(AttributeTargets.Class)]
public class CustomConstructorOfAttribute : Attribute
{
    public readonly Type type;
    public CustomConstructorOfAttribute(Type type)
    {
        this.type = type;
    }
}