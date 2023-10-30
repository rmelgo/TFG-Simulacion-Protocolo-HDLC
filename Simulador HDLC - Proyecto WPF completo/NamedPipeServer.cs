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
    public class NamedPipeServer
    {
        private NamedPipeServerStream pipeServer;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private static Mutex mutex = new Mutex();

        public NamedPipeServer(string pipeName) //Constructor que recibe como parámetro el nombre de la tubería servidor a crear y que crea una instancia de una tubería servidor de entrada y salida con el nombre recibido como parámetro
        {
            try
            {
                pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut);

            } catch (Exception ex) //Si en la creación de la tubería servidor, existe algún tipo de error, se captura la excepción para que no se detenga el simulador
            {
                Console.WriteLine("ERROR -> {0}", ex);
            }
        }

        public void ConectarServidor() //Función que permite a la tubería servidor esperar la conexión con una tubería cliente
        {
            pipeServer.WaitForConnection(); //Se utiliza el método WaitForConnection() el cual espera bloqueado a recibir una conexión por parte de una tubería cliente
        }
       
        public async void EscucharTuberia() //Función que permite a la tuberia servidor recibir mensajes y mantenerse a la espera de mensajes
        {
            try
            {
                StreamReader sr = new StreamReader(pipeServer);
                string temp = await sr.ReadLineAsync(); //Se utiliza el método ReadLineAsync() de manera asíncrona el cual permite esperar a recibir mensajes por la tubería sin bloquear el proceso principal
                if (temp != null) //Si el mensaje recibido no es nulo
                {
                    mutex.WaitOne(); //Se protege la recepción de mensajes con una sección critica. El objetivo es que la recepción de mensajes sea atómica
                    OnMessageReceived(new MessageReceivedEventArgs(temp)); //Se dispara el evento MessageReceived. Con este evento, desde la estación principal se puede manejar de manera adecuada la recepción de mensajes y actuar de una manera u otra en función del mensaje recibido
                    //Este evento tendrá como parámetro MessageReceivedEventArgs que es una clase que hereda de EventArgs y tiene un atributo en el cual se almacenará el contenido del mensaje para que pueda ser transmitido a la funcón manejadora del evento
                    mutex.ReleaseMutex();
                }
            }
            catch (Exception) //Si en la recepción de mensajes en la tubería servidor, existe algún tipo de error, se captura la excepción para que no se detenga el simulador
            {
                Console.WriteLine("ERROR -> Canalizacion cerrada (NamedPipeServer)");
            }
        }

        public void Close() //Función que cierra la tubería servidor asociada y libera los recursos correspondientes
        {
            pipeServer.Close(); //Se utiliza el método Close() para cerrar la tubería servidor
        }

        public NamedPipeServerStream Tuberia //Propiedad que permite obtener y definir el valor de la instancia de la tubería cliente
        {
            get
            {
                return pipeServer;
            }
            set
            {
                pipeServer = value;
            }     
        }

        private void OnMessageReceived(MessageReceivedEventArgs e) //Método que nos permite disparar el evento MessageReceived
        {
            MessageReceived?.Invoke(this, e);
        }
    }

    public class MessageReceivedEventArgs : EventArgs //Clase que hereda de EventArgs, la cual nos va a permitir transmitir el contenido del mensaje recibido cuando se dispare el evento MessageReceived
    {
        public string Message { get; private set; } //Propiedad que permite obtener y almacenar el contenido del mensaje recibido de la tubería

        public MessageReceivedEventArgs(string message) //Constructor que recibe como parámetro el mensaje recibido y que asigna dicho mensaje recibido al atributo Message
        {
            Message = message;
        }
    }
}
