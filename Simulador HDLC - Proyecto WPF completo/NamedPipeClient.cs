using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simulador_HDLC
{
    public class NamedPipeClient
    {
        private NamedPipeClientStream pipeClient;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public NamedPipeClient(string pipeName) //Constructor que recibe como parámetro el nombre de la tubería cliente a crear y que crea una instancia de una tubería cliente de entrada y salida con el nombre recibido como parámetro
        {
            pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
        }

        public async Task<bool> ConectarCliente(int tiempo_espera) //Función que permite conectar la tubería cliente a una tubería servidor (recibe como parámetro el tiempo máximo que se intentará realizar dicha conexión)
        {
            try
            {
                await pipeClient.ConnectAsync(tiempo_espera); //Se utiliza el método ConnectAsync() de la tuberia cliente para intentar realizar una conexión con una tuberia servidor (se le pasa como parámetro el tiempo límite para conseguir dicha conexión)
                if (pipeClient.IsConnected) //Si se consigue establecer una conexión con una tubería servidor, se devuelve un valor verdadero ya que la conexión fue exitosa
                {
                    Console.WriteLine("Conexión establecida correctamente");
                    return true;
                }
                else //Si expira el tiempo máximo y no se consiguió establecer una conexión con una tubería servidor, se devuelve un valor falso ya que no se pudo realizar la conexión
                {
                    Console.WriteLine("No se pudo establecer la conexión");
                    return false;
                }
            }
            catch (Exception ex) //Si la llamada a ConnectAsync() falla, se devuelve un valor falso ya que no se pudo realizar la conexión
            {
                Console.WriteLine("Error al establecer la conexión: {0}", ex.Message);
                return false;
            }         
        }

        public void Close() //Función que cierra la tubería cliente asociada y libera los recursos correspondientes
        {
            pipeClient.Close(); //Se utiliza el método Close() para cerrar la tubería cliente
        }

        public NamedPipeClientStream Tuberia //Propiedad que permite obtener y definir el valor de la instancia de la tubería cliente
        {
            get
            {
                return pipeClient;
            }
            set
            {
                pipeClient = value;
            }
        }
    }
}
