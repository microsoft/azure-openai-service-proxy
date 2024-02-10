using AzureOpenAIProxy.Management.Database;
using Microsoft.EntityFrameworkCore;

namespace AzureOpenAIProxy.Management;

public partial class AoaiProxyContext : DbContext
{
    public AoaiProxyContext()
    {
    }

    public AoaiProxyContext(DbContextOptions<AoaiProxyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventAttendee> EventAttendees { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    public virtual DbSet<OwnerCatalog> OwnerCatalogs { get; set; }

    public virtual DbSet<OwnerEventMap> OwnerEventMaps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum(
            "aoai",
            "model_type",
            ["openai-chat", "openai-embedding", "openai-dalle2", "openai-dalle3", "openai-whisper", "openai-completion"]);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("event_pkey");

            entity.ToTable("event", "aoai");

            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("event_id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.DailyRequestCap).HasColumnName("daily_request_cap");
            entity.Property(e => e.EndTimestamp)
                .HasColumnType("timestamp(6) without time zone")
                .HasColumnName("end_utc");
            entity.Property(e => e.EventCode)
                .HasMaxLength(64)
                .HasColumnName("event_code");
            entity.Property(e => e.EventMarkdown)
                .HasMaxLength(8192)
                .HasColumnName("event_markdown");
            entity.Property(e => e.EventUrl)
                .HasMaxLength(256)
                .HasColumnName("event_url");
            entity.Property(e => e.EventUrlText)
                .HasMaxLength(256)
                .HasColumnName("event_url_text");
            entity.Property(e => e.MaxTokenCap).HasColumnName("max_token_cap");
            entity.Property(e => e.OrganizerEmail)
                .HasMaxLength(128)
                .HasColumnName("organizer_email");
            entity.Property(e => e.OrganizerName)
                .HasMaxLength(128)
                .HasColumnName("organizer_name");
            entity.Property(e => e.OwnerId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("owner_id");
            entity.Property(e => e.StartTimestamp)
                .HasColumnType("timestamp(6) without time zone")
                .HasColumnName("start_utc");

            entity.HasMany(d => d.Catalogs).WithMany(p => p.Events)
                .UsingEntity<Dictionary<string, object>>(
                    "EventCatalogMap",
                    r => r.HasOne<OwnerCatalog>().WithMany()
                        .HasForeignKey("CatalogId")
                        .HasConstraintName("fk_eventcatalogmap_ownercatalog"),
                    l => l.HasOne<Event>().WithMany()
                        .HasForeignKey("EventId")
                        .HasConstraintName("fk_eventcatalogmap_event"),
                    j =>
                    {
                        j.HasKey("EventId", "CatalogId").HasName("eventcatalogmap_pkey");
                        j.ToTable("event_catalog_map", "aoai");
                        j.IndexerProperty<string>("EventId")
                            .HasMaxLength(50)
                            .HasColumnName("event_id");
                        j.IndexerProperty<Guid>("CatalogId").HasColumnName("catalog_id");
                    });
        });

        modelBuilder.Entity<EventAttendee>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.EventId }).HasName("eventattendee_pkey");

            entity.ToTable("event_attendee", "aoai");

            entity.Property(e => e.UserId)
                .HasMaxLength(128)
                .HasColumnName("user_id");
            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .HasColumnName("event_id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.ApiKey).HasColumnName("api_key");

            entity.HasOne(d => d.Event).WithMany(p => p.EventAttendees)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("fk_eventattendee_event");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("owner_pkey");

            entity.ToTable("owner", "aoai");

            entity.Property(e => e.OwnerId)
                .HasMaxLength(128)
                .HasColumnName("owner_id");

            entity.Property(e => e.Email)
                .HasMaxLength(128)
                .HasColumnName("email");

            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        modelBuilder.Entity<OwnerCatalog>(entity =>
        {
            entity.HasKey(e => e.CatalogId).HasName("ownercatalog_pkey");

            entity.ToTable("owner_catalog", "aoai");

            entity.Property(e => e.CatalogId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("catalog_id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.DeploymentName)
                .HasMaxLength(64)
                .HasColumnName("deployment_name");
            entity.Property(e => e.EndpointKey)
                .HasMaxLength(128)
                .HasColumnName("endpoint_key");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.ResourceName)
                .HasMaxLength(64)
                .HasColumnName("resource_name");
            entity.Property(e => e.ModelType)
                .HasColumnName("model_type")
                .HasDefaultValueSql("'openai-chat'::aoai.model_type");


            entity.HasOne(d => d.Owner).WithMany(p => p.OwnerCatalogs)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("fk_groupmodels_group");
        });

        modelBuilder.Entity<OwnerEventMap>(entity =>
        {
            entity.HasKey(e => new { e.OwnerId, e.EventId }).HasName("ownereventmap_pkey");

            entity.ToTable("owner_event_map", "aoai");

            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .HasColumnName("event_id");
            entity.Property(e => e.Creator).HasColumnName("creator");

            entity.HasOne(d => d.Event).WithMany(p => p.OwnerEventMaps)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("fk_ownereventmap_event");

            entity.HasOne(d => d.Owner).WithMany(p => p.OwnerEventMaps)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("fk_ownereventmap_owner");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
