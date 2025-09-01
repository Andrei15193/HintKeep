namespace HintKeep.GraphQL.Definitions;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class MutationFieldAttribute : Attribute
{
    public bool AllowAnonymous { get; init; }
}