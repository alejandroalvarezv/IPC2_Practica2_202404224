namespace Practica_2
{
    public class Nodo
    {
        public Paciente Dato { get; set; }
        public Nodo? Siguiente { get; set; }

        public Nodo(Paciente paciente)
        {
            Dato = paciente;
            Siguiente = null;
        }
    }

    public class GestionCola
    {
        private Nodo? inicio;
        private Nodo? fin;

        public void Encolar(Paciente nuevoPaciente)
        {
            Nodo nuevoNodo = new Nodo(nuevoPaciente);
            if (inicio == null)
            {
                inicio = nuevoNodo;
                fin = nuevoNodo;
            }
            else
            {
                fin!.Siguiente = nuevoNodo;
                fin = nuevoNodo;
            }
        }

        public Paciente? Desencolar()
        {
            if (inicio == null) return null;

            Paciente atendido = inicio.Dato;
            inicio = inicio.Siguiente;
            if (inicio == null) fin = null;

            return atendido;
        }

        public Nodo? ObtenerInicio() => inicio;
    }
}