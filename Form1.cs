using System;
using System.Collections.Generic;
using System.ComponentModel; // BindingList
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ReservaCine
{
    // Modelo mostrado en el grid
    public class Reserva
    {
        public string Nombre { get; set; }
        public string DUI { get; set; }
        public string Categoria { get; set; }
        public string Pelicula { get; set; }
        public int Cantidad { get; set; }
    }

    public class Form1 : Form
    {
       
        private TextBox txtNombre;
        private MaskedTextBox mtxDUI;
        private ComboBox cmbCategoria;
        private ComboBox cmbPelicula;
        private NumericUpDown nudCantidad;
        private Button btnAgregar;
        private DataGridView dgvReservas;
        private TableLayoutPanel barra;

        
        private readonly BindingList<Reserva> _reservas = new BindingList<Reserva>();

        
        private readonly Dictionary<string, List<string>> pelisPorCategoria =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Accion",   new List<string> { "Mision Explosiva", "Velocidad Maxima", "Fuerza X" } },
                { "Comedia",  new List<string> { "Risas 24/7", "Vecinos Locos", "Clase Turista" } },
                { "Drama",    new List<string> { "Lagrimas del Ayer", "La Decision", "Sombras" } },
                { "Infantil", new List<string> { "Aventuras Mini", "Zoo Party", "Robotitos" } }
            };

        public Form1()
        {
            Text = "Reserva de boletos de cine";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1000, 600);
            MinimumSize = new Size(900, 520);

            ConstruirUI();
            ConectarEventos();
            CargarInicial();
        }

        private void ConstruirUI()
        {
            
            barra = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 110,
                ColumnCount = 12,
                RowCount = 2,
                Padding = new Padding(12),
                AutoSize = false
            };
            for (int i = 0; i < 12; i++)
                barra.ColumnStyles.Add(new ColumnStyle(i % 2 == 0 ? SizeType.AutoSize : SizeType.Percent, i % 2 == 0 ? 0 : 16.6f));
            barra.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            barra.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lblNombre = new Label { Text = "Nombre:", AutoSize = true, Anchor = AnchorStyles.Left };
            txtNombre = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Width = 220 };

            var lblDui = new Label { Text = "DUI:", AutoSize = true, Anchor = AnchorStyles.Left };
            mtxDUI = new MaskedTextBox { Anchor = AnchorStyles.Left, Width = 120 };

            var lblCat = new Label { Text = "Categoria:", AutoSize = true, Anchor = AnchorStyles.Left };
            cmbCategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left, Width = 180 };

            var lblPeli = new Label { Text = "Pelicula:", AutoSize = true, Anchor = AnchorStyles.Left };
            cmbPelicula = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left, Width = 220 };

            var lblCant = new Label { Text = "Cantidad:", AutoSize = true, Anchor = AnchorStyles.Left };
            nudCantidad = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 1, Anchor = AnchorStyles.Left, Width = 80 };

            btnAgregar = new Button { Text = "Agregar", AutoSize = true, Anchor = AnchorStyles.Left };

            barra.Controls.Add(lblNombre, 0, 0);
            barra.Controls.Add(txtNombre, 1, 0);
            barra.Controls.Add(lblDui, 2, 0);
            barra.Controls.Add(mtxDUI, 3, 0);
            barra.Controls.Add(lblCat, 4, 0);
            barra.Controls.Add(cmbCategoria, 5, 0);
            barra.Controls.Add(lblPeli, 6, 0);
            barra.Controls.Add(cmbPelicula, 7, 0);
            barra.Controls.Add(lblCant, 8, 0);
            barra.Controls.Add(nudCantidad, 9, 0);
            barra.Controls.Add(btnAgregar, 11, 0);

            Controls.Add(barra);

            
            dgvReservas = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoGenerateColumns = true,     
                ColumnHeadersVisible = true,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Enlace
            dgvReservas.DataSource = _reservas;
            Controls.Add(dgvReservas);
            dgvReservas.BringToFront(); // por si algo lo tapa
        }

        private void ConectarEventos()
        {
            Load += (s, e) => txtNombre.Focus();

            txtNombre.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
                    e.Handled = true;
            };

            txtNombre.TextChanged += (s, e) => ActualizarHabilitadoAgregar();
            mtxDUI.TextChanged += (s, e) => ActualizarHabilitadoAgregar();
            nudCantidad.ValueChanged += (s, e) => ActualizarHabilitadoAgregar();

            cmbCategoria.SelectedIndexChanged += (s, e) =>
            {
                CargarPeliculas();
                ActualizarHabilitadoAgregar();
            };
            cmbPelicula.SelectedIndexChanged += (s, e) => ActualizarHabilitadoAgregar();

            btnAgregar.Click += (s, e) => AgregarReserva();
        }

        private void CargarInicial()
        {
            mtxDUI.Mask = "00000000-0"; // DUI ES
            cmbCategoria.Items.AddRange(pelisPorCategoria.Keys.ToArray());
            ActualizarHabilitadoAgregar();
        }

        private void CargarPeliculas()
        {
            cmbPelicula.Items.Clear();
            cmbPelicula.SelectedIndex = -1;

            if (cmbCategoria.SelectedIndex >= 0)
            {
                string cat = Convert.ToString(cmbCategoria.SelectedItem);
                if (!string.IsNullOrEmpty(cat) && pelisPorCategoria.ContainsKey(cat))
                    cmbPelicula.Items.AddRange(pelisPorCategoria[cat].ToArray());
            }
        }

        private void AgregarReserva()
        {
            if (!FormularioValido())
            {
                MessageBox.Show("Completa todos los campos.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _reservas.Add(new Reserva
            {
                Nombre = txtNombre.Text.Trim(),
                DUI = mtxDUI.Text,
                Categoria = Convert.ToString(cmbCategoria.SelectedItem) ?? string.Empty,
                Pelicula = Convert.ToString(cmbPelicula.SelectedItem) ?? string.Empty,
                Cantidad = (int)nudCantidad.Value
            });

            
            txtNombre.Clear();
            mtxDUI.Clear();
            cmbCategoria.SelectedIndex = -1;
            cmbPelicula.Items.Clear();
            cmbPelicula.SelectedIndex = -1;
            nudCantidad.Value = 1;
            txtNombre.Focus();

            ActualizarHabilitadoAgregar();
        }

        private bool FormularioValido()
        {
            bool nombreOk = !string.IsNullOrWhiteSpace(txtNombre.Text);
            bool duiOk = mtxDUI.MaskCompleted;
            bool catOk = cmbCategoria.SelectedIndex >= 0;
            bool peliOk = cmbPelicula.SelectedIndex >= 0;
            bool cantOk = nudCantidad.Value > 0;
            return nombreOk && duiOk && catOk && peliOk && cantOk;
        }

        private void ActualizarHabilitadoAgregar()
        {
            btnAgregar.Enabled = FormularioValido();
        }
    }
}
