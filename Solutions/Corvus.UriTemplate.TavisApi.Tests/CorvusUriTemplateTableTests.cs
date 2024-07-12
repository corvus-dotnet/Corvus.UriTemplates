using System;
using Corvus.UriTemplates;
using Xunit;


namespace UriTemplateTests
{
    public class CorvusUriTemplateTableTests
    {
        [Theory,
        InlineData("/", "root"),
        InlineData("/baz/fod/burg", ""),
        InlineData("/baz/kit", "kit"),
        InlineData("/baz/fod", "baz"),
        InlineData("/baz/fod/blob", "blob"),
        InlineData("/glah/flip/blob", "goo")]
        public void FindPathTemplates(string url, string key)
        {
            var builder = UriTemplateTable.CreateBuilder<string>();  // Shorter paths and literal path segments should be added to the table first.
            builder.Add("/", "root");
            builder.Add("/foo/{bar}", "foo");
            builder.Add("/baz/kit", "kit");
            builder.Add("/baz/{bar}", "baz");
            builder.Add("/baz/{bar}/blob", "blob");
            builder.Add("/{goo}/{bar}/blob", "goo");
            var table = builder.ToTable();

            if (table.TryMatch(url.AsSpan(), out TemplateMatchResult<string> match))
            {
                Assert.Equal(key, match.Result);
            }
        }

        [Theory,
     InlineData("/games", "games"),
     InlineData("/games/monopoly/Setup/23", "gamessetup"),
     InlineData("/games/monopoly/Resources/foo/23", "resource"),
     InlineData("/games/monopoly/22/Chat/33", "chat"),
     InlineData("/games/monopoly/22/State/33", "state"),
    ]
        public void FindTemplatesInGamesApi(string url, string key)
        {
            var builder = UriTemplateTable.CreateBuilder<string>();  // Shorter paths and literal path segments should be added to the table first.
            builder.Add("/games", "games");
            builder.Add("/games/{gametitle}/Setup/{gamesid}", "gamessetup");
            builder.Add("/games/{gametitle}/Resources/{resourcetype}/{resourceid}", "resource");
            builder.Add("/games/{gametitle}/{gameid}/Chat/{chatid}", "chat");
            builder.Add("/games/{gametitle}/{gameid}/State/{stateid}", "state");
            var table = builder.ToTable();

            if (table.TryMatch(url.AsSpan(), out TemplateMatchResult<string> match))
            {
                Assert.Equal(key, match.Result);
            }
        }

        [Theory,
InlineData("/foo?x=1&y=2", "fooxy3"),
InlineData("/foo?x=1", "fooxy2"),
InlineData("/foo?x=a,b,c,d", "fooxy2"),
InlineData("/foo?y=2", "fooxy"),

InlineData("/foo", "fooxy"),
]
        public void FindTemplatesWithQueryStrings(string url, string key)
        {
            var builder = UriTemplateTable.CreateBuilder<string>();  // Shorter paths and literal path segments should be added to the table first.
            builder.Add("/foo?x={x}&y={y}", "fooxy3");
            builder.Add("/foo?x={x}{&y}", "fooxy2");
            builder.Add("/foo?x={x}{&z}", "fooxy4");
            builder.Add("/foo{?x,y}", "fooxy");
            builder.Add("/foo", "foo");

            var table = builder.ToTable();

            if (table.TryMatch(url.AsSpan(), out TemplateMatchResult<string> match))
            {
                Assert.Equal(key, match.Result);
            }
        }

        [Fact]
        public void FindTemplatesWithArrayQueryParameters()
        {
            var builder = UriTemplateTable.CreateBuilder<string>();  // More restrictive templates should have priority over less restrictive ones
            builder.Add("/foo?x={x}&y={y}", "fooxy3");
            builder.Add("/foo?x={x}{&y}", "fooxy2");
            builder.Add("/foo?x={x}{&z}", "fooxy4");
            builder.Add("/foo{?x,y}", "fooxy");
            builder.Add("/foo", "foo");

            var table = builder.ToTable();

            if (table.TryMatch("/foo?x=a,b,c,d".AsSpan(), out TemplateMatchResult<string> match))
            {
                Assert.Equal("fooxy2", match.Result);
            }
        }
    }
}



