using HotChocolate.Language;
using LanguageExt;

namespace Arrivin.Server.Web;

public class UnitType : ScalarType<Unit, StringValueNode>
{
    public UnitType() : base("Unit") { }

    public override IValueNode ParseResult(object? resultValue) => throw new NotImplementedException();

    protected override Unit ParseLiteral(StringValueNode valueSyntax) => Unit.Default;

    protected override StringValueNode ParseValue(Unit runtimeValue) => new("()");
}
