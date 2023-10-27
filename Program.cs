using System;
using System.Net;
using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;
using PacketDotNet.Tcp;
using System.Collections.ObjectModel;
using PacketDotNet.Ieee80211;
using PcapDotNet.Core;
using PcapDotNet.Packets;

namespace Example3
{
    //CAPTURA DE PAQUETE SIMPLE DEMO 1
    public class Program
    {
        public static int packetCounter = 0;
        public static void Main()
        {
            
            Console.WriteLine("PACKET CAPTURE DEMO");

            //Checkeamos nuestra version de SharpPcap
            var version = Pcap.Version;

            Console.WriteLine("SharpPcap verion: " + version);
            Console.WriteLine();

            //Vamos a verificar que se encuentren interfaces disponibles 

            var devices = LibPcapLiveDeviceList.Instance;
            Console.WriteLine("RAW PACKET CAPTURE" + Environment.NewLine);
            Console.WriteLine(devices.Count < 1 ? "No interfaces found" : "Interfaces Availables");
            Console.WriteLine();

            //Capturar las interfaces disponibles en el dispositivo

            //Crear contador
            int count_Interfaces = 0;

            //Imprimir lista de interfaces disponibles
            Console.WriteLine("Available interfaces to capture");

            foreach (var dev in devices)
            {
                Console.WriteLine("{0} {1}", count_Interfaces, dev.Interface);
                count_Interfaces++;
            }

            Console.WriteLine("---------------------------------------");


            //Selección de Interfaz 
            Console.WriteLine("Selecciona una interfaz");

            //Utilizando nuestra misma variable counterInterfaces haremos que el usuario selecione la Interfaz a capturar 
            count_Interfaces = int.Parse(Console.ReadLine());

            /*Capturamos la seleccion del usuario y guardamos en una variable de tipo var llamada "device" Importante recordar que nuestra variable "devices" contiene
             vario valores lo cual lo convierte en un array. 

            device = devices[counterInterfaces] aqui decimos que de la variables "devices" seleccionaremos la entrada del usuario
             
             */

            var device = devices[count_Interfaces]; //
            
            Console.WriteLine($"Interfaz Seleccionada\n: Media Type: {device.Interface.FriendlyName},\n Adpater: {device.Interface.Description }");
            int cont = 0;
            List<string> addreses = new List<string>();
            foreach(var dev in device.Interface.Addresses)
            {
                addreses.Add(dev.Addr.ToString());
            }
            Console.WriteLine($" IPv4 address: {addreses[0]}\n IPv6 address: {addreses[1]}\n Mac Address{addreses[2]}");

            Console.WriteLine("-------------------------------------------------------------------"+ Environment.NewLine);
            Console.WriteLine("PRESS ENTER TO START CAPTURING");

            Console.ReadLine();


            Console.WriteLine("SIMPLE PACKET CAPTURE / PRESS ENTER TO STOP CAPTURING AND SEE STATUS");


            using var deviceCaptured = devices[count_Interfaces];//Interfaz previamente seleccinada guardada en una nueva variable deviceCapture


            //Registramos nuestra función de controlador para el evento de llegada de paquetes

            deviceCaptured.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);

            
            int readTimeoutMilliseconds = 1000; //Especifiacamos el tiempo máximo de espera para la lectura de paquetes en milisegundos

            /*Abrimos nuestra interfaz para que empiece a capturar
             
             la siguiente linea de código
             deviceCaptured.Open(mode: DeviceModes.Promiscuous | DeviceModes.DataTransferUdp | DeviceModes.NoCaptureLocal);

            Analisis del código

            - deviceCapture contiene lo que es nuestra interfaz que seleccionamos
            - deviceCapture.Open() abrimos la interfaz para empezar la captura

            - mode: DeviceModes.Promiscuous | DeviceModes.DataTransferUdp | DeviceModes.NoCaptureLocal: Aquí se establece el modo de operación del dispositivo de red. 
            En este caso, se utilizan varias opciones en modo de bits (bitwise OR) para especificar los modos deseados:

            - mode: DevicesModes.Promiscous: Esta opción indica que el dispositivo debe estar 
            en modo Promiscous (Promiscuo) esto permite la captura de todos los paquetes en la red,
            incluso aquellos que no estan destinados a la propia máquina

            -mode: Devices.DataTransferUdp: Indicamos se de desea transferencias de protocolo UDP (USER DATA PROGRAM)

            -mode: DeviceModes.NoCaptureLocal: Esta opción indica que los paquetes originados
            por la propia interfaz no deben ser capturado. Esto es útil para evitar la captura
            de los propios paquetes generados por el programa que realiza la captura
             

            - read_timeout: readTimeoutMilliseconds: Aquí se especifica el tiempo máximo de espera para la lectura de paquetes en milisegundos.
            Es decir, si no se reciben paquetes durante este 
            período de tiempo, la operación de lectura finalizará.
             
             
             
             */

            deviceCaptured.Open(mode: DeviceModes.Promiscuous | DeviceModes.DataTransferUdp | DeviceModes.NoCaptureLocal );

            Console.WriteLine();

            Console.WriteLine("--Listening on {0} {1}, hit Enter to stop Capturing...", deviceCaptured.Name, deviceCaptured.Description);

            //Empezar a Capturar
            device.StartCapture();

            //Esperar por a que el usuario presione 'ENTER'
            Console.ReadLine();

            //Parar Captura de procesos
            deviceCaptured.StopCapture();

            Console.WriteLine("Capture Stopped...");

            //Imprimir estadisitica de la interfaz
            Console.WriteLine($"Send: {packetCounter}, Dropped: {deviceCaptured.Statistics.DroppedPackets}, Interface Dropped Packets: {deviceCaptured.Statistics.InterfaceDroppedPackets}");

        }

        //Metodo para capturar los paquetes desde nuestra interfaz
        private static void device_OnPacketArrival(object sender, PacketCapture e)
        {
            packetCounter++;
     
            /*Obtiene la marca de tiempo del paquete capturado y la convierte en un objeto `DateTime`
             representando la fecha y hora exactas en que se capturó el paquete. La variable `time`
            obtendra estas marcas de tiempo*/
            var time = e.Header.Timeval.Date;

            /*Obtenemos la longitud del paquete capturado `e.Data`. La varieble `len` alamacenara esta valor*/
            var len = e.Data.Length;

            /*Obtenemos la representación sin procesar del paquete capturado utilizando
             el método `GetPacket()` proporcionado por el objeto 'e'*/
            var rawPacket = e.GetPacket();
            
            Console.WriteLine($"{time.Hour}:{time.Minute}:{time.Second}:{time.Millisecond} Len={len}");
            Console.WriteLine(rawPacket.ToString());
        }
      

    }
}






//evices = LibPcapLiveDeviceList.Instance;

// 
//LibPcapLiveDevice device = LibPcapLiveDeviceList.Instance.FirstOrDefault(d => d.Interface.GatewayAddresses != null && d.Interface.FriendlyName == "Wi-Fi");

//if (device == null)
//{
//    Console.WriteLine("No active network inteface found...");
//    return;
//}

/*  device.Open(DeviceModes.Promiscuous);
  device.Filter = "arp";


          Console.WriteLine($"ARP MAC Address Table for interface: {device.Interface.FriendlyName}");


  //Capturar Paquetes ARP
  device.OnPacketArrival += (sender, e) =>
  {
      var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
      var arpPacket = packet.Extract<ArpPacket>();

      if(arpPacket != null && arpPacket.Operation == ArpOperation.Response)
      {
          Console.WriteLine($"IP Address: {arpPacket.SenderProtocolAddress}, Mac Address: {arpPacket.SenderHardwareAddress}");

      }
  };

  device.StartCapture();

  Console.WriteLine("Pulsa Cualquier Tecla para detener la Captura");
  Console.ReadLine();

  device.StopCapture();
  device.Close();
*/