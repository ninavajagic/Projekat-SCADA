using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            byte[] request = new byte[12];
            ModbusReadCommandParameters mdmRead = this.CommandParameters as ModbusReadCommandParameters;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdmRead.TransactionId)), 0, request, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdmRead.ProtocolId)), 0, request, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdmRead.Length)), 0, request, 4, 2);
            request[6] = mdmRead.UnitId;
            request[7] = mdmRead.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdmRead.StartAddress)), 0, request, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdmRead.Quantity)), 0, request, 10, 2);
            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            //TO DO: IMPLEMENT
            ModbusReadCommandParameters mdmRead = this.CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> dict = new Dictionary<Tuple<PointType, ushort>, ushort>();
            int counter = 0;
            ushort address = mdmRead.StartAddress;
            ushort value;
            byte mask = 1;
            for (int i = 0; i < response[8]; i++)
            {
                byte tempByte = response[9 + i];
                for (int j = 0; j < 8; j++)
                {
                    value = (ushort)(tempByte & mask);
                    tempByte >>= 1;
                    dict.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, address), value);
                    counter++;
                    address++;
                    if (counter == mdmRead.Quantity)
                    {
                        break;
                    }
                }
            }
            return dict;
        }
    }
}