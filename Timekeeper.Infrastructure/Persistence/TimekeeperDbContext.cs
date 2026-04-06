using Microsoft.EntityFrameworkCore;
using Timekeeper.Domain.Entities;

namespace Timekeeper.Infrastructure.Persistence;

public sealed class TimekeeperDbContext(DbContextOptions<TimekeeperDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayRun> PayRuns => Set<PayRun>();
    public DbSet<PayRunLine> PayRunLines => Set<PayRunLine>();
    public DbSet<TaxProfile> TaxProfiles => Set<TaxProfile>();
    public DbSet<TaxRule> TaxRules => Set<TaxRule>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.EmployeeNumber).HasMaxLength(32);
            entity.Property(item => item.FullName).HasMaxLength(160);
            entity.Property(item => item.Department).HasMaxLength(120);
            entity.Property(item => item.WorkState).HasMaxLength(16);
            entity.Property(item => item.HourlyRate).HasPrecision(12, 2);
            entity.HasIndex(item => item.EmployeeNumber).IsUnique();
        });

        modelBuilder.Entity<TaxProfile>(entity =>
        {
            entity.HasKey(item => item.EmployeeId);
            entity.Property(item => item.StateCode).HasMaxLength(16);
            entity.Property(item => item.LocalTaxCode).HasMaxLength(32);
            entity.Property(item => item.AdditionalFederalWithholding).HasPrecision(12, 2);
            entity.HasOne(item => item.Employee)
                .WithOne(item => item.TaxProfile)
                .HasForeignKey<TaxProfile>(item => item.EmployeeId);
        });

        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.HoursWorked).HasPrecision(8, 2);
            entity.Property(item => item.OvertimeHours).HasPrecision(8, 2);
            entity.Property(item => item.ProjectCode).HasMaxLength(32);
            entity.Property(item => item.Notes).HasMaxLength(256);
            entity.HasOne(item => item.Employee)
                .WithMany(item => item.TimeEntries)
                .HasForeignKey(item => item.EmployeeId);
        });

        modelBuilder.Entity<PayRun>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(120);
        });

        modelBuilder.Entity<PayRunLine>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.GrossPay).HasPrecision(12, 2);
            entity.Property(item => item.FederalTax).HasPrecision(12, 2);
            entity.Property(item => item.StateTax).HasPrecision(12, 2);
            entity.Property(item => item.LocalTax).HasPrecision(12, 2);
            entity.Property(item => item.EmployerTax).HasPrecision(12, 2);
            entity.Property(item => item.NetPay).HasPrecision(12, 2);
            entity.HasOne(item => item.PayRun)
                .WithMany(item => item.Lines)
                .HasForeignKey(item => item.PayRunId);
            entity.HasOne(item => item.Employee)
                .WithMany(item => item.PayRunLines)
                .HasForeignKey(item => item.EmployeeId);
        });

        modelBuilder.Entity<TaxRule>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Code).HasMaxLength(48);
            entity.Property(item => item.Name).HasMaxLength(160);
            entity.Property(item => item.RegionCode).HasMaxLength(32);
            entity.Property(item => item.RatePercent).HasPrecision(10, 4);
            entity.Property(item => item.ParametersJson).HasColumnType("jsonb");
            entity.Property(item => item.SourceSystem).HasMaxLength(64);
            entity.Property(item => item.SourceReference).HasMaxLength(160);
            entity.Property(item => item.Notes).HasMaxLength(512);
            entity.HasIndex(item => new { item.Code, item.EffectiveFrom });
        });
    }
}
