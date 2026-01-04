using LanguageExt;
using Vogen;

namespace Arrivin.Domain;

[ValueObject<string>]
public partial record StorePath;

[ValueObject<string>]
public partial record DeploymentName;

[ValueObject<Uri>]
public partial record StoreUrl;

public record DeploymentInfo(
    StorePath Derivation,
    Option<StorePath> OutPath
);

[ValueObject<string>]
public partial record Installable;