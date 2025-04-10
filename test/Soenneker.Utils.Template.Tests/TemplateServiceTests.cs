using FluentAssertions;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.Template.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

    [Fact]
    public async ValueTask Template_should_render()
    {
        string basePath = AppContext.BaseDirectory;
        string contentPath = Path.Combine(basePath, "content.html");
        string templatePath = Path.Combine(basePath, "default.html");

        var replacements = new Dictionary<string, object>
        {
            {"Body", "<p>This is the HTML body content.</p>"},
            {"Message", "Hello from the template!"},
            {"Uri", "https://example.com"}
        };

        string result = await _util.Render(templatePath, replacements, contentPath, null);

        result.Should().Contain("https://example.com");
    }
}