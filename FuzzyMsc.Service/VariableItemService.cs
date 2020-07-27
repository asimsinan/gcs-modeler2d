using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class VariableItemService : Service<VariableItem>, IVariableItemService
    {
        private readonly IRepositoryAsync<VariableItem> _repository;
        public VariableItemService(IRepositoryAsync<VariableItem> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IVariableItemService : IService<VariableItem>, IBaseService
    {

    }
}
