using Soenneker.Utils.Template.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Utils.Template.Tests;

[Collection("Collection")]
public class TemplateUtilTests : FixturedUnitTest
{
    private readonly ITemplateUtil _util;

    public TemplateUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<ITemplateUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
