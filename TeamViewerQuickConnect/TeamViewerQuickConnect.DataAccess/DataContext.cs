using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickConnect.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Item> Items { get; set; }

        public string DbPath { get; }

        public DataContext()
        {
            DbPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\QuickConnect.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source = {DbPath}");
        }

    }
}
