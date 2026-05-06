 using FotografiasCampos.Api.Domain.POCOs;

namespace FotografiasCampos.Api.Domain.Interfaces;

public interface IJwtService
{
    string GenerarToken(Usuario usuario);
}
