
using System.Text;
using HandlebarsDotNet;

if (args.Length < 2)
{
    Console.WriteLine("Expected templates and destination directories to be specified.");
    Environment.Exit(1);
}

var templatesDirectory = new DirectoryInfo(args[0]);
var destinationDirectory = new DirectoryInfo(args[1]);

if (!templatesDirectory.Exists)
{
    Console.WriteLine("Source directory does not exist.");
    Environment.Exit(2);
}
destinationDirectory.Create();

var data = new Data(
    Features: destinationDirectory
        .EnumerateFiles("Features-\u2010-*.md", SearchOption.AllDirectories)
        .Select(featureFileInfo => new Feature(
            Name: Path
                .GetFileNameWithoutExtension(featureFileInfo.Name)
                .Replace('-', ' ')
                .Replace('\u2010', '-'),
            PageLink: Path.GetFileNameWithoutExtension(featureFileInfo.Name)
        ))
        .ToList()
);

foreach (var templateFileInfo in templatesDirectory.EnumerateFiles("*.hbs", SearchOption.AllDirectories))
{
    using var templateReader = new StreamReader(
        new FileStream(templateFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read),
        Encoding.UTF8,
        leaveOpen: false
    );
    using var markdownWriter = new StreamWriter(
        new FileStream(
            Path.Join(destinationDirectory.FullName, Path.ChangeExtension(templateFileInfo.Name, ".md")),
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read),
        Encoding.UTF8,
        leaveOpen: false
    );
    Handlebars
        .Compile(templateReader)
        .Invoke(markdownWriter, data);
}

record Data(
    IEnumerable<Feature> Features
);

record Feature(
    string Name,
    string PageLink
);