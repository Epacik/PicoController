using PicoController.Core.Devices.Inputs;
using PicoController.Core.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicoController.Core.Config
{
    public class Config : IClonable<Config>
    {
        public Config() { }

        [JsonConstructor]
        public Config(int maxDelayBetweenClicks, List<Device> devices)
        {
            MaxDelayBetweenClicks = maxDelayBetweenClicks;
            Devices = devices;
        }

        public Config(List<Device> devices)
        {
            Devices = devices;
        }

        [JsonPropertyName("maxDelayBetweenClicks")]
        public int MaxDelayBetweenClicks { get; set; }

        [JsonPropertyName("verbosity")]
        public string? Verbosity { get; set; }

        [JsonPropertyName("devices")]
        public List<Device> Devices { get; set; } = new List<Device>();

        public static Config ExampleConfig()
        {
            byte inputId = 0;
            return new()
            {
                MaxDelayBetweenClicks = 50,
                Devices = new()
                {
                    new Device
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Example device",
                        Interface = new(InterfaceType.COM, new()
                        {
                            { "port", JsonSerializer.SerializeToElement("COM6") },
                            { "rate", JsonSerializer.SerializeToElement(115200) },
                            { "dataBits", JsonSerializer.SerializeToElement(8) },
                            { "stopBits", JsonSerializer.SerializeToElement(1) },
                            { "parity", JsonSerializer.SerializeToElement(0) }
                        }),
                        Inputs = new Input[]
                        {
                            GetExampleInput(inputId++, InputType.Button),
                            GetExampleInput(inputId++, InputType.Button),
                            GetExampleInput(inputId++, InputType.Button),
                            GetExampleInput(inputId++, InputType.Button),
                            GetExampleInput(inputId++, InputType.EncoderWithButton),
                            GetExampleInput(inputId++, InputType.EncoderWithButton),
                            GetExampleInput(inputId++, InputType.EncoderWithButton),
                            GetExampleInput(inputId++, InputType.EncoderWithButton),
                            GetExampleInput(inputId++, InputType.EncoderWithButton),
                        },
                    }
                }
            };
        }

        public static Config ExampleConfig(int numberOfDevices)
        {
            var rand = new Random();
            
            var devices = new Device[numberOfDevices];

            for (int i = 0; i < numberOfDevices; i++)
            {
                var type = (InterfaceType)rand.Next(1, 3);
                Debug.WriteLine($"Device interface type: {type}");

                var inputs = new Input[rand.Next(2, 20)];

                for (int j = 0; j < inputs.Length; j++)
                {
                    inputs[j] = GetExampleInput((byte)j, (InputType)rand.Next(1, (int)InputType.MAX));
                }

                devices[i] = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"Example device {i}",
                    Interface = new(type, type switch
                    {
                        InterfaceType.COM => new()
                        {
                            { "port",     JsonSerializer.SerializeToElement($"COM{rand.Next(0, 9)}") },
                            { "rate",     JsonSerializer.SerializeToElement(115200) },
                            { "dataBits", JsonSerializer.SerializeToElement(8) },
                            { "stopBits", JsonSerializer.SerializeToElement(1) },
                            { "parity",   JsonSerializer.SerializeToElement(0) }
                        },
                        InterfaceType.WiFi => new()
                        {
                            { "ip", JsonSerializer.SerializeToElement($"192.168.1.{rand.Next(1, 254)}") },
                        },
                        InterfaceType.Bluetooth => new()
                        {
                            { "name", JsonSerializer.SerializeToElement($"Device {rand.Next(1, 254)}") }
                        },
                        InterfaceType.None => new() { },
                        _ => throw new NotImplementedException(),
                    }),
                    Inputs = inputs,
                };

            }

            return new()
            {
                MaxDelayBetweenClicks = 50,
                Devices = devices.ToList(),
            };
        }

        private static Input GetExampleInput(byte id, InputType type) => new Input(id, type, GetExampleActions(type));

        private static Dictionary<string, InputAction> GetExampleActions(InputType type) => type switch
        {
            InputType.Button => new()
            {
                { "press", new InputAction() },
                { "doublePress", new InputAction() },
                { "triplePress", new InputAction() },
            },
            InputType.Encoder => new()
            {
                { "rotationClockwise", new InputAction() },
                { "rotationCounterClockwise", new InputAction() },
                { "pressedRotationClockwise", new InputAction() },
                { "pressedRotationCounterClockwise", new InputAction() },
            },
            InputType.EncoderWithButton | InputType.MAX => new()
            {
                { "press", new InputAction() },
                { "doublePress", new InputAction() },
                { "triplePress", new InputAction() },
                { "rotationClockwise", new InputAction() },
                { "rotationCounterClockwise", new InputAction() },
                { "pressedRotationClockwise", new InputAction() },
                { "pressedRotationCounterClockwise", new InputAction() },
            },

            _ => throw new NotImplementedException(),
        };

        public Config Clone()
        {
            var config = (Config)MemberwiseClone();
            config.Devices = Devices.ConvertAll(x => x.Clone());
            return config;
        }
    }
}
