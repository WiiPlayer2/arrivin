using Vogen;

namespace Arrivin.Client.Domain;

[ValueObject<Uri>]
public partial record ServerUrl;

[ValueObject<IReadOnlyList<string>>(fromPrimitiveCasting: CastOperator.None, toPrimitiveCasting: CastOperator.None)]
public partial record NixArgs;