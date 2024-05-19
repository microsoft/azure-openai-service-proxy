﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AzureOpenAIProxy.Management.Database;

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

    public virtual DbSet<ActiveAttendeeGrowthView> ActiveAttendeeGrowthViews { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventAttendee> EventAttendees { get; set; }

    public virtual DbSet<EventAttendeeRequest> EventAttendeeRequests { get; set; }

    public virtual DbSet<Metric> Metrics { get; set; }

    public virtual DbSet<MetricView> MetricViews { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    public virtual DbSet<OwnerCatalog> OwnerCatalogs { get; set; }

    public virtual DbSet<OwnerEventMap> OwnerEventMaps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("aoai", "model_type", new[] { "openai-chat", "openai-embedding", "openai-dalle3", "openai-whisper", "openai-completion", "openai-instruct", "azure-ai-search" })
            .HasPostgresExtension("aoai", "pgcrypto");

        modelBuilder.Entity<ActiveAttendeeGrowthView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("active_attendee_growth_view", "aoai");

            entity.Property(e => e.Attendees).HasColumnName("attendees");
            entity.Property(e => e.DateStamp).HasColumnName("date_stamp");
            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .HasColumnName("event_id");
        });

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
                .HasColumnName("end_timestamp");
            entity.Property(e => e.EventCode)
                .HasMaxLength(64)
                .HasColumnName("event_code");
            entity.Property(e => e.EventImageUrl)
                .HasMaxLength(256)
                .IsRequired(false)
                .HasColumnName("event_image_url");
            entity.Property(e => e.EventMarkdown)
                .HasMaxLength(8192)
                .HasColumnName("event_markdown");
            entity.Property(e => e.EventSharedCode)
                .HasMaxLength(64)
                .IsRequired(false)
                .HasColumnName("event_shared_code");
            entity.Property(e => e.MaxTokenCap).HasColumnName("max_token_cap");
            entity.Property(e => e.OrganizerEmail)
                .HasMaxLength(128)
                .HasColumnName("organizer_email");
            entity.Property(e => e.OrganizerName)
                .HasMaxLength(128)
                .HasColumnName("organizer_name");
            entity.Property(e => e.OwnerId)
                .HasMaxLength(128)
                .HasColumnName("owner_id");
            entity.Property(e => e.StartTimestamp)
                .HasColumnType("timestamp(6) without time zone")
                .HasColumnName("start_timestamp");
            entity.Property(e => e.TimeZoneLabel)
                .HasMaxLength(64)
                .HasColumnName("time_zone_label");
            entity.Property(e => e.TimeZoneOffset).HasColumnName("time_zone_offset");

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

            entity.HasIndex(e => e.ApiKey, "api_key_unique_index").IsUnique();

            entity.Property(e => e.UserId)
                .HasMaxLength(128)
                .HasColumnName("user_id");
            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .HasColumnName("event_id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.ApiKey)
                .HasMaxLength(36)
                .HasColumnName("api_key");

            entity.HasOne(d => d.Event).WithMany(p => p.EventAttendees)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("fk_eventattendee_event");
        });

        modelBuilder.Entity<EventAttendeeRequest>(entity =>
        {
            entity.HasKey(e => new { e.ApiKey, e.DateStamp }).HasName("eventattendeerequest_pkey");

            entity.ToTable("event_attendee_request", "aoai");

            entity.Property(e => e.ApiKey)
                .HasColumnType("character varying")
                .HasColumnName("api_key");
            entity.Property(e => e.DateStamp).HasColumnName("date_stamp");
            entity.Property(e => e.RequestCount).HasColumnName("request_count");
            entity.Property(e => e.TokenCount).HasColumnName("token_count");

            entity.HasOne(d => d.ApiKeyNavigation).WithMany(p => p.EventAttendeeRequests)
                .HasPrincipalKey(p => p.ApiKey)
                .HasForeignKey(d => d.ApiKey)
                .HasConstraintName("fk_eventattendeerequest_eventattendee");
        });

        modelBuilder.Entity<Metric>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("metric", "aoai");

            entity.HasIndex(e => e.EventId, "event_id_index");

            entity.Property(e => e.ApiKey)
                .HasColumnType("character varying")
                .HasColumnName("api_key");
            entity.Property(e => e.DateStamp)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("date_stamp");
            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .HasColumnName("event_id");
            entity.Property(e => e.Resource)
                .HasMaxLength(64)
                .HasColumnName("resource");
            entity.Property(e => e.TimeStamp)
                .HasDefaultValueSql("CURRENT_TIME")
                .HasColumnName("time_stamp");
            entity.Property(e => e.Usage)
                .HasColumnType("jsonb")
                .HasColumnName("usage");

            entity.HasOne(d => d.Event).WithMany()
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("fk_metric");
        });

        modelBuilder.Entity<MetricView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("metric_view", "aoai");

            entity.Property(e => e.CompletionTokens).HasColumnName("completion_tokens");
            entity.Property(e => e.DateStamp).HasColumnName("date_stamp");
            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .HasColumnName("event_id");
            entity.Property(e => e.PromptTokens).HasColumnName("prompt_tokens");
            entity.Property(e => e.Resource)
                .HasMaxLength(64)
                .HasColumnName("resource");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");
            entity.Property(e => e.TotalTokens).HasColumnName("total_tokens");
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
            entity.Property(e => e.EndpointUrlEncrypted)
                .HasColumnType("bytea")
                .HasColumnName("endpoint_url_encrypted");
            entity.Property(e => e.EndpointKeyEncrypted)
                .HasColumnType("bytea")
                .HasColumnName("endpoint_key_encrypted");
            entity.Property(e => e.FriendlyName)
                .HasMaxLength(64)
                .HasColumnName("friendly_name");
            entity.Property(e => e.Location)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("location");
            entity.Property(e => e.ModelType)
                .HasColumnName("model_type")
                .HasDefaultValueSql("'openai-chat'::aoai.model_type");
            entity.Property(e => e.OwnerId)
                .HasMaxLength(128)
                .HasColumnName("owner_id");

            entity.HasOne(d => d.Owner).WithMany(p => p.OwnerCatalogs)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("fk_groupmodels_group");
        });

        modelBuilder.Entity<OwnerEventMap>(entity =>
        {
            entity.HasKey(e => new { e.OwnerId, e.EventId }).HasName("ownereventmap_pkey");

            entity.ToTable("owner_event_map", "aoai");

            entity.Property(e => e.OwnerId)
                .HasMaxLength(128)
                .HasColumnName("owner_id");
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
