using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace RegisterDevice
{



    class Program
    {
        static RegistryManager registryManager;


        static void Main(string[] args)
        {
            try
            {

                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: RegisterDevice [hub connection string] [device name] [OPTIONS]\n\n");
                    Console.WriteLine("Options:\n");
                    Console.WriteLine("\t-d\tDelete Device");
                    Console.WriteLine("\t-v\tView device properties");
                    return;
                }


                registryManager = RegistryManager.CreateFromConnectionString(args[0]);

                if (args.Length > 2 && args[2].ToLower() == "-d")
                    RemoveDeviceAsync(args[1]).Wait();
                else if (args.Length > 2 && args[2].ToLower() == "-v")
                    ViewDeviceAsync(args[1]).Wait();
                else
                    AddDeviceAsync(args[1]).Wait();


            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e.ToString());
                Console.ResetColor();

            }


        }

        private static async Task AddDeviceAsync(string deviceId)
        {
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }

        private static async Task RemoveDeviceAsync(string deviceId)
        {
            Device device = await registryManager.GetDeviceAsync(deviceId);
            if (device == null)
            {
                DeviceNotFound(deviceId);
            }
            else
            {
                await registryManager.RemoveDeviceAsync(deviceId);
                Console.WriteLine("Successfully removed device: {0}", deviceId);
            }

        }

        private static async Task ViewDeviceAsync(string deviceId)
        {
            Device device = await registryManager.GetDeviceAsync(deviceId);
            if (device == null)
            {
                DeviceNotFound(deviceId);
            }
            else
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(device))
                {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(device);
                    Console.WriteLine("{0}{1}{2}", name,new string('.',40-name.Length) ,value);
                }
            }

        }

        private static void DeviceNotFound(string deviceId)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Device {0} Not Found", deviceId);
            Console.ResetColor();
        }
    }
}
