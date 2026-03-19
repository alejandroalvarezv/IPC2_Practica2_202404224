using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO; 

namespace Practica_2
{
    public partial class Form1 : Form
    {
        private GestionCola colaClinica = new GestionCola();
        
        private TextBox txtNombre = null!;
        private NumericUpDown txtEdad = null!;
        private ComboBox cmbEspecialidad = null!;
        private Button btnRegistrar = null!;
        private Button btnAtender = null!;
        private Label lblResultado = null!;

        public Form1()
        {
            this.Text = "Sistema de Turnos Médicos - Clínica";
            this.Size = new Size(450, 550); // Aumenté un poco el alto
            this.StartPosition = FormStartPosition.CenterScreen;

            InicializarComponentesManual();
        }

        private void InicializarComponentesManual()
        {
            Label lblNombre = new Label { Text = "Nombre del Paciente:", Location = new Point(20, 20), Width = 150 };
            txtNombre = new TextBox { Location = new Point(20, 45), Width = 200 };

            Label lblEdad = new Label { Text = "Edad:", Location = new Point(20, 80), Width = 150 };
            txtEdad = new NumericUpDown { Location = new Point(20, 105), Width = 100 };

            Label lblEsp = new Label { Text = "Especialidad:", Location = new Point(20, 140), Width = 150 };
            cmbEspecialidad = new ComboBox { Location = new Point(20, 165), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEspecialidad.Items.AddRange(new string[] { "Medicina General", "Pediatría", "Ginecología", "Dermatología" });

            btnRegistrar = new Button { Text = "Registrar Turno", Location = new Point(20, 210), Width = 200, Height = 40 };
            btnRegistrar.Click += BtnRegistrar_Click;

            btnAtender = new Button { Text = "Atender Siguiente", Location = new Point(230, 210), Width = 180, Height = 40 };
            btnAtender.Click += BtnAtender_Click;

            lblResultado = new Label { Text = "Esperando registros...", Location = new Point(20, 270), Width = 400, Height = 100, ForeColor = Color.Blue };

            this.Controls.Add(lblNombre);
            this.Controls.Add(txtNombre);
            this.Controls.Add(lblEdad);
            this.Controls.Add(txtEdad);
            this.Controls.Add(lblEsp);
            this.Controls.Add(cmbEspecialidad);
            this.Controls.Add(btnRegistrar);
            this.Controls.Add(btnAtender);
            this.Controls.Add(lblResultado);
        }

        private void BtnRegistrar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNombre.Text) || cmbEspecialidad.SelectedItem == null)
            {
                MessageBox.Show("Por favor complete todos los campos.");
                return;
            }

            string nombre = txtNombre.Text;
            int edad = (int)txtEdad.Value;
            string especialidad = cmbEspecialidad.SelectedItem.ToString() ?? "";

            Paciente nuevo = new Paciente(nombre, edad, especialidad);
            
            int tiempoEsperaPrevio = SumarTiemposEnCola(colaClinica.ObtenerInicio());
            int tiempoTotalEstimado = tiempoEsperaPrevio + nuevo.TiempoAtencion;

            colaClinica.Encolar(nuevo);

            lblResultado.Text = $"REGISTRADO:\nPaciente: {nombre}\nAtención: {nuevo.TiempoAtencion} min\nEspera Total: {tiempoTotalEstimado} min";
            txtNombre.Clear();

            GenerarGrafica(); 
        }

        private void BtnAtender_Click(object? sender, EventArgs e)
        {
            Paciente? atendido = colaClinica.Desencolar();

            if (atendido != null)
            {
                MessageBox.Show($"Atendiendo a: {atendido.Nombre}\nEspecialidad: {atendido.Especialidad}");
                lblResultado.Text = "Paciente atendido con éxito.";
                GenerarGrafica(); 
            }
            else
            {
                MessageBox.Show("No hay turnos pendientes.");
            }
        }

        private int SumarTiemposEnCola(Nodo? nodo)
        {
            int suma = 0;
            while (nodo != null)
            {
                suma += nodo.Dato.TiempoAtencion;
                nodo = nodo.Siguiente;
            }
            return suma;
        }

        private void GenerarGrafica()
        {
            string dot = "digraph G {\n";
            dot += "  rankdir=LR;\n"; 
            dot += "  node [shape=record, style=filled, fillcolor=lightblue];\n";

            Nodo? actual = colaClinica.ObtenerInicio();
            int i = 0;

            if (actual == null)
            {
                dot += "  n_vacia [label=\"COLA VACÍA\"];\n";
            }
            else
            {
                while (actual != null)
                {
                    dot += $"  nodo{i} [label=\"{{ Nombre: {actual.Dato.Nombre} | {actual.Dato.Especialidad} | {actual.Dato.TiempoAtencion} min }}\"];\n";
                    if (actual.Siguiente != null)
                    {
                        dot += $"  nodo{i} -> nodo{i + 1};\n";
                    }
                    actual = actual.Siguiente;
                    i++;
                }
            }

            dot += "}";

            try 
            {
                File.WriteAllText("cola.dot", dot);
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dot",
                    Arguments = "-Tpng cola.dot -o cola.png",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                System.Diagnostics.Process.Start(startInfo)?.WaitForExit();
                
                if (File.Exists("cola.png"))
                {
                    Console.WriteLine("Gráfica generada en la carpeta del proyecto.");
                }
            } 
            catch (Exception ex) 
            {
                MessageBox.Show("Instala Graphviz y agrégalo al PATH para ver la gráfica: " + ex.Message);
            }
        }
    }
}