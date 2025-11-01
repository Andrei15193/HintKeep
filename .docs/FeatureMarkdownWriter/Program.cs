using System.Text;
using System.Text.RegularExpressions;
using Gherkin;
using Gherkin.Ast;

if (args.Length < 3)
{
    Console.WriteLine("Expected source url, source and destination directories to be specified.");
    Environment.Exit(1);
}

var sourceUrl = args[0];
var sourceDirectory = new DirectoryInfo(args[1]);
var destinationDirectory = new DirectoryInfo(args[2]);

if (!sourceDirectory.Exists)
{
    Console.WriteLine("Source directory does not exist.");
    Environment.Exit(2);
}
destinationDirectory.Create();

var parser = new Parser();

foreach (var featureFileInfo in sourceDirectory.EnumerateFiles("*.feature", SearchOption.AllDirectories))
{
    var document = parser.Parse(featureFileInfo.FullName);
    var featureMarkdownFilePath = Path.Join(destinationDirectory.FullName, $"Features - {document.Feature.Name}.md".Replace("-", "\u2010").Remove(' ', '-'));

    Console.WriteLine($"Generating '{featureMarkdownFilePath}' from '{featureFileInfo.FullName}'.");
    using var featureMarkdownTextWriter = new StreamWriter(
        new FileStream(
            featureMarkdownFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read
        ),
        Encoding.UTF8)
    {
        NewLine = "\n"
    };

    var sourceRelativePath = featureFileInfo
        .FullName[sourceDirectory.FullName.Length..]
        .Replace('\\', '/')
        .Trim('/');

    WriteFeature(featureMarkdownTextWriter, document.Feature, sourceUrl, sourceRelativePath);
}

static void WriteFeature(TextWriter featureMarkdownTextWriter, Feature feature, string sourceUrl, string sourceRelativePath)
{
    featureMarkdownTextWriter.Write($"**Source document:** [{sourceRelativePath}]({sourceUrl}/{sourceRelativePath}).");
    if (feature.Tags.Any())
    {
        featureMarkdownTextWriter.WriteLine("  ");
        featureMarkdownTextWriter.WriteLine(string.Join(' ', feature.Tags.Select(tag => $"`{tag.Name}`")));
    }
    else
        featureMarkdownTextWriter.WriteLine();


    if (!string.IsNullOrWhiteSpace(feature.Description))
    {
        var descriptionLines = feature
            .Description
            .Split('\n', StringSplitOptions.TrimEntries)
            .Append(string.Empty);

        featureMarkdownTextWriter.WriteLine();
        var currentLine = descriptionLines.First();
        foreach (var nextLine in descriptionLines.Skip(1))
        {
            featureMarkdownTextWriter.Write(Regex.Replace(currentLine, @"^(as\s+a|i\s+want\s+to|so\s+that)", "**$0**", RegexOptions.IgnoreCase));
            if (!string.IsNullOrWhiteSpace(currentLine) && !string.IsNullOrWhiteSpace(nextLine))
                featureMarkdownTextWriter.Write("  ");
            featureMarkdownTextWriter.WriteLine();

            currentLine = nextLine;
        }
    }

    WriteChildren(featureMarkdownTextWriter, feature.Children);
}

static void WriteChildren(TextWriter featureMarkdownTextWriter, IEnumerable<IHasLocation> children)
{
    foreach (var child in children)
        if (child is Background background)
            WriteBackground(featureMarkdownTextWriter, background);
        else if (child is Scenario scenario)
            WriteScenario(featureMarkdownTextWriter, scenario);
        else if (child is Rule rule)
            WriteRule(featureMarkdownTextWriter, rule);
        else
            Console.WriteLine(child.GetType());
}

static void WriteBackground(TextWriter featureMarkdownTextWriter, Background background)
{
    featureMarkdownTextWriter.WriteLine();
    WriteSummary(featureMarkdownTextWriter, background);

    WriteSteps(featureMarkdownTextWriter, background);
}

static void WriteScenario(TextWriter featureMarkdownTextWriter, Scenario scenario)
{
    featureMarkdownTextWriter.WriteLine();
    WriteSummary(featureMarkdownTextWriter, scenario);

    WriteSteps(featureMarkdownTextWriter, scenario);

    foreach (var example in scenario.Examples)
        WriteExample(featureMarkdownTextWriter, example);
}


static void WriteExample(TextWriter featureMarkdownTextWriter, Examples example)
{
    featureMarkdownTextWriter.WriteLine();
    WriteSummary(featureMarkdownTextWriter, example);

    WriteDataTable(featureMarkdownTextWriter, new DataTable([example.TableHeader, .. example.TableBody]));
}

static void WriteRule(TextWriter featureMarkdownTextWriter, Rule rule)
{
    featureMarkdownTextWriter.WriteLine();
    WriteSummary(featureMarkdownTextWriter, rule);

    WriteChildren(featureMarkdownTextWriter, rule.Children);
}

static void WriteSummary(TextWriter featureMarkdownTextWriter, IHasDescription hasDescription)
{
    featureMarkdownTextWriter.Write(hasDescription.Keyword.Trim().ToLowerInvariant() switch
    {
        "rule" => "## ",
        _ => "### "
    });
    featureMarkdownTextWriter.Write(hasDescription.Keyword.Trim());
    featureMarkdownTextWriter.Write(": ");
    featureMarkdownTextWriter.Write(hasDescription.Name);

    if (hasDescription is IHasTags hasTags)
        foreach (var tag in hasTags.Tags)
        {
            featureMarkdownTextWriter.Write(" `");
            featureMarkdownTextWriter.Write(tag.Name);
            featureMarkdownTextWriter.Write('`');
        }

    featureMarkdownTextWriter.WriteLine();

    if (!string.IsNullOrWhiteSpace(hasDescription.Description))
    {
        featureMarkdownTextWriter.WriteLine();
        foreach (var descriptionLine in hasDescription.Description.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            featureMarkdownTextWriter.WriteLine(descriptionLine.Trim());
    }
}

static void WriteSteps(TextWriter featureMarkdownTextWriter, IHasSteps hasSteps)
{
    if (hasSteps.Steps.Any())
    {
        var (given, when, then) = GetGroupedSteps(hasSteps);

        WriteGroupedSteps(featureMarkdownTextWriter, given);
        WriteGroupedSteps(featureMarkdownTextWriter, when);
        WriteGroupedSteps(featureMarkdownTextWriter, then);
    }
}

static void WriteGroupedSteps(TextWriter featureMarkdownTextWriter, IEnumerable<Step> steps)
{
    if (steps.Any())
    {
        featureMarkdownTextWriter.WriteLine();

        var stepsToProcess = new Queue<Step>(steps);
        while (stepsToProcess.Count > 0)
        {
            var step = stepsToProcess.Dequeue();
            var isLastStep = stepsToProcess.Count == 0;

            var keyword = step.Keyword.Trim();
            var isListItemStep = keyword == "*";
            var keywordMarkdown = isListItemStep ? "*" : $"**{keyword}**";

            featureMarkdownTextWriter.Write($"{keywordMarkdown} {step.Text}");
            if (step.Argument is DocString docString)
            {
                featureMarkdownTextWriter.WriteLine();
                WriteDocString(featureMarkdownTextWriter, docString, indent: isListItemStep ? 2 : 0);
            }
            else if (step.Argument is DataTable dataTable)
            {
                featureMarkdownTextWriter.WriteLine();
                WriteDataTable(featureMarkdownTextWriter, dataTable, indent: isListItemStep ? 2 : 0);
                if (!isLastStep)
                    featureMarkdownTextWriter.WriteLine();
            }
            else if (!isLastStep)
                featureMarkdownTextWriter.WriteLine("  ");
            else
                featureMarkdownTextWriter.WriteLine();
        }
    }
}

static (IEnumerable<Step> Given, IEnumerable<Step> When, IEnumerable<Step> Then) GetGroupedSteps(IHasSteps hasSteps)
{
    var index = 0;
    var groupings = hasSteps.Steps.ToLookup(
        (step) =>
        {
            if (
                step.Keyword.Contains("When", StringComparison.OrdinalIgnoreCase)
                || step.Keyword.Contains("Then", StringComparison.OrdinalIgnoreCase)
            )
                index++;

            return index;
        }
    );

    return (
        groupings[0],
        groupings[1],
        groupings[2]
    );
}

static void WriteDocString(TextWriter featureMarkdownTextWriter, DocString docString, int indent = 0)
{
    var indentString = indent <= 0 ? string.Empty : new string(' ', indent);

    featureMarkdownTextWriter.WriteLine($"{indentString}```{docString.ContentType}");
    featureMarkdownTextWriter.WriteLine($"{indentString}{docString.Content}");
    featureMarkdownTextWriter.WriteLine($"{indentString}```");
}

static void WriteDataTable(TextWriter featureMarkdownTextWriter, DataTable dataTable, int indent = 0)
{
    if (dataTable.Rows.Any())
    {
        var indentString = indent <= 0 ? string.Empty : new string(' ', indent);

        var header = dataTable.Rows.First();
        var rows = dataTable.Rows.Skip(1);

        featureMarkdownTextWriter.WriteLine($"{indentString}|{string.Join('|', header.Cells.Select(cell => cell.Value))}|");
        featureMarkdownTextWriter.WriteLine($"{indentString}|{string.Join('|', header.Cells.Select(cell => "---"))}|");
        foreach (var row in rows)
            featureMarkdownTextWriter.WriteLine($"{indentString}|{string.Join('|', row.Cells.Select(cell => cell.Value))}|");
    }
}