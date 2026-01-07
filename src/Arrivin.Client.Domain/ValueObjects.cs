using Arrivin.Domain;
using Vogen;

namespace Arrivin.Client.Domain;

[ValueObject<Uri>]
public partial record ServerUrl
{
    public StoreUrl Store => StoreUrl.From(new Uri(Value, "store"));

    public ApiUrl Api => ApiUrl.From(new Uri(Value, "graphql"));
}

[ValueObject<Uri>]
public partial record ApiUrl;

[ValueObject<IReadOnlyList<string>>(fromPrimitiveCasting: CastOperator.None, toPrimitiveCasting: CastOperator.None)]
public partial record NixArgs
{
    public sealed override string ToString() => $"[{string.Join(" ", Value.Select(x => $"\"{x}\""))}]";
}

[ValueObject<string>]
public partial record FilePath
{
    public FilePath Join(FileName name) =>
        From(Path.Join(Value, name.Value));
}

[ValueObject<string>]
public partial record FileName;
