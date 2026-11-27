using System.Text.RegularExpressions;
using Inventory.Domain;

namespace Inventory.Architecture.Tests;

public partial class DependencyRules
{
    [Fact]
    public void Domain_references_only_the_base_class_library()
    {
        var references = typeof(DomainException).Assembly.GetReferencedAssemblies().Select(a => a.Name!);
        var allowed = new Func<string, bool>(n => n == "netstandard" || n == "mscorlib" || n.StartsWith("System", StringComparison.Ordinal));
        Assert.All(references, name => Assert.True(allowed(name), $"Domain must not reference {name}."));
    }

    [Fact]
    public void A_slice_never_uses_another_slice()
    {
        var features = Path.Combine(RepoRoot(), "src", "Inventory.Api", "Features");
        var slices = Directory.GetDirectories(features).SelectMany(Directory.GetDirectories).ToList();
        Assert.NotEmpty(slices);

        foreach (var slice in slices)
        {
            var own = $"Inventory.Api.Features.{Path.GetFileName(Path.GetDirectoryName(slice))}.{Path.GetFileName(slice)}";
            foreach (var file in Directory.GetFiles(slice, "*.cs"))
            {
                foreach (Match m in FeatureNamespace().Matches(File.ReadAllText(file)))
                {
                    Assert.True(m.Value == own, $"{Path.GetFileName(file)} uses {m.Value}, which belongs to another slice.");
                }
            }
        }
    }

    [Fact]
    public void File_names_start_with_the_slice_name()
    {
        var features = Path.Combine(RepoRoot(), "src", "Inventory.Api", "Features");
        foreach (var slice in Directory.GetDirectories(features).SelectMany(Directory.GetDirectories))
        {
            Assert.All(Directory.GetFiles(slice, "*.cs"), f =>
                Assert.StartsWith(Path.GetFileName(slice) + ".", Path.GetFileName(f), StringComparison.Ordinal));
        }
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Inventory.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Inventory.slnx not found above " + AppContext.BaseDirectory);
    }

    [GeneratedRegex(@"Inventory\.Api\.Features\.[A-Za-z]+\.[A-Za-z]+")]
    private static partial Regex FeatureNamespace();
}
