using Dapper;
using eCommerce.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace eCommerce.API.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDbConnection _connection;

        public UsuarioRepository()
        {
            _connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=eCommerce;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        public List<Usuario> GetAll()
        {
            var sql = @"SELECT U.*, C.*, EE.*, D.*
                        FROM usuarios U
                        LEFT JOIN contatos C ON U.id = C.usuarioid
                        LEFT JOIN enderecosentrega EE ON EE.usuarioid = U.id
                        LEFT JOIN UsuariosDepartamentos UE ON UE.usuarioid = U.id
                        LEFT JOIN Departamentos D ON D.id = UE.DepartamentoId";

            var listaUsuarios = new List<Usuario>();

            _connection.Query<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(sql,
            (usuario, contato, enderecoEntrega, departamento) =>
            {
                //Verificação usuario
                if (listaUsuarios.SingleOrDefault(u => u.Id == usuario.Id) == null)
                {
                    usuario.Departamentos = new List<Departamento>();
                    usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                    usuario.Contato = contato;
                    listaUsuarios.Add(usuario);
                }
                else
                {
                    usuario = listaUsuarios.SingleOrDefault(u => u.Id == usuario.Id);
                }

                //Verificaçao endereço
                if (usuario.EnderecosEntrega.SingleOrDefault(e => e.Id == enderecoEntrega.Id) == null)
                {
                    usuario.EnderecosEntrega.Add(enderecoEntrega);
                }

                //Verificaçao Departamento
                if (usuario.Departamentos.SingleOrDefault(d => d.Id == departamento.Id) == null)
                {
                    usuario.Departamentos.Add(departamento);
                }

                return usuario;
            });

            return listaUsuarios;
        }

        public Usuario Get(int id)
        {
            var sql = @"SELECT U.*, C.*, EE.*, D.*
                        FROM usuarios U
                        LEFT JOIN contatos C ON U.id = C.usuarioid
                        LEFT JOIN enderecosentrega EE ON EE.usuarioid = U.id
                        LEFT JOIN UsuariosDepartamentos UE ON UE.usuarioid = U.id
                        LEFT JOIN Departamentos D ON D.id = UE.DepartamentoId
                        WHERE U.Id = @Id";

            var listaUsuarios = new List<Usuario>();

            _connection.Query<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(sql,
            (usuario, contato, enderecoEntrega, departamento) =>
            {
                //Verificação usuario
                if (listaUsuarios.SingleOrDefault(u => u.Id == usuario.Id) == null)
                {
                    usuario.Departamentos = new List<Departamento>();
                    usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                    usuario.Contato = contato;
                    listaUsuarios.Add(usuario);
                }
                else
                {
                    usuario = listaUsuarios.SingleOrDefault(u => u.Id == usuario.Id);
                }

                //Verificaçao endereço
                if (usuario.EnderecosEntrega.SingleOrDefault(e => e.Id == enderecoEntrega.Id) == null)
                {
                    usuario.EnderecosEntrega.Add(enderecoEntrega);
                }

                //Verificaçao Departamento
                if (usuario.Departamentos.SingleOrDefault(d => d.Id == departamento.Id) == null)
                {
                    usuario.Departamentos.Add(departamento);
                }

                return usuario;
            }, new { Id = id });

            return listaUsuarios.SingleOrDefault();
        }

        public void Insert(Usuario usuario)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();

            try
            {
                string sqlUsuario = @"INSERT INTO Usuarios(Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro)
                        VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                usuario.Id = _connection.Query<int>(sqlUsuario, usuario, transaction).Single();

                if (usuario.Contato != null)
                {
                    usuario.Contato.UsuarioId = usuario.Id;
                    string sqlContato = @"INSERT INTO Contatos(UsuarioId, Telefone, Celular)
                        VALUES (@UsuarioId, @Telefone, @Celular);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    usuario.Contato.Id = _connection.Query<int>(sqlContato, usuario.Contato, transaction).Single();
                }

                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        enderecoEntrega.UsuarioId = usuario.Id;
                        string sqlEnderecoEntrega = @"INSERT INTO
                        EnderecosEntrega(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento)
                        VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEnderecoEntrega, enderecoEntrega, transaction).Single();
                    }
                }

                if (usuario.Departamentos != null && usuario.Departamentos.Count > 0)
                {
                    foreach (var departamento in usuario.Departamentos)
                    {
                        string sqlUsuariosDepartamentos = @"INSERT INTO
                        UsuariosDepartamentos(UsuarioId, DepartamentoId) VALUES (@UsuarioId, @DepartamentoId);";
                        _connection.Execute(sqlUsuariosDepartamentos, new { UsuarioId = usuario.Id, DepartamentoId = departamento.Id }, transaction);
                    }
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Update(Usuario usuario)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();

            try
            {
                string sqlUsuario = @"UPDATE Usuarios SET
                        Nome = @Nome, Email = @Email, Sexo = @Sexo, RG = @RG , CPF = @CPF, NomeMae = @NomeMae, SituacaoCadastro = @SituacaoCadastro
                        WHERE Id = @Id;";
                _connection.Execute(sqlUsuario, usuario, transaction);

                if (usuario.Contato != null)
                {
                    string sqlContato = @"UPDATE Contatos SET
                        Telefone = @Telefone, Celular = @Celular
                        WHERE Id = @Id;";
                    _connection.Execute(sqlContato, usuario.Contato, transaction);
                }

                var sqlDeletarEnderecoEntrega = "DELETE FROM EnderecosEntrega WHERE UsuarioId = @Id";
                _connection.Execute(sqlDeletarEnderecoEntrega, usuario, transaction);

                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        string sqlEnderecoEntrega = @"INSERT INTO
                        EnderecosEntrega(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento)
                        VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEnderecoEntrega, enderecoEntrega, transaction).Single();
                    }
                }

                var sqlDeletarUsuariosEnderecos = "DELETE FROM UsuariosDepartamentos WHERE UsuarioId = @Id";
                _connection.Execute(sqlDeletarUsuariosEnderecos, usuario, transaction);

                if (usuario.Departamentos != null && usuario.Departamentos.Count > 0)
                {
                    foreach (var departamento in usuario.Departamentos)
                    {
                        string sqlUsuariosDepartamentos = @"INSERT INTO UsuariosDepartamentos(UsuarioId, DepartamentoId) 
                                                            VALUES (@UsuarioId, @DepartamentoId);";
                        _connection.Execute(sqlUsuariosDepartamentos, new { UsuarioId = usuario.Id, DepartamentoId = departamento.Id }, transaction);
                    }
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Delete(int id)
        {
            _connection.Execute("DELETE FROM Usuarios WHERE Id = @Id", new { Id = id });
        }
    }
}