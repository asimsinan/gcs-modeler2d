using FuzzyMsc.Dto;
using FuzzyMsc.Service;
using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Pattern.UnitOfWork;
using System;
using System.Linq;

namespace FuzzyMsc.Bll
{
    public interface IUserManager : IBaseManager
    {
        ResultDTO Save(int parameter1, string parametre2);

        ResultDTO Fetch();
    }

    public class UserManager : IUserManager
    {
        IUnitOfWorkAsync _unitOfWork;
        IUserService _userService;

        public UserManager(
            IUnitOfWorkAsync unitOfWork,
            IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        #region Fetching Operations

        public ResultDTO Save(int parameter1,string parameter2)
        {
            ResultDTO result = new ResultDTO();
            try
            {
                result.Message = "Save success";
                result.ResultObject = null;
                result.Success = true;                
            }
            catch (Exception ex)
            {
                result.Message = "Save unsuccessful";
                result.Success = false;
                result.Exception = ex;           
            }
            return result;
        }


        public ResultDTO Fetch()
        {
            ResultDTO result = new ResultDTO();
            try
            {
              
                result.Message = "Success";
                result.ResultObject = _userService.Queryable().FirstOrDefault().name;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = "Error";
                result.Success = false;
                result.Exception = ex;
            }
            return result;
           
        }

        #endregion

    }
}
