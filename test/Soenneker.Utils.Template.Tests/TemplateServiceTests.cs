﻿using AwesomeAssertions;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.Template.Abstract;
using System;
using System.Collections.Generic;
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