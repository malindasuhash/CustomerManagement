using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Infrastructure
{
    // Approach from Microsoft Copilot = https://copilot.microsoft.com/chats/t3sFY5ipM6gxqUkUp3d7v
    internal class IgnorePropertiesResolver : DefaultContractResolver
    {
        private readonly HashSet<string> _ignoreProps;

        public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
        {
            _ignoreProps = new HashSet<string>(propNamesToIgnore, StringComparer.OrdinalIgnoreCase);
            NamingStrategy = new CamelCaseNamingStrategy();
        }

        protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);

            return props
                .Where(p => !_ignoreProps.Contains(p.PropertyName))
                .ToList();
        }
    }

}
