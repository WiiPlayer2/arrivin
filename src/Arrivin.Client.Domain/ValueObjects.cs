using Vogen;

namespace Arrivin.Client.Domain;

[ValueObject<Uri>]
public partial record ServerUrl;

[ValueObject<IReadOnlyList<string>>(fromPrimitiveCasting: CastOperator.None, toPrimitiveCasting: CastOperator.None)]
public partial record NixArgs;

[ValueObject<string>]
public partial record FilePath
{
    public FilePath Join(FileName name) =>
        From(Path.Join(Value, name.Value));
}

[ValueObject<string>]
public partial record FileName;
