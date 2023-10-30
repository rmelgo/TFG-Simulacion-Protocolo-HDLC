using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Simulador_HDLC
{
    /// <summary>
    /// Lógica de interacción para VentanaVisualizacionTramaInformación.xaml
    /// </summary>
    public partial class VentanaVisualizacionTramaInformación : Window
    {
        public String direccionEstacion { get; set; }

        public String infoBitCommandRequest { get; set; }

        public int infoBitSondeo { get; set; }

        public int numeroSecuencia { get; set; }

        public int numeroTramaEsperada { get; set; }

        private String nombreEstacionActual;

        public VentanaVisualizacionTramaInformación(String tipoTrama, String direccionEstacion, String infoBitCommandRequest, int infoBitSondeo, int numeroSecuencia, int numeroTramaEsperada, String nombreEstacionActual)
        {
            InitializeComponent();
            mostrarValores(tipoTrama, direccionEstacion, infoBitCommandRequest, infoBitSondeo, numeroSecuencia, numeroTramaEsperada);
            cargarValores(tipoTrama, direccionEstacion, infoBitCommandRequest, infoBitSondeo);
            this.nombreEstacionActual = nombreEstacionActual;
        }

        private void mostrarValores(String tipoTrama, String direccionEstacion, String infoBitCommandRequest, int infoBitSondeo, int numeroSecuencia, int numeroTramaEsperada)
        {
            CajaTipoTrama.Text = tipoTrama;
            CajaDireccionEstacion.Text = direccionEstacion;
            CajaBitCommandRequest.Text = infoBitCommandRequest;
            CajaNumeroSecuencia.Text = (numeroSecuencia + 1).ToString();
            CajaBitSondeo.Text = infoBitSondeo.ToString();
            CajaNumeroTramaEsperada.Text = numeroTramaEsperada.ToString();

            CajaBitCommandRequest.Items.Add("C");
            CajaBitCommandRequest.Items.Add("R");

            CajaBitSondeo.Items.Add(0);
            CajaBitSondeo.Items.Add(1);

            for (int i = 0; i < 8; i++) //cambiar el 8 por un parametro para que quede mejor
            {
                CajaNumeroSecuencia.Items.Add(i);
            }

            for (int i = 0; i < 8; i++) //cambiar el 8 por un parametro para que quede mejor
            {
                CajaNumeroTramaEsperada.Items.Add(i);
            }
        }

        public void cargarValores(String tipoTrama, String direccionEstacion, String infoBitCommandRequest, int infoBitSondeo)
        {
            this.direccionEstacion = direccionEstacion;
            this.infoBitCommandRequest = infoBitCommandRequest;
            this.infoBitSondeo = infoBitSondeo;
            this.numeroSecuencia = numeroSecuencia; //o numeroSecuencia + 1????
            this.numeroTramaEsperada = numeroTramaEsperada;
        }

        private void BotonEnviar_Click(object sender, RoutedEventArgs e)
        {
            direccionEstacion = CajaDireccionEstacion.Text;
            infoBitCommandRequest = CajaBitCommandRequest.Text;
            infoBitSondeo = Int32.Parse(CajaBitSondeo.Text);
            numeroSecuencia = Int32.Parse(CajaNumeroSecuencia.Text);
            numeroTramaEsperada = Int32.Parse(CajaNumeroTramaEsperada.Text);

            DialogResult = true;
        }

        private void BotonCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CajaBitCommandRequest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0].ToString() == "R")
            {
                CajaDireccionEstacion.Text = nombreEstacionActual;
            }
            else
            {
                CajaDireccionEstacion.Text = direccionEstacion;
            }
        }
    }
}
