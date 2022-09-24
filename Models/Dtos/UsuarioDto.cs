using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPeliculas.Models.Dtos
{
    public class UsuarioDto
    {
        public string UsuarioA { get; set; }
        public byte[] PasswordHash { get; set; }
    }
}
