using eCommerce.API.Models;
using eCommerce.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepository _repository;

        public UsuariosController()
        {
            _repository = new UsuarioRepository();
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_repository.GetAll());
        }

        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            var usuario = _repository.Get(id);
            if (usuario is null)
            {
                return NotFound($"Nenhum usuário cadastrado para o Id:{id}");
            }

            return Ok(usuario);
        }

        [HttpPost()]
        public IActionResult Post([FromBody] Usuario usuario)
        {
            _repository.Insert(usuario);
            return Ok(usuario);
        }

        [HttpPut()]
        public IActionResult Put([FromBody] Usuario usuario)
        {
            _repository.Update(usuario);
            return Ok(usuario);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var usuario = _repository.Get(id);
            if (usuario is null)
            {
                return NotFound($"Nenhum usuário cadastrado para o Id:{id}");
            }

            _repository.Delete(id);

            return Ok(usuario);
        }
    }
}