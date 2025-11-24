using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UziSport.Model;

namespace UziSport
{
    public interface ILookupService
    {
        Task<IReadOnlyList<CatalogInfo>> GetCatalogsAsync(bool forceRefresh = false);
    }

}
