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
    /// Lógica de interacción para VentanaAdvertencia.xaml
    /// </summary>
    public partial class VentanaAdvertencia : Window
    {
        public VentanaAdvertencia(String mensaje, bool advertencia) //Constructor que recibe como parámetros el mensaje a mostrar en la ventana de advertencia y un booleano para indicar si el mensaje es una advertencia o un mensaje informativo
        {
            InitializeComponent();
            cargarVentanaAdvertencia(mensaje, advertencia);

            //Se ajusta la anchura y altura mínima y máxima al valor actual de la ventana para evitar el redimensinado de la ventana por parte del usuario
            this.MinWidth = this.MaxWidth = this.Width;
            this.MinHeight = this.MaxHeight = this.Height;
        }

        private void cargarVentanaAdvertencia(string mensaje, bool advertencia)
        {
            Texto.Text = mensaje; //Se "rellena" el cuerpo de la ventana de advertencia con el mensaje pasado como parámetro
            if (!advertencia) //Si el mensaje no es de tipo advertencia, se carga una imagen con el icono de información
            {
                Image imagen = new Image();

                BitmapImage imagenBitmap = new BitmapImage(new Uri("info.png", UriKind.Relative));

                ImagenIcono.Source = imagenBitmap;
            }
            else //Si el mensaje es de tipo advertencia, se carga una imagen con el icono de advertencia
            {
                Image imagen = new Image();

                BitmapImage imagenBitmap = new BitmapImage(new Uri("advertencia.png", UriKind.Relative));

                ImagenIcono.Source = imagenBitmap;
            }
        }

        private void BotonAceptar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; //Cuando el usuario pulse el botón de aceptar, se cerrara la instancia de la ventana de advertencia devolviendo un valor true
        }
    }
}
