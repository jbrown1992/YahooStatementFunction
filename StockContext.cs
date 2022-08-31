using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

public class StockContext : DbContext
{
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<IncomeStatement> IncomeStatements { get; set; }
    public DbSet<CashFlow> CashFlows { get; set; }

    public string DbPath { get; }

    public StockContext()
    {
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server =(localdb)\\MSSQLLocalDB; Database=StocksDB; Trusted_Connection=True; MultipleActiveResultSets=True;");
}
