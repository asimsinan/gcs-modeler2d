using FuzzyMsc.Service.Interface;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;

namespace FuzzyMsc.Service
{
    public class UserService : Service<User>, IUserService
    {
        private readonly IRepositoryAsync<User> _repository;
        public UserService(IRepositoryAsync<User> repository) : base(repository)
        {
            _repository = repository;
        }
    }

    public interface IUserService : IService<User>, IBaseService
    {

    }
}
