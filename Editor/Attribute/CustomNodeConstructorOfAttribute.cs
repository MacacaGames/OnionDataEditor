using System;

[AttributeUsage(AttributeTargets.Class)]
public class CustomNodeConstructorOfAttribute : Attribute
{
    public readonly Type type;
    public CustomNodeConstructorOfAttribute(Type type)
    {
        this.type = type;
    }
}