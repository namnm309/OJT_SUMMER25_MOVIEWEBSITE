using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI.Areas.ConcessionManagement.Models;

namespace UI.Areas.ConcessionManagement.Services
{
    public interface IConcessionManagementUIService
    {
        Task<List<ConcessionItemViewModel>> GetAllConcessionItemsAsync();
        Task<ConcessionItemViewModel?> GetConcessionItemByIdAsync(Guid id);
        Task<bool> CreateConcessionItemAsync(ConcessionItemViewModel model);
        Task<bool> UpdateConcessionItemAsync(Guid id, ConcessionItemViewModel model);
        Task<bool> DeleteConcessionItemAsync(Guid id);
    }
}
