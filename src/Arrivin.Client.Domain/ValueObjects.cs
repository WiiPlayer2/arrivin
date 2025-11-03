using Vogen;

namespace Arrivin.Client.Domain;

[ValueObject<Uri>]
public partial record ServerUrl;

[ValueObject<IReadOnlyList<string>>]
public partial record NixArgs;