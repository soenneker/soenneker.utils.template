using AwesomeAssertions;
using Soenneker.Tests.HostedUnit;
using Soenneker.Utils.Template.Abstract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soenneker.Utils.Template.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class TemplateUtilTests : HostedUnitTest
{
    private readonly ITemplateUtil _util;

    public TemplateUtilTests(Host host) : base(host)
    {
        _util = Resolve<ITemplateUtil>(true);
    }

    [Test]
    public void Default()
    {
    }

    [Test]
    public async ValueTask Template_should_render()
    {
        string basePath = AppContext.BaseDirectory;
        string contentPath = System.IO.Path.Combine(basePath, "content.html");
        string templatePath = System.IO.Path.Combine(basePath, "default.html");

        var tokens = new Dictionary<string, object>
        {
            {"Body", "<p>This is the HTML body content.</p>"},
            {"Message", "Hello from the template!"},
            {"Uri", "https://example.com"}
        };

        string result = await _util.RenderWithContent(templatePath, tokens, contentPath, cancellationToken: CancellationToken);

        result.Should().Contain("https://example.com");
    }
}