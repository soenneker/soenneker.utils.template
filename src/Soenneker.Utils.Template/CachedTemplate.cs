using System;

namespace Soenneker.Utils.Template;

internal readonly record struct CachedTemplate(DateTimeOffset LastModified, Scriban.Template Template);
