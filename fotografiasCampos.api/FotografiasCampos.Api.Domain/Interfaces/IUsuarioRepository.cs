using FotografiasCampos.Api.Domain.POCOs;

namespace FotografiasCampos.Api.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorUsernameAsync(string username);
    Task<bool> CrearAsync(Usuario usuario);
}