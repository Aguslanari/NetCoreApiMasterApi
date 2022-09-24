using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ApiPeliculas.Models;
using ApiPeliculas.Models.Dtos;
using ApiPeliculas.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApiPeliculas.Controllers
{
    [Authorize]
    [Route("api/Usuarios")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ApiPeliculasUsuarios")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public UsuariosController(IUsuarioRepository userRepo, IMapper mapper, IConfiguration config)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _config = config;
        }

        /// <summary>
        /// Registro de nuevo usuario
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetUsuarios()
        {
            var listaUsuarios = _userRepo.GetUsuarios();

            var listaUsuariosDto = new List<UsuarioDto>();

            foreach (var lista in listaUsuarios)
            {
                listaUsuariosDto.Add(_mapper.Map<UsuarioDto>(lista));
            }
            return Ok(listaUsuariosDto);
        }

        /// <summary>
        /// Obtener un usuario individual
        /// </summary>
        /// <param name="usuarioId"> Este es el id de la usuario</param>
        /// <returns></returns>
        [HttpGet("{usuarioId:int}", Name = "GetUsuario")]
        public IActionResult GetUsuario(int usuarioId)
        {
            var itemUsuario = _userRepo.GetUsuario(usuarioId);

            if (itemUsuario == null)
            {
                return NotFound();
            }

            var itemUsuarioDto = _mapper.Map<UsuarioDto>(itemUsuario);
            return Ok(itemUsuarioDto);
        }

        /// <summary>
        /// Registro de nuevo usuario
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Registro")]
        public IActionResult Registro(UsuarioAuthDto usuarioAuthDto)
        {
            usuarioAuthDto.Usuario = usuarioAuthDto.Usuario.ToLower();

            if (_userRepo.ExisteUsuario(usuarioAuthDto.Usuario))
            {
                return BadRequest("El usuario ya existe");
            }

            var usuarioACrear = new Usuario
            {
                UsuarioA = usuarioAuthDto.Usuario
            };

            var usuarioCreado = _userRepo.Registro(usuarioACrear, usuarioAuthDto.Password);
            return Ok(usuarioCreado);
        }

        /// <summary>
        /// Acceso/Autenticación de usuario
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UsuarioAuthLoginDto usuarioAuthLoginDto)
        {            
            //throw new Exception("Error generado");

            var usuarioDesdeRepo = _userRepo.Login(usuarioAuthLoginDto.Usuario, usuarioAuthLoginDto.Password);

            if (usuarioDesdeRepo == null)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, usuarioDesdeRepo.Id.ToString()),
            new Claim(ClaimTypes.Name, usuarioDesdeRepo.UsuarioA.ToString())
        };

            //Generación de token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credenciales
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }                   
        }
}