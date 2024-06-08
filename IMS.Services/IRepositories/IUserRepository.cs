using IMS.Data.Model;

namespace IMS.Services.IRepositories
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User registerViewModel);
        Task<List<User>> GetAllUser();
        Task<bool> IsEmailExist(string email);
        Task<bool> InActiveUser(string userId);
    }
}
