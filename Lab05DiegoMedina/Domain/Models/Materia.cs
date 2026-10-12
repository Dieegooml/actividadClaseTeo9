using System;
using System.Collections.Generic;

namespace Lab05DiegoMedina.Models;

public partial class Materia
{
    public int IdMateria { get; set; }

    public int? IdCurso { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual Curso? IdCursoNavigation { get; set; }
}
