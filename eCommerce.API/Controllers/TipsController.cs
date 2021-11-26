using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using eCommerce.API.Models;
using System.Linq;
using Dapper.FluentMap;
using eCommerce.API.Mappers;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipsController : ControllerBase
    {
        private readonly IDbConnection _connection;

        public TipsController()
        {
            _connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=eCommerce;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            var sql = @"SELECT * FROM Usuarios WHERE Id = @Id;
                        SELECT * FROM Contatos WHERE  UsuarioId = @Id;
                        SELECT * FROM EnderecosEntrega WHERE UsuarioId = @Id;
                        SELECT D.* FROM Departamentos D JOIN UsuariosDepartamentos UD ON UD.DepartamentoId= D.Id
                        WHERE UD.UsuarioId = @Id";

            using (var multipleResultsSets = _connection.QueryMultiple(sql, new { Id = id }))
            {
                var usuario = multipleResultsSets.Read<Usuario>().SingleOrDefault();
                var contato = multipleResultsSets.Read<Contato>().SingleOrDefault();
                var enderecoEntregas = multipleResultsSets.Read<EnderecoEntrega>().ToList();
                var departamentos = multipleResultsSets.Read<Departamento>().ToList();

                if (usuario != null)
                {
                    usuario.Contato = contato;
                    usuario.EnderecosEntrega = enderecoEntregas;
                    usuario.Departamentos = departamentos;

                    return Ok(usuario);
                }
            }

            return NotFound();
        }

        [HttpGet("stored/usuarios")]
        public IActionResult GetStoreProcedure()
        {
            var usuarios = _connection.Query<Usuario>("SelecionarUsuarios", commandType: CommandType.StoredProcedure);
            return Ok(usuarios);
        }

        [HttpGet("stored/usuarios/{id:int}")]
        public IActionResult GetByIdStoreProcedure(int id)
        {
            var usuario = _connection.Query<Usuario>("SelecionarUsuario", new { Id = id }, commandType: CommandType.StoredProcedure);
            return Ok(usuario);
        }

        [HttpGet("mapper1/usuarios")]
        public IActionResult GetMapper1()
        {
            /*  
             *  Problema: Mapear colunas com nomes diferntes das propriedades do objeto
             *  Solucao 01: Nomear a coluna.                           
             */  

            var sql = @"select U.Id cod,u.Nome NomeCompleto,u.Email,u.Sexo,u.CPF,u.RG,
                               u.NomeMae NomeCompletoMae,u.SituacaoCadastro Situacao,u.DataCadastro 
                        from Usuarios U ";

            var usuarios = _connection.Query<UsuarioTwo>(sql);
            return Ok(usuarios);
        }

        [HttpGet("mapper2/usuarios")]
        public IActionResult GetMapper2()
        {
            /*  
             *  Problema: Mapear colunas com nomes diferntes das propriedades do objeto
             *  Solucao 02: C#(POO) Mapeamento por meio da biblioteca Dapper.FluentMap.                           
             */

            FluentMapper.Initialize(config =>
            {
                config.AddMap(new UsuarioTwoMap());
            });

            var sql = @"select * from Usuarios";

            var usuarios = _connection.Query<UsuarioTwo>(sql);
            return Ok(usuarios);
        }
    }


}