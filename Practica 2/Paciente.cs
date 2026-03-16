namespace Practica_2
{
    public class Paciente
    {
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public string Especialidad { get; set; }
        public int TiempoAtencion { get; set; }

        public Paciente(string nombre, int edad, string especialidad)
        {
            Nombre = nombre;
            Edad = edad;
            Especialidad = especialidad;

            // Tiempos de atención según el enunciado
            if (especialidad == "Medicina General") TiempoAtencion = 10;
            else if (especialidad == "Pediatría") TiempoAtencion = 15;
            else if (especialidad == "Ginecología") TiempoAtencion = 20;
            else if (especialidad == "Dermatología") TiempoAtencion = 25;
            else TiempoAtencion = 0;
        }
    }
}