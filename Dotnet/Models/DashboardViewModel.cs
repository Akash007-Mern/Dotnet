using System.Collections.Generic;

namespace Dotnet.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public IEnumerable<UseList> RecentUsers { get; set; } = new List<UseList>();
    }
}
