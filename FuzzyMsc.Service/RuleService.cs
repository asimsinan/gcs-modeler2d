using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class RuleService : Service<Rule>, IRuleService
    {
        private readonly IRepositoryAsync<Rule> _repository;
        public RuleService(IRepositoryAsync<Rule> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IRuleService : IService<Rule>, IBaseService
    {

    }
}