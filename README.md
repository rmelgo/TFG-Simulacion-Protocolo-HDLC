# Aplicación para la simulación del protocolo HDLC

![Inicial](https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/cbbe1d24-2982-4f5b-b33f-d2c34d43b0cc)

# - Introducción

Proyecto realizado en la asignatura de Trabajo de Fin de Grado del grado de Ingenieria Informática de la Universidad de Salamanca. El enunciado del proyecto se encuentra subido en el repositorio en un archivo pdf llamado *Popuesta TFG.pdf*.   
  
El principal objetivo de este proyecto es la realización de una aplicación de escritorio que implemente el protocolo de control de enlace de datos de alto nivel (HDLC).
El desarrrollo de esta herramienta esta principalmente enfocada en su uso en la asignatura de Redes de Computadores I del grado de Ingeniería Informática con el propósito de ser utilizada con fines didácticos.
Para el desarrollo de este proyecto, se permite total libertad a la hora de elegir el entorno de desarrollo en el que se construirá el proyecto.

Al tratarse de un Trabajo de Fin de Grado, el desarrollo no se basará exclusivamente en la cosntrucción de un producto software, si no que se realizaran exhautivos trabajos de investigación, análisis, diseño, documentación, planificación, etc.

# - Comentarios sobre el entorno de desarrollo

Para ejecutar este programa, se requerirá de una distribución del Sistema Operativo **Windows**.    

Para la elaboración y compilación de este simulador, se ha utilizado el framework **Visual Studio 2019**. 

En el repositorio se adjuntará un fichero ejecutable, de manera que no será necesario disponer de ninguna herramienta para la ejecución del simulador. Simplemente será necerario hacer uso del Sistema Operativo **Windows**. 

# Comentarios sobre el material adjuntado

El material adjuntado para la realización de este proyecto es tan amplia, que se va a clasificar en 2 secciones:

## Material teórico

En esta sección del proyecto, se adjuntarán una serie de documentos con distintos aspectos relevantes para la construcción del simulador del protocolo HDLC. Estos documentos se encuentran dentro de la carpeta de **Documentación** que se encuentra en el repositorio. Esta carpeta contiene los siguientes documentos:

- Un documento llamado ***Memoria TFG.pdf*** en el que se establece una *memoria general* del proyecto desarrollada. Esta memoria incluye:

  - Los objetivos a cumplir en el desarrollo del proyecto
  - Los conceptos teóricos fundamentales del protocolo HDLC
  - Trabajos relacionados
  - Métodologias, técnicas y herramientas utilizadas
  - Aspectos relevantes del desarrollo del proyecto (ciclo de vida)
  - Conclusiones y líneas futuras
    
- Un documento llamado ***Anexo I_ Plan de proyecto software.pdf*** en el que se realiza una *planificación* del proyecto. Esta planificación consta de:

  - Una estimación del esfuerzo del proyecto
  - Una planificación temporal del proyecto junto con un ánalisis de la planificación temporal (Diagrama de Gantt)

- Un documento llamado ***Anexo II.a_ Especificación de requisitos software.pdf*** en el que se realizan tareas relacionadas con la disciplina de *Requisitos*. Estas tareas son:

  - Una elicitación de requisitos la definición formal de los objetivos del sistema y elaboración del diagrama de casos de uso
  - Actividades relacionadas con el Desarrollo Centrado en el Usuario
 
- Un documento llamado ***Anexo II.b_ Análisis de requisitos.pdf*** en el que se realizan tareas relacionadas con la disciplina de *Análisis*. Estas tareas son:  
 
  - Un análisis de los requisitos previamente identificados y elaboración del diagrama de clases del sistema
  - Un análisis de tareas realizadas por el usuario en el futuro sistema
 
- Un documento llamado ***Anexo III_ Especificación del diseño.pdf*** en el que se realizan tareas relacionadas con la disciplina de *Diseño*. Estas tareas son:  

  - Realización del diseño de la estructura del sistema y la relación de los casos de uso previamente identificados a nivel de diseño
  - Realización de actividades de diseño de la interfaz de la aplicación.

- Un documento llamado ***Anexo IV_ Documentación técnica de programación.pdf*** en el que se incluye información relevante sobre la programación del simulador. Esta información es la siguiente:

  - Bibliotecas externas utilizadas
  - Estructura del código fuente de la aplicación
  - Colección de métodos utilizado (incluyendo nombre, parámetros, valores de retorno, clase en la que se encuentran y una breve descripción)

- Un documento llamado ***Anexo V_ Manual de usuario.pdf*** en el que se incluye un manual del usuario donde se explican las distintas funcionalidades que incluye el simulador así como el aspecto visual del simulador en disintos contextos.

## Implementación

En esta sección del proyecto, se adjuntarán una serie de ficheros que forman parte de la implementación del simulador. Estos ficheros se encuentran dentro de la carpeta de **Implementación** que se encuentra en el repositorio. Esta carpeta contiene los siguientes ficheros:

- Una carpeta llamada ***Simulador HDLC - Ejecutable y librerias necesarias*** que contiene el material mínimo para ejecutar el simulador HDLC y hacer uso de todas las funcionalidades. Esta carpeta incluye los siguientes ficheros:

  - Un fichero llamado ***Simulador HDLC.exe** el cual se trata de un fichero ejecutable que contiene la implementación del simulador HDLC.
  - Un fichero llamado ***Simulador HDLC.exe.config** que contiene una serie de configuraciones necesarias para ejecutar correctamente el simulador.
  - Un fichero llamado ***Simulador HDLC.pdb*** que contiene información de depuración sobre la ejeución del simulador.
  - Un fichero llamado ***Newtonsoft.Json.dll*** el cual se trata de una libreria de enlazado dinámico con funciones necesarias para el manejo de objetos JSON, los cuales son utilizados por el simulador.
  - Un fichero llamado ***Newtonsoft.Json.xml*** que contiene información sobre el contenido de la libreria de enlazado dinámico ***Newtonsoft.Json.dll***.
  - Un fichero llamado ***Imagen_Captura_Tráfico.png*** con un ejemplo de captura de tráfico guardado como imagen.
  - Un par de ficheros ***prueba_captura.txt*** y ***prueba_captura.txt (2)*** que contiene información sobre una captura de tráfico desde el punto de vista de las 2 máquinas involucradas en el enlace.
 
- Una carpeta llamada ***Simulador HDLC - Proyecto WPF completo.zip*** que contiene el proyecto WPF completo con la implementación del simulador del protocolo HDLC con todas las clases involucradas.

# - Estructura de la aplicación

Al realizar la implementación del simulador en el entorno de WPF, se van a tener 3 tipos de ficheros:

- Ficheros ***.xaml*** con la representación visual de las distintas ventanas del simulador.
- Ficheros ***.xaml.cs*** con la lógica interna del simulador.
- Ficheros ***.cs*** que actuan de modelos y almacenan los datos del simulador.

Estos ficheros se van a agrupar en distintas clases. El diagrama de clases del proyecto es la siguiente:

![Estructura](https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/0091fe1a-4de7-444b-8897-68196df8ed94)

En este diagrama, existen 3 tipos de clases:

- **Clases del sistema**: Son las que se encuentran en la primera fila
- **Clases ventana**: Incluyen los ficheros *.xaml* y *.xaml.cs*. Se encuentran en la segunda, tercera y cuarta fila.
- **Clases modelo**: Incluyen los ficheros *.cd*. Se encuentran en las últimas 2 filas.

# - Funcionalidades del simulador

## Pantalla principal

Al abrir el fichero ***Simulador HDLC.exe***, inicialmente se presentará una ventana similar a la que se presenta en la siguiente imagen:

![Ejemplo ejecucion 1](https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/fabc26a8-64c1-4b69-ae1d-633d069cdada)

La ventana principal se agrupa en 3 secciones principales:

- Una sección que contiene **información básica** sobre la estación. En concreto se muestra la siguiente información:
  
  - Nombre de la estación
  - Modo de funcionamiento (Semiautomático o manual)
  - Número de secuencia (VS)
  - Número de trama esperada (VR)
    
- Una sección que contiene 2 tablas en las que se **representa información sobre las tramas enviadas y recibidas por la estación**. En concreto, se muestra la siguiente información sobre cada trama:
  
  - Instante en el que se envío/recibío la trama en la estación
  - Tipo de trama
  - Dirección a la que va dirigida la trama (si es un comando) o dirección de origen de la trama (si es una respuesta)
  - Número de secuencia de la trama (NS)
  - Estado del bit de poll (P/F)
  - Número de trama esperada (NR)

- Una sección en la que se **representa graficamente información sobre las tramas enviadas y recibidas por la estación**. En concreto, se muestra la siguiente información sobre cada trama:

  - Dirección a la que va dirigida la trama (si es un comando) o dirección de origen de la trama (si es una respuesta)
  - Tipo de trama
  - Estado del bit de poll (P/F)
  - Número de secuencia de la trama (NS)
  - Número de trama esperada (NR)

## Configuración

Cada ventana de simulador corresponde a una estación. Cada estación tiene asociada una **configuración**. 

Dentro de la **configuración de la estación**, existen 3 secciones:

- Protocolo
- Modo de trabajo
- Canal

Para acceder a la configuración, se debe hacer click a un botón con una rueda dentada que se encuentra en la esquina superior derecha. 

### Configuración del protocolo de la estación

Al acceder a la configuración de la estación, se desplegará una ventana en la que se mostrará por defecto la sección de la configuración del protocolo de la estación. Esta sección tiene el siguiente aspecto:

![Ejemplo ejecucion 2](https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/c280d274-4db5-4a59-95bb-1b8a8806b6cf)

Dentro de la sección de la configuración del protocolo de la estación, se identifican 2 subsecciones:

- Una sección destinada a la configuración de ***timeouts***. En esta sección, se pueden configurar 3 tipos de timeouts:
  
  - **Timeout ante Command**: Este timeout hace referencia al tiempo máximo que una estación espera una respuesta cuando esta envía un comando con el bit de sondeo activado. 
  - **Timeout ante trama I**: Este timeout hace referencia al tiempo máximo que una estación espera reconocimiento de una trama de información anteriormente enviada. 
  - **Timeout ante Request**: Este timeout hace referencia al tiempo máximo que una estación espera la recepción de un comando tras realizar el envío de una respuesta.

**Nota**: Para obtener mas información del funcionamiento o significado de los timeouts, se recomienda utilizar el manual de usuario del simulador o utilizar los botones de ayuda con el símbolo "?".

- Una sección destinada a la configuración del ***control de flujo***. Esta sección cuenta con 2 párametros:

  - **Tamaño de la ventana**: Este parámetro hace referencia al número máximo de tramas que en un determinado momento pueden estar pendientes de confirmación. 
  - **Número de tramas erróneas consecutivas**: Este parámetro hace referencia al número máximo de tramas erróneas consecutivas permitidas que una estación puede enviar antes de desconectar el enlace físico con la otra estación conectada. 

**Nota**: Para obtener mas información del funcionamiento o significado de estos parámetros, se recomienda utilizar el manual de usuario del simulador o utilizar los botones de ayuda con el símbolo "?".

### Configuración del modo de trabajo de la estación

La sección de la configuración del modo de trabajo de la estación tiene el siguiente aspecto:

![Ejemplo ejecucion 3](https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/8aab81bd-0ad8-4afa-8079-b4d49d402a9c)

La estación tiene 2 modos de trabajo en los que la estación puede funcionar:

  - **Modo semiautomático**: La estación responde de manera automática en algunas situaciones. En el manual de usuario y en los sistemas de ayuda se detallan en que situciones se producen respuestas automáticas. También se aplicarán los timeouts configurados en la sección de protocolo y se permitira el envío de tramas erróneas.
  - **Modo manual**: La estación no responde de manera automática en ninguna situación. Tampoco se aplicarán los timeouts configurados en la sección de protocolo y ni se permitira el envío de tramas erróneas.

### Configuración del canal de transmisión de la estación

La sección de la configuración del canal de transmisión de la estación tiene el siguiente aspecto:

![Ejemplo ejecucion 4](https://github.com/rmelgo/TFG-Simulacion-Protocolo-HDLC/assets/145989723/bd9ca946-11ad-4382-8db1-c64249ad2dbd)

Dentro de la sección de la configuración del canal de transmisión de la estación, se identifican 2 parámetros fundamentales:

  - **Retardo**: El retardo hace referencia al tiempo transcurrido desde que se produce el envío de la trama desde la estación de origen hasta que se produce la recepción de la trama en la estación de destino. 
  - **Modo manual**: La tasa de error hace referencia a la probabilidad de que una trama enviada sea recibida de manera errónea debido a una alteración en el contenido de la trama.

**Nota**: Para obtener mas información del funcionamiento o significado de estos parámetros, se recomienda utilizar el manual de usuario del simulador o utilizar los botones de ayuda con el símbolo "?".
