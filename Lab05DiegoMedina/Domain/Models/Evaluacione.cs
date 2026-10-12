using System;
using System.Collections.Generic;

namespace Lab05DiegoMedina.Models;

public partial class Evaluacione
{
    public int IdEvaluacion { get; set; }

    public int? IdEstudiante { get; set; }

    public int? IdCurso { get; set; }

    public decimal? Calificacion { get; set; }

    public DateOnly? Fecha { get; set; }

    public virtual Curso? IdCursoNavigation { get; set; }

    public virtual Estudiante? IdEstudianteNavigation { get; set; }
}
