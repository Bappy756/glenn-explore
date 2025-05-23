using Api.Features.Auth.Models;
using Api.Features.OpenRouter.Models;
using Api.Source.Features.Game;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Infrastructure.Database;

public class ApplicationDbContext : IdentityDbContext<User>
{   
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<RaceResult> RaceResults => Set<RaceResult>();
    public DbSet<LLMMessage> LLMMessages => Set<LLMMessage>();
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // User entity (from IdentityDbContext)
        builder.Entity<User>();
        
        // Message indexes
        builder.Entity<Message>(b =>
        {
            b.HasIndex(m => m.SentAt)
                .HasDatabaseName("IX_Messages_SentAt");
            b.HasIndex(m => new { m.PlayerId, m.SentAt })
                .HasDatabaseName("IX_Messages_PlayerId_SentAt");
            b.HasIndex(m => m.Type)
                .HasDatabaseName("IX_Messages_Type");
        });
        
        // Player indexes
        builder.Entity<Player>(b =>
        {
            b.HasIndex(p => p.PlayerId)
                .HasDatabaseName("IX_Players_PlayerId");
            b.HasIndex(p => p.LastSeen)
                .HasDatabaseName("IX_Players_LastSeen");
            b.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Players_Name");
        });

        // RaceResult indexes and relationships
        builder.Entity<RaceResult>(b =>
        {
            // Indexes for common queries
            b.HasIndex(r => r.PlayerId)
                .HasDatabaseName("IX_RaceResults_PlayerId");
            b.HasIndex(r => r.TrackId)
                .HasDatabaseName("IX_RaceResults_TrackId");
            b.HasIndex(r => r.CompletedAt)
                .HasDatabaseName("IX_RaceResults_CompletedAt");
            b.HasIndex(r => new { r.TrackId, r.Time })
                .HasDatabaseName("IX_RaceResults_TrackId_Time");
            b.HasIndex(r => new { r.PlayerId, r.TrackId, r.Time })
                .HasDatabaseName("IX_RaceResults_PlayerId_TrackId_Time");

            // Relationship with Player
            b.HasOne(r => r.Player)
                .WithMany()
                .HasForeignKey(r => r.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LLMMessage configuration and indexes
        builder.Entity<LLMMessage>(b =>
        {
            // Primary index for conversation lookup (most common query)
            b.HasIndex(m => new { m.ConversationId, m.SentAt })
                .HasDatabaseName("IX_LLMMessages_ConversationId_SentAt");
            
            // Index for role-based queries
            b.HasIndex(m => new { m.Role, m.SentAt })
                .HasDatabaseName("IX_LLMMessages_Role_SentAt");
            
            // Index for tool-related queries
            b.HasIndex(m => m.ToolName)
                .HasDatabaseName("IX_LLMMessages_ToolName");
            
            // General timestamp index
            b.HasIndex(m => m.SentAt)
                .HasDatabaseName("IX_LLMMessages_SentAt");

            // Set reasonable max lengths for string fields
            b.Property(m => m.ConversationId)
                .HasMaxLength(100);
            
            b.Property(m => m.Role)
                .HasMaxLength(50);
            
            b.Property(m => m.ToolName)
                .HasMaxLength(100);
        });
    }
} 