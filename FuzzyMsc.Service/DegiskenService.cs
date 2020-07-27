using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class VariableService : Service<Variable>, IVariableService
    {
        private readonly IRepositoryAsync<Variable> _repository;
        public VariableService(IRepositoryAsync<Variable> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IVariableService : IService<Variable>, IBaseService
    {

    }
}