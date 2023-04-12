using _MicroserviceTemplate_.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _MicroserviceTemplate_.EF.DataConfigurations
{
    class SettingsEntryDataConfiguration : IEntityTypeConfiguration<SettingsEntry>
    {
        public void Configure(EntityTypeBuilder<SettingsEntry> builder)
        {
            builder.HasData(
                Create("General:AuthorizationBaseUrl", null, "Base URL of Authorization service"),
                Create("General:AuthorizationAccessToken", null, "Access token to Authorization service"),
                Create("TokenValidation:Audience", null, "Comma separated audiences allowed"));
        }

        private SettingsEntry Create(string id, object value, string description = null)
        {
            return new SettingsEntry
            {
                Id = id,
                Value = value?.ToString(),
                Description = description
            };
        }
    }
}