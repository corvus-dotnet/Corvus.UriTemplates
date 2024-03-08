using Corvus.UriTemplates;
using Xunit;


namespace UriTemplateTests
{
    public class CorvusUriTemplateAndVerbTableTests
    {
        [Theory,
        InlineData("/", "GET", "root_get"),
        InlineData("/", "PUT", "root_put"),
        InlineData("/baz/fod/burg", "GET", "get"),
        InlineData("/baz/fod/burg", "PUT", "put"),
        InlineData("/baz/kit", "GET", "kit_get"),
        InlineData("/baz/kit", "PUT", "kit_put"),
        InlineData("/baz/fod", "GET", "baz_get"),
        InlineData("/baz/fod", "PUT", "baz_put"),
        InlineData("/baz/fod/blob", "GET", "blob_get"),
        InlineData("/baz/fod/blob", "PUT", "blob_put"),
        InlineData("/glah/flip/blob", "GET", "goo_get"),
        InlineData("/glah/flip/blob", "PUT", "goo_put"),
        InlineData("foo/glah/flip/blob", "PUT", "goo_put"),]
        public void FindPathTemplates(string url, string verb, string key)
        {
            var builder = UriTemplateAndVerbTable.CreateBuilder<string>();
            builder.Add("/", "GET", "root_get");
            builder.Add("/", "PUT", "root_put");
            builder.Add("/foo/{bar}", "GET", "foo_get");
            builder.Add("/foo/{bar}", "PUT", "foo_put");
            builder.Add("/baz/kit", "GET", "kit_get");
            builder.Add("/baz/kit", "PUT", "kit_put");
            builder.Add("/baz/{bar}", "GET", "baz_get");
            builder.Add("/baz/{bar}", "PUT", "baz_put");
            builder.Add("/baz/{bar}/blob", "GET", "blob_get");
            builder.Add("/baz/{bar}/blob", "PUT", "blob_put");
            builder.Add("/{goo}/{bar}/blob", "GET", "goo_get");
            builder.Add("/{goo}/{bar}/blob", "PUT", "goo_put");
            builder.Add("foo/{goo}/{bar}/blob", "PUT", "foo_prefix_goo_put");
            var table = builder.ToTable();

            if (table.TryMatch(url, verb, out TemplateMatchResult<string> match))
            {
                Assert.Equal(key, match.Result);
            }
        }

        [Theory,
        InlineData("/", "GET", "root_get"),
        InlineData("/", "PUT", "root_put"),
        InlineData("/baz/fod/burg", "GET", "get"),
        InlineData("/baz/fod/burg", "PUT", "put"),
        InlineData("/baz/kit", "GET", "kit_get"),
        InlineData("/baz/kit", "PUT", "kit_put"),
        InlineData("/baz/fod", "GET", "baz_get"),
        InlineData("/baz/fod", "PUT", "baz_put"),
        InlineData("/baz/fod/blob", "GET", "blob_get"),
        InlineData("/baz/fod/blob", "PUT", "blob_put"),
        InlineData("/glah/flip/blob", "GET", "goo_get"),
        InlineData("/glah/flip/blob", "PUT", "goo_put"),
        InlineData("foo/glah/flip/blob", "PUT", "foo_prefix_goo_put"),
        ]
        public void FindPathTemplatesWithRootedMatch(string url, string verb, string key)
        {
            var builder = UriTemplateAndVerbTable.CreateBuilder<string>();
            builder.Add("/", "GET", "root_get");
            builder.Add("/", "PUT", "root_put");
            builder.Add("/foo/{bar}", "GET", "foo_get");
            builder.Add("/foo/{bar}", "PUT", "foo_put");
            builder.Add("/baz/kit", "GET", "kit_get");
            builder.Add("/baz/kit", "PUT", "kit_put");
            builder.Add("/baz/{bar}", "GET", "baz_get");
            builder.Add("/baz/{bar}", "PUT", "baz_put");
            builder.Add("/baz/{bar}/blob", "GET", "blob_get");
            builder.Add("/baz/{bar}/blob", "PUT", "blob_put");
            builder.Add("/{goo}/{bar}/blob", "GET", "goo_get");
            builder.Add("/{goo}/{bar}/blob", "PUT", "goo_put");
            builder.Add("foo/{goo}/{bar}/blob", "PUT", "foo_prefix_goo_put");
            var table = builder.ToTable();

            if (table.TryMatch(url, verb, out TemplateMatchResult<string> match, true))
            {
                Assert.Equal(key, match.Result);
            }
        }
    }
}


