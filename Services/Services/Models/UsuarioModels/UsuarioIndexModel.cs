using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.UsuarioModels
{
    public class UsuarioIndexModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
    }
}
