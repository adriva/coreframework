﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Adriva.Extensions.Analytics.Server.Entities;
using System;

namespace Adriva.Extensions.Analytics.Repository.EntityFramework
{
    /// <summary>
    /// Provides the base implementation of an entity framework DbContext that can be used to work with analytics items.
    /// </summary>
    public class AnalyticsDatabaseContext : DbContext
    {
        private readonly IDatabaseModelBuilder DatabaseModelBuilder;

        /// <summary>
        /// Gets or sets a collection of AnalyticsItem objects that can be used to query or save the instances.
        /// </summary>
        /// <value>An instance of a DbSet class that represents the AnalyticsItem collection.</value>
        public DbSet<AnalyticsItem> AnalyticsItems { get; set; }

        /// <summary>
        /// Gets or sets a collection of RequestItem objects that can be used to query or save the instances.
        /// </summary>
        /// <value>An instance of a DbSet class that represents the RequestItem collection.</value>
        public DbSet<RequestItem> Requests { get; set; }

        /// <summary>
        /// Gets or sets a collection of ExceptionItem objects that can be used to query or save the instances.
        /// </summary>
        /// <value>An instance of a DbSet class that represents the ExceptionItem collection.</value>
        public DbSet<ExceptionItem> Exceptions { get; set; }

        /// <summary>
        /// Gets or sets a collection of MessageItem objects that can be used to query or save the instances.
        /// </summary>
        /// <value>An instance of a DbSet class that represents the MessageItem collection.</value>
        public DbSet<MessageItem> Messages { get; set; }

        /// <summary>
        /// Gets or sets a collection of MetricItem objects that can be used to query or save the instances.
        /// </summary>
        /// <value>An instance of a DbSet class that represents the MetricItem collection.</value>
        public DbSet<MetricItem> Metrics { get; set; }

        /// <summary>
        /// Gets or sets a collection of DependencyItem objects that can be used to query or save the instances.
        /// </summary>
        /// <value>An instance of a DbSet class that represents the DependencyItem collection.</value>
        public DbSet<DependencyItem> Dependencies { get; set; }

        /// <summary>
        /// Gets or sets a collection of AvailabilityItem objects that can be used to query or save the instances.
        /// </summary>
        /// <value>An instance of a DbSet class that represents the AvailabilityItem collection.</value>
        public DbSet<AvailabilityItem> AvailabilityItems { get; set; }

        /// <summary>
        /// Gets or sets a collection of EventItem objects that can be used to query or save the instances.
        /// </summary>
        /// <value>An instance of a DbSet class that represents the EventItem collection.</value>
        public DbSet<EventItem> Events { get; set; }

        /// <summary>
        /// Initializes a new instance of the Adriva.Extensions.Analytics.Repository.EntityFramework.AnalyticsDatabaseContext class using the specified options.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        /// <param name="serviceProvider">The IServiceProvider instance that will be used to resolve dependencies.</param>
        public AnalyticsDatabaseContext(DbContextOptions<AnalyticsDatabaseContext> options, IServiceProvider serviceProvider) : base(options)
        {
            this.ChangeTracker.AutoDetectChangesEnabled = false;
            this.ChangeTracker.LazyLoadingEnabled = false;
            this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            this.DatabaseModelBuilder = serviceProvider?.GetService<IDatabaseModelBuilder>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnalyticsItem>(
                e =>
                {
                    e.ToTable("AnalyticsItems");
                    e.HasKey(x => x.Id);
                    e.Property(x => x.Id).ValueGeneratedOnAdd();

                    e.HasOne(x => x.RequestItem)
                        .WithOne(x => x.AnalyticsItem)
                        .HasPrincipalKey<AnalyticsItem>(x => x.Id)
                        .HasForeignKey<RequestItem>(x => x.AnalyticsItemId);

                    e.HasOne(x => x.MessageItem)
                        .WithOne(x => x.AnalyticsItem)
                        .HasPrincipalKey<AnalyticsItem>(x => x.Id)
                        .HasForeignKey<MessageItem>(x => x.AnalyticsItemId);

                    e.HasOne(x => x.AvailabilityItem)
                        .WithOne(x => x.AnalyticsItem)
                        .HasPrincipalKey<AnalyticsItem>(x => x.Id)
                        .HasForeignKey<AvailabilityItem>(x => x.AnalyticsItemId);

                    e.HasMany(x => x.Exceptions)
                        .WithOne(x => x.AnalyticsItem)
                        .HasForeignKey(x => x.AnalyticsItemId)
                        .HasPrincipalKey(x => x.Id);

                    e.HasMany(x => x.Metrics)
                        .WithOne(x => x.AnalyticsItem)
                        .HasForeignKey(x => x.AnalyticsItemId)
                        .HasPrincipalKey(x => x.Id);

                    e.HasMany(x => x.Events)
                        .WithOne(x => x.AnalyticsItem)
                        .HasForeignKey(x => x.AnalyticsItemId)
                        .HasPrincipalKey(x => x.Id);

                    e.HasMany(x => x.Dependencies)
                        .WithOne(x => x.AnalyticsItem)
                        .HasForeignKey(x => x.AnalyticsItemId)
                        .HasPrincipalKey(x => x.Id);

                    e.HasIndex(x => x.InstrumentationKey);
                }
            );

            modelBuilder.Entity<ExceptionItem>(
                e =>
                {
                    e.ToTable("Exceptions");
                    e.HasKey(x => x.Id);
                    e.Property(x => x.Id).ValueGeneratedOnAdd();
                    e.HasIndex(x => x.AnalyticsItemId);
                }
            );

            modelBuilder.Entity<RequestItem>(
                e =>
                {
                    e.ToTable("Requests");
                    e.HasKey(x => x.Id);
                    e.Property(x => x.Id).ValueGeneratedOnAdd();
                    e.HasIndex(x => x.AnalyticsItemId);
                }
            );

            modelBuilder.Entity<EventItem>(
                e =>
                {
                    e.ToTable("Events");
                    e.HasKey(x => x.Id);
                    e.Property(x => x.Id).ValueGeneratedOnAdd();
                    e.HasIndex(x => x.AnalyticsItemId);
                }
            );

            modelBuilder.Entity<MetricItem>(
                e =>
                {
                    e.ToTable("Metrics");
                    e.HasKey(x => x.Id);
                    e.Property(x => x.Id).ValueGeneratedOnAdd();
                    e.HasIndex(x => x.AnalyticsItemId);
                }
            );

            modelBuilder.Entity<AvailabilityItem>(
                e =>
                {
                    e.ToTable("Availability");
                    e.HasKey(x => x.Id);
                    e.Property(x => x.Id).ValueGeneratedOnAdd();
                    e.HasIndex(x => x.AnalyticsItemId);
                }
            );

            modelBuilder.Entity<MessageItem>(
                e =>
                {
                    e.ToTable("Messages");
                    e.HasKey(x => x.Id);
                    e.Property(x => x.Id).ValueGeneratedOnAdd();
                    e.HasIndex(x => x.AnalyticsItemId);
                }
            );

            modelBuilder.Entity<DependencyItem>(
                e =>
                {
                    e.ToTable("Dependencies");
                    e.HasKey(x => x.Id);
                    e.Property(x => x.Id).ValueGeneratedOnAdd();
                    e.HasIndex(x => x.AnalyticsItemId);
                }
            );

            this.DatabaseModelBuilder?.OnModelCreating(this, modelBuilder);
        }
    }
}
