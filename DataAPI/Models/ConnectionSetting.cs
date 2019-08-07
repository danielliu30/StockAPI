using DataAPI.Interfaces;

namespace DataAPI.Models
{
    public class ConnectionSetting : IConnectionSetting
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }
    }
}
