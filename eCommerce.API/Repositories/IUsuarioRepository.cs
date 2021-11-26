using eCommerce.API.Models;
using System.Collections.Generic;

namespace eCommerce.API.Repositories
{
    public interface IUsuarioRepository
    {
        List<Usuario> GetAll();

        Usuario Get(int id);

        void Insert(Usuario usuario);

        void Update(Usuario usuario);

        void Delete(int id);
    }
}