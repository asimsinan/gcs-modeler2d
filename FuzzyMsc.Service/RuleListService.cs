using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class RuleListService : Service<RuleList>, IRuleListService
    {
        private readonly IRepositoryAsync<RuleList> _repository;
        public RuleListService(IRepositoryAsync<RuleList> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IRuleListService : IService<RuleList>, IBaseService
    {

    }
}
