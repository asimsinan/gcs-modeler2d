using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class RuleListItemService : Service<RuleListItem>, IRuleListItemService
    {
        private readonly IRepositoryAsync<RuleListItem> _repository;
        public RuleListItemService(IRepositoryAsync<RuleListItem> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IRuleListItemService : IService<RuleListItem>, IBaseService
    {

    }
}
