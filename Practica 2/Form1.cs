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

        private PictureBox picGrafica = null!; 

        public Form1()
        {
            this.Text = "Sistema de Turnos";
            this.Size = new Size(850, 650); 
            this.StartPosition = FormStartPosition.CenterScreen;
            InicializarComponentesManual();
        }

        private void InicializarComponentesManual()
        {
            Panel pnlEntrada = new Panel { Location = new Point(10, 10), Size = new Size(410, 600), BorderStyle = BorderStyle.FixedSingle };
            Panel pnlGrafica = new Panel { Location = new Point(430, 10), Size = new Size(390, 600), BorderStyle = BorderStyle.FixedSingle };

            Label lblNombre = new Label { Text = "Paciente:", Location = new Point(15, 15), Width = 150 };
            txtNombre = new TextBox { Location = new Point(15, 40), Width = 200 };

            Label lblEdad = new Label { Text = "Edad:", Location = new Point(15, 75), Width = 150 };
            txtEdad = new NumericUpDown { Location = new Point(15, 100), Width = 100 };

            Label lblEsp = new Label { Text = "Especialidad:", Location = new Point(15, 135), Width = 150 };
            cmbEspecialidad = new ComboBox { Location = new Point(15, 160), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEspecialidad.Items.AddRange(new string[] { "Medicina General", "Pediatría", "Ginecología", "Dermatología" });

            btnRegistrar = new Button { Text = "Registrar Turno", Location = new Point(15, 205), Width = 180, Height = 40, BackColor = Color.LightGreen };
            btnRegistrar.Click += BtnRegistrar_Click;

            btnAtender = new Button { Text = "Atender Siguiente", Location = new Point(205, 205), Width = 180, Height = 40, BackColor = Color.LightSkyBlue };
            btnAtender.Click += BtnAtender_Click;

            btnVisualizar = new Button { Text = "Visualizar Lista de Texto", Location = new Point(15, 255), Width = 370, Height = 35, BackColor = Color.LightGoldenrodYellow };
            btnVisualizar.Click += BtnVisualizar_Click;

            Label lblLista = new Label { Text = "Cola de Turnos (Detalle de Texto):", Location = new Point(15, 300), Width = 250 };
            lstColaTexto = new ListBox { Location = new Point(15, 325), Width = 370, Height = 150, Font = new Font("Consolas", 9) };
            
            lblResultado = new Label { Text = "Esperando registros...", Location = new Point(15, 485), Width = 370, Height = 40, ForeColor = Color.Blue };

            btnSalir = new Button { Text = "Salir del Sistema", Location = new Point(115, 540), Width = 180, Height = 40, BackColor = Color.LightCoral };
            btnSalir.Click += (s, e) => Application.Exit();

            pnlEntrada.Controls.AddRange(new Control[] { lblNombre, txtNombre, lblEdad, txtEdad, lblEsp, cmbEspecialidad, btnRegistrar, btnAtender, btnVisualizar, lblLista, lstColaTexto, lblResultado, btnSalir });

            Label lblTituloGrafica = new Label { Text = "Estado de la Cola (Graphviz):", Location = new Point(15, 15), Width = 250, Font = new Font("Arial", 10, FontStyle.Bold) };
            
            picGrafica = new PictureBox { 
                Location = new Point(15, 40), 
                Size = new Size(360, 540), 
                BorderStyle = BorderStyle.Fixed3D,
                SizeMode = PictureBoxSizeMode.Zoom 
            };

            pnlGrafica.Controls.AddRange(new Control[] { lblTituloGrafica, picGrafica });

            this.Controls.Add(pnlEntrada);
            this.Controls.Add(pnlGrafica);
        }

        private void BtnRegistrar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNombre.Text) || cmbEspecialidad.SelectedItem == null)
            {
                MessageBox.Show("Por favor complete todos los campos.");
                return;
            }
            Paciente nuevo = new Paciente(txtNombre.Text, (int)txtEdad.Value, cmbEspecialidad.SelectedItem.ToString()!);
            colaClinica.Encolar(nuevo);
            lblResultado.Text = $"REGISTRADO: {nuevo.Nombre}";
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

                string mensaje = $"ATENDIENDO A:\nNombre: {atendido.Nombre}\n" +
                                 $"---------------------------\n" +
                                 $"1. Tiempo Atención: {tAtencion} min\n" +
                                 $"2. Tiempo en Cola: {tEnCola} min\n" +
                                 $"3. Tiempo Total: {tAtencion + tEnCola} min";

                MessageBox.Show(mensaje, "Atención Realizada");
                tiempoAtencionAcumulado += tAtencion;
                if (colaClinica.ObtenerInicio() == null) tiempoAtencionAcumulado = 0;
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
            int esperaCorriente = tiempoAtencionAcumulado; 
            if (actual == null) { MessageBox.Show("La cola está vacía."); return; }

            while (actual != null)
            {
                int tAtencion = actual.Dato.TiempoAtencion;
                string info = $"{actual.Dato.Nombre} | Atenc: {tAtencion}m | Espera: {esperaCorriente}m | Total: {esperaCorriente + tAtencion}m";
                lstColaTexto.Items.Add(info);
                esperaCorriente += tAtencion;
                actual = actual.Siguiente;
            }
        }

        private void ActualizarImagenEnPantalla()
        {
            try 
            {
                if (File.Exists("cola.png"))
                {

                    using (FileStream stream = new FileStream("cola.png", FileMode.Open, FileAccess.Read))
                    {
                        if (picGrafica.Image != null) picGrafica.Image.Dispose();
                        
                        picGrafica.Image = Image.FromStream(stream);
                    }
                }
            } 
            catch {
            }
        }

        private void GenerarGrafica()
        {
            string dot = "digraph G {\n";
            dot += "  rankdir=LR;\n"; 
            dot += "  node [shape=record, style=filled, fillcolor=lightblue, fontname=\"Arial\", fontsize=10];\n";

            Nodo? actual = colaClinica.ObtenerInicio();
            int i = 0;
            int esperaEnImagen = tiempoAtencionAcumulado; 

            if (actual == null)
            {
                dot += "  n_vacia [label=\"COLA VACÍA\", fillcolor=lightcoral];\n";
            }
            else
            {
                while (actual != null)
                {
                    int tAtencion = actual.Dato.TiempoAtencion;
                    dot += $"  nodo{i} [label=\"{{ {actual.Dato.Nombre} | ATENC: {tAtencion}m\\nESPERA: {esperaEnImagen}m\\nTOTAL: {esperaEnImagen + tAtencion}m }}\"];\n";
                    
                    if (actual.Siguiente != null)
                    {
                        dot += $"  nodo{i} -> nodo{i + 1};\n";
                    }

                    esperaEnImagen += tAtencion;
                    actual = actual.Siguiente;
                    i++;
                }
            }
            dot += "}";

            try 
            {
                File.WriteAllText("cola.dot", dot);
                var startInfo = new System.Diagnostics.ProcessStartInfo {
                    FileName = "dot", Arguments = "-Tpng cola.dot -o cola.png",
                    UseShellExecute = false, CreateNoWindow = true
                };
                System.Diagnostics.Process.Start(startInfo)?.WaitForExit();

                ActualizarImagenEnPantalla();
            } 
            catch { }
        }
    }
}