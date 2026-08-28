namespace HintKeep.GraphQL.Features.Contexts;

public delegate Task PageContextAction(ApplicationContext applicationContext, CancellationToken cancellationToken = default);

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

    public virtual IReadOnlyDictionary<string, string> FormFieldMappings { get; } = _emptyDictionary;
    private static readonly IReadOnlyDictionary<string, string> _emptyDictionary = new Dictionary<string, string>();

    public abstract IReadOnlyDictionary<string, PageContextAction> LinkActions { get; }

    public abstract IReadOnlyDictionary<string, PageContextAction> ButtonActions { get; }
}