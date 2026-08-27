namespace HintKeep.GraphQL.Features.Contexts;

public abstract record PageContext(
    IReadOnlyDictionary<string, object?> FormData,
    IReadOnlyDictionary<string, string> FormErrorData
)
{
    public PageContext(Dictionary<string, object?> formData)
        : this(
            FormData: formData,
            FormErrorData: new Dictionary<string, string>()
        )
    {
    }

    public abstract string Name { get; }

    public abstract IReadOnlyDictionary<string, Func<ApplicationContext, Task>> LinkActions { get; }

    public abstract IReadOnlyDictionary<string, Func<ApplicationContext, Task>> ButtonActions { get; }
}