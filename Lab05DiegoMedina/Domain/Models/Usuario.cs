using System;
using System.Collections.Generic;

namespace Lab05DiegoMedina.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string UserName { get; set; } = null!;

    public Rol Rol { get; set; } = Rol.Estudiante;

    public string Nombre { get; set; } = null!;

    public string? Correo { get; set; }

    public string Password { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime CreadoEn { get; set; }
}
