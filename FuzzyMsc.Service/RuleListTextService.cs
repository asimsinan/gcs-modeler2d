using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class RuleListTextService : Service<RuleListText>, IRuleListTextService
    {
        private readonly IRepositoryAsync<RuleListText> _repository;
        public RuleListTextService(IRepositoryAsync<RuleListText> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IRuleListTextService : IService<RuleListText>, IBaseService
    {

    }
}
