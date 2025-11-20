namespace SistemaParamedicos.API.DTOs
{
    public class EmpleadoDTO
    {
        public string IdEmpleado { get; set; }
        public string Rfid { get; set; }
        public string Nombre { get; set; }
        public string Sexo { get; set; }
        public string Telefono { get; set; }
        public string Alergias { get; set; }
        public string TipoSangre { get; set; }
        public string IdPuesto { get; set; }
        public string IdDepartamento { get; set; }
        public string IdArea { get; set; }
        public DateTime? Nacimiento { get; set; }
        public string Foto { get; set; }
        public string Estado { get; set; }

        // ⭐ Solo incluir el objeto Puesto (sin la colección de empleados)
        public PuestoDTO Puesto { get; set; }
    }
}