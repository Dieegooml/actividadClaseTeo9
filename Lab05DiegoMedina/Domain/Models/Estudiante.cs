using System;
using System.Collections.Generic;

namespace Lab05DiegoMedina.Models;

public partial class Estudiante
{
    public int IdEstudiante { get; set; }

    public string Nombre { get; set; } = null!;

    public int Edad { get; set; }

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public virtual ICollection<Asistencia> Asistencia { get; set; } = new List<Asistencia>();

    public virtual ICollection<Evaluacione> Evaluaciones { get; set; } = new List<Evaluacione>();

    public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
