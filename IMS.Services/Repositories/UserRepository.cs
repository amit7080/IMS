using IMS.Data.Context;
using IMS.Data.Interface;
using IMS.Data.Model;
using IMS.Data.ViewModel;
using IMS.Services.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Services.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IUnitOfWork _unitofWork;
        private readonly IRepository<User> _userRepository;

        public UserRepository(IUnitOfWork unitofWork, IRepository<User> userRepository)
        {
            _unitofWork = unitofWork;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get All Users
        /// </summary>
        /// <returns></returns>
        public async Task<List<User>> GetAllUser()
        {
            // Find the user by username
            return await _userRepository.GetAll().ToListAsync();
        }
        public async Task<User> CreateUserAsync(User registerViewModel)
        {
            _ = _userRepository.Add(registerViewModel);
            _unitofWork.commit();
            return registerViewModel;
        }

        public async Task<bool> IsEmailExist(string email)
        {
            try
            {
                var userCount = await _userRepository.GetAll().Where(x => x.Email == email && !x.IsDeleted).CountAsync();
                if (userCount > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> InActiveUser(string userId)
        {

            var user = await _userRepository.GetByIdAsync(userId);
            user.IsActive = !user.IsActive;
            _userRepository.Update(user);
            _unitofWork.commit();
            return true;
        }
    }
}
