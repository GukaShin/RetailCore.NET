using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailCore.Domain.Entities;

namespace RetailCore.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ReceiptNumber).HasMaxLength(40).IsRequired();
        builder.HasIndex(s => s.ReceiptNumber).IsUnique();
        builder.HasIndex(s => new { s.StoreId, s.CreatedAt });

        builder.HasOne(s => s.Store).WithMany().HasForeignKey(s => s.StoreId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Cashier).WithMany().HasForeignKey(s => s.CashierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Shift).WithMany().HasForeignKey(s => s.ShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Items).WithOne(i => i.Sale!).HasForeignKey(i => i.SaleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Payments).WithOne(p => p.Sale!).HasForeignKey(p => p.SaleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.Receipt).WithOne(r => r.Sale!).HasForeignKey<Receipt>(r => r.SaleId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ProductNameSnapshot).HasMaxLength(200);
        builder.Property(i => i.BarcodeSnapshot).HasMaxLength(64);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ReferenceNumber).HasMaxLength(80);
    }
}

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("receipts");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ReceiptNumber).HasMaxLength(40);
        builder.Property(r => r.ReceiptText).HasMaxLength(8000);
        builder.Property(r => r.PdfPath).HasMaxLength(512);
        builder.HasIndex(r => r.SaleId).IsUnique();
    }
}

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("refunds");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Reason).HasMaxLength(400);

        builder.HasOne(r => r.Sale).WithMany().HasForeignKey(r => r.SaleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Cashier).WithMany().HasForeignKey(r => r.CashierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(r => r.Items).WithOne(i => i.Refund!).HasForeignKey(i => i.RefundId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class RefundItemConfiguration : IEntityTypeConfiguration<RefundItem>
{
    public void Configure(EntityTypeBuilder<RefundItem> builder)
    {
        builder.ToTable("refund_items");
        builder.HasKey(i => i.Id);
    }
}
