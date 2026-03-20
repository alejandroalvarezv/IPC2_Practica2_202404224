using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO; 

namespace Practica_2
{
    public partial class Form1 : Form
    {
        private GestionCola colaClinica = new GestionCola();
        
        private int tiempoAtencionAcumulado = 0;

        private TextBox txtNombre = null!;
        private NumericUpDown txtEdad = null!;
        private ComboBox cmbEspecialidad = null!;
        private Button btnRegistrar = null!;
        private Button btnAtender = null!;
        private Button btnVisualizar = null!;
        private ListBox lstColaTexto = null!;
        private Label lblResultado = null!;
        private Button btnSalir = null!;

        public Form1()
        {
            this.Text = "Sistema de Turnos Médicos - Clínica";
            this.Size = new Size(460, 620); 
            this.StartPosition = FormStartPosition.CenterScreen;

            InicializarComponentesManual();
        }

        private void InicializarComponentesManual()
        {
            // --- Registro de Pacientes ---
            Label lblNombre = new Label { Text = "Nombre del Paciente:", Location = new Point(20, 20), Width = 150 };
            txtNombre = new TextBox { Location = new Point(20, 45), Width = 200 };

            Label lblEdad = new Label { Text = "Edad:", Location = new Point(20, 80), Width = 150 };
            txtEdad = new NumericUpDown { Location = new Point(20, 105), Width = 100 };

            Label lblEsp = new Label { Text = "Especialidad:", Location = new Point(20, 140), Width = 150 };
            cmbEspecialidad = new ComboBox { Location = new Point(20, 165), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEspecialidad.Items.AddRange(new string[] { "Medicina General", "Pediatría", "Ginecología", "Dermatología" });

            // --- Botones de Acción ---
            btnRegistrar = new Button { Text = "Registrar Turno", Location = new Point(20, 210), Width = 190, Height = 40, BackColor = Color.LightGreen };
            btnRegistrar.Click += BtnRegistrar_Click;

            btnAtender = new Button { Text = "Atender Siguiente", Location = new Point(220, 210), Width = 190, Height = 40, BackColor = Color.LightSkyBlue };
            btnAtender.Click += BtnAtender_Click;

            btnVisualizar = new Button { Text = "Visualizar Cola", Location = new Point(20, 260), Width = 390, Height = 40, BackColor = Color.LightGoldenrodYellow };
            btnVisualizar.Click += BtnVisualizar_Click;

            // --- Visualización de Datos (Rúbrica: Ver Cola) ---
            Label lblLista = new Label { Text = "Cola de Turnos (Detalle):", Location = new Point(20, 310), Width = 200 };
            lstColaTexto = new ListBox { Location = new Point(20, 335), Width = 390, Height = 120, Font = new Font("Consolas", 9) };

            lblResultado = new Label { Text = "Esperando registros...", Location = new Point(20, 470), Width = 400, Height = 40, ForeColor = Color.Blue };

            // --- Botón Salir ---
            btnSalir = new Button { Text = "Salir", Location = new Point(165, 520), Width = 100, Height = 35, BackColor = Color.LightCoral };
            btnSalir.Click += (s, e) => Application.Exit();

            this.Controls.Add(lblNombre);
            this.Controls.Add(txtNombre);
            this.Controls.Add(lblEdad);
            this.Controls.Add(txtEdad);
            this.Controls.Add(lblEsp);
            this.Controls.Add(cmbEspecialidad);
            this.Controls.Add(btnRegistrar);
            this.Controls.Add(btnAtender);
            this.Controls.Add(btnVisualizar);
            this.Controls.Add(lblLista);
            this.Controls.Add(lstColaTexto);
            this.Controls.Add(lblResultado);
            this.Controls.Add(btnSalir);
        }

        private void BtnRegistrar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNombre.Text) || cmbEspecialidad.SelectedItem == null)
            {
                MessageBox.Show("Por favor complete todos los campos.");
                return;
            }

            Paciente nuevo = new Paciente(txtNombre.Text, (int)txtEdad.Value, cmbEspecialidad.SelectedItem.ToString()!);
            
            int tiempoEsperaEnFila = SumarTiemposEnCola(colaClinica.ObtenerInicio());
            int tiempoTotalEstimado = tiempoEsperaEnFila + nuevo.TiempoAtencion;

            colaClinica.Encolar(nuevo);

            lblResultado.Text = $"REGISTRADO: {nuevo.Nombre}\nEspera en fila al llegar: {tiempoEsperaEnFila} min";
            txtNombre.Clear();
            txtEdad.Value = 0;

            GenerarGrafica(); 
        }

        private void BtnAtender_Click(object? sender, EventArgs e)
        {
            Paciente? atendido = colaClinica.Desencolar();

            if (atendido != null)
            {
                int tAtencion = atendido.TiempoAtencion;
                int tEnCola = tiempoAtencionAcumulado; 
                int tTotal = tAtencion + tEnCola;

                string mensaje = $"ATENDIENDO A:\n" +
                                 $"Nombre: {atendido.Nombre}\n" +
                                 $"Especialidad: {atendido.Especialidad}\n" +
                                 $"---------------------------\n" +
                                 $"1. Tiempo Atención: {tAtencion} min\n" +
                                 $"2. Tiempo en Cola: {tEnCola} min\n" +
                                 $"3. Tiempo Total: {tTotal} min";

                MessageBox.Show(mensaje, "Atención Realizada");

                tiempoAtencionAcumulado += tAtencion;

                lblResultado.Text = $"Atendido: {atendido.Nombre}.";
                
                if (colaClinica.ObtenerInicio() == null)
                {
                    tiempoAtencionAcumulado = 0;
                }

                lstColaTexto.Items.Clear(); 
                GenerarGrafica(); 
            }
            else
            {
                MessageBox.Show("No hay turnos pendientes.");
                tiempoAtencionAcumulado = 0;
            }
        }

        private void BtnVisualizar_Click(object? sender, EventArgs e)
        {
            lstColaTexto.Items.Clear();
            Nodo? actual = colaClinica.ObtenerInicio();
            
            int esperaDesdeAhora = 0; 

            if (actual == null)
            {
                MessageBox.Show("La cola está vacía.");
                return;
            }

            while (actual != null)
            {
                int tAtencion = actual.Dato.TiempoAtencion;
                int tTotal = esperaDesdeAhora + tAtencion;

                string info = $"{actual.Dato.Nombre} | Atenc: {tAtencion}m | Espera: {esperaDesdeAhora}m | Total: {tTotal}m";
                lstColaTexto.Items.Add(info);

                esperaDesdeAhora += tAtencion;
                actual = actual.Siguiente;
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
                    dot += $"  nodo{i} [label=\"{{ {actual.Dato.Nombre} | {actual.Dato.Especialidad} | {actual.Dato.TiempoAtencion} min }}\"];\n";
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
            } 
            catch { }
        }
    }
}