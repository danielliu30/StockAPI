using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAPI.Interfaces
{
    public interface IConnectionSetting
    {
        string ConnectionString { get; set; }

        string DatabaseName { get; set; }
    }
}
