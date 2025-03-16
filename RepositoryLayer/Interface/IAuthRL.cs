using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface IAuthRL
    {
        User GetEmail(string email);
        User RegisterRL(User user);
        User LoginRL(UserDTO userDTO);
    }
}
