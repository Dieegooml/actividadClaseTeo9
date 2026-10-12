using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Lab05DiegoMedina.Models;

public partial class ColegioDBContext : DbContext
{
    public ColegioDBContext()
    {
    }

    public ColegioDBContext(DbContextOptions<ColegioDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asistencia> Asistencias { get; set; }

    public virtual DbSet<Curso> Cursos { get; set; }

    public virtual DbSet<Estudiante> Estudiantes { get; set; }

    public virtual DbSet<Evaluacione> Evaluaciones { get; set; }

    public virtual DbSet<Materia> Materias { get; set; }

    public virtual DbSet<Matricula> Matriculas { get; set; }

    public virtual DbSet<Profesore> Profesores { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    // La cadena de conexión llega por IConfiguration (appsettings.json), por eso el
    // DbContext se genera con el scaffolding usando --no-onconfiguring.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.HasKey(e => e.IdAsistencia).HasName("PRIMARY");

            entity.HasIndex(e => e.IdCurso, "id_curso");

            entity.HasIndex(e => e.IdEstudiante, "id_estudiante");

            entity.Property(e => e.IdAsistencia).HasColumnName("id_asistencia");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdCurso).HasColumnName("id_curso");
            entity.Property(e => e.IdEstudiante).HasColumnName("id_estudiante");

            entity.HasOne(d => d.IdCursoNavigation).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.IdCurso)
                .HasConstraintName("asistencias_ibfk_2");

            entity.HasOne(d => d.IdEstudianteNavigation).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.IdEstudiante)
                .HasConstraintName("asistencias_ibfk_1");
        });

        modelBuilder.Entity<Curso>(entity =>
        {
            entity.HasKey(e => e.IdCurso).HasName("PRIMARY");

            entity.Property(e => e.IdCurso).HasColumnName("id_curso");
            entity.Property(e => e.Creditos).HasColumnName("creditos");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Estudiante>(entity =>
        {
            entity.HasKey(e => e.IdEstudiante).HasName("PRIMARY");

            entity.Property(e => e.IdEstudiante).HasColumnName("id_estudiante");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .HasColumnName("direccion");
            entity.Property(e => e.Edad).HasColumnName("edad");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Evaluacione>(entity =>
        {
            entity.HasKey(e => e.IdEvaluacion).HasName("PRIMARY");

            entity.HasIndex(e => e.IdCurso, "id_curso");

            entity.HasIndex(e => e.IdEstudiante, "id_estudiante");

            entity.Property(e => e.IdEvaluacion).HasColumnName("id_evaluacion");
            entity.Property(e => e.Calificacion)
                .HasPrecision(5, 2)
                .HasColumnName("calificacion");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdCurso).HasColumnName("id_curso");
            entity.Property(e => e.IdEstudiante).HasColumnName("id_estudiante");

            entity.HasOne(d => d.IdCursoNavigation).WithMany(p => p.Evaluaciones)
                .HasForeignKey(d => d.IdCurso)
                .HasConstraintName("evaluaciones_ibfk_2");

            entity.HasOne(d => d.IdEstudianteNavigation).WithMany(p => p.Evaluaciones)
                .HasForeignKey(d => d.IdEstudiante)
                .HasConstraintName("evaluaciones_ibfk_1");
        });

        modelBuilder.Entity<Materia>(entity =>
        {
            entity.HasKey(e => e.IdMateria).HasName("PRIMARY");

            entity.HasIndex(e => e.IdCurso, "id_curso");

            entity.Property(e => e.IdMateria).HasColumnName("id_materia");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.IdCurso).HasColumnName("id_curso");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdCursoNavigation).WithMany(p => p.Materia)
                .HasForeignKey(d => d.IdCurso)
                .HasConstraintName("materias_ibfk_1");
        });

        modelBuilder.Entity<Matricula>(entity =>
        {
            entity.HasKey(e => e.IdMatricula).HasName("PRIMARY");

            entity.HasIndex(e => e.IdCurso, "id_curso");

            entity.HasIndex(e => e.IdEstudiante, "id_estudiante");

            entity.Property(e => e.IdMatricula).HasColumnName("id_matricula");
            entity.Property(e => e.IdCurso).HasColumnName("id_curso");
            entity.Property(e => e.IdEstudiante).HasColumnName("id_estudiante");
            entity.Property(e => e.Semestre)
                .HasMaxLength(20)
                .HasColumnName("semestre");

            entity.HasOne(d => d.IdCursoNavigation).WithMany(p => p.Matriculas)
                .HasForeignKey(d => d.IdCurso)
                .HasConstraintName("matriculas_ibfk_2");

            entity.HasOne(d => d.IdEstudianteNavigation).WithMany(p => p.Matriculas)
                .HasForeignKey(d => d.IdEstudiante)
                .HasConstraintName("matriculas_ibfk_1");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.HasIndex(e => e.UserName, "ix_usuarios_user_name").IsUnique();
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");
            entity.Property(e => e.CreadoEn)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasColumnName("creado_en");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Rol)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValueSql("'Estudiante'")
                .HasColumnName("rol");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .HasColumnName("user_name");
        });

        modelBuilder.Entity<Profesore>(entity =>
        {
            entity.HasKey(e => e.IdProfesor).HasName("PRIMARY");

            entity.Property(e => e.IdProfesor).HasColumnName("id_profesor");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");
            entity.Property(e => e.Especialidad)
                .HasMaxLength(100)
                .HasColumnName("especialidad");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
