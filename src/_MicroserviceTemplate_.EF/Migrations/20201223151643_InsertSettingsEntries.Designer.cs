// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using _MicroserviceTemplate_.EF;

namespace _MicroserviceTemplate_.EF.Migrations
{
    [DbContext(typeof(_MicroserviceTemplate_DbContext))]
    [Migration("20201223151643_InsertSettingsEntries")]
    partial class InsertSettingsEntries
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("_MicroserviceTemplate_.Domain.Settings.SettingsEntry", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnName("SettingsEntryId")
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("CreateTimeUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdateTimeUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("SettingsEntries");

                    b.HasData(
                        new
                        {
                            Id = "General:AuthorizationBaseUrl",
                            CreateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Base URL of Authorization service",
                            UpdateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Id = "General:AuthorizationAccessToken",
                            CreateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Access token to Authorization service",
                            UpdateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Id = "TokenValidation:Audience",
                            CreateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Comma separated audiences allowed",
                            UpdateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });
#pragma warning restore 612, 618
        }
    }
}


