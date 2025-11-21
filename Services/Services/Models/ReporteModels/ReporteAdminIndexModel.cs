using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.ReporteModels
{
    public class ReporteAdminIndexModel : ReporteIndexModel
    {
        public string NombreUsuario { get; set; } = string.Empty;
    }
}
