using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPeliculas.Models.Dtos
{
    public class UsuarioAuthLoginDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Usuario { get; set; }
        [Required(ErrorMessage = "El usuario es obligatorio")]       
        public string Password { get; set; }
    }
}
