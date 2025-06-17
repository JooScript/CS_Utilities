using System.Text;

namespace Utilities
{
    public static class ClsNumberSystemConverter
    {
        #region Binary Conversions

        /// <summary>
        /// Converts a binary string to its decimal equivalent
        /// </summary>
        /// <param name="binary">Binary string (may include negative sign)</param>
        /// <returns>Decimal integer value</returns>
        public static int BinaryToDecimal(string binary)
        {
            ClsValidation.ValidateBinary(binary);

            bool isNegative = binary.StartsWith("-");
            string cleanBinary = isNegative ? binary.Substring(1) : binary;

            // Using LINQ for more concise conversion
            int decimalValue = cleanBinary
                .Select((c, i) => c == '1' ? 1 << (cleanBinary.Length - 1 - i) : 0)
                .Sum();

            return isNegative ? -decimalValue : decimalValue;
        }

        /// <summary>
        /// Converts a binary string to hexadecimal
        /// </summary>
        public static string BinaryToHexadecimal(string binary)
        {
            ClsValidation.ValidateBinary(binary);

            // Pad with leading zeros to make length a multiple of 4
            int padding = (4 - (binary.Length % 4)) % 4;
            string paddedBinary = binary.PadLeft(binary.Length + padding, '0');

            StringBuilder hexBuilder = new StringBuilder();

            for (int i = 0; i < paddedBinary.Length; i += 4)
            {
                string nibble = paddedBinary.Substring(i, 4);
                int value = BinaryToDecimal(nibble);
                hexBuilder.Append(value.ToString("X"));
            }

            return hexBuilder.ToString();
        }

        /// <summary>
        /// Converts a binary string to octal
        /// </summary>
        public static string BinaryToOctal(string binary)
        {
            ClsValidation.ValidateBinary(binary);

            // Pad with leading zeros to make length a multiple of 3
            int padding = (3 - (binary.Length % 3)) % 3;
            string paddedBinary = binary.PadLeft(binary.Length + padding, '0');

            StringBuilder octalBuilder = new StringBuilder();

            for (int i = 0; i < paddedBinary.Length; i += 3)
            {
                string triplet = paddedBinary.Substring(i, 3);
                int value = BinaryToDecimal(triplet);
                octalBuilder.Append(value);
            }

            return octalBuilder.ToString();
        }

        /// <summary>
        /// Converts a decimal number to binary string
        /// </summary>
        public static string DecimalToBinary(int decimalNumber)
        {
            if (decimalNumber == 0) return "0";

            bool isNegative = decimalNumber < 0;
            uint number = isNegative ? (uint)(-decimalNumber) : (uint)decimalNumber;

            // Calculate required capacity upfront
            int bits = number == 0 ? 1 : (int)Math.Log(number, 2) + 1;
            char[] buffer = new char[bits + (isNegative ? 1 : 0)];
            int index = bits;

            while (number > 0)
            {
                buffer[--index] = (number & 1) == 1 ? '1' : '0';
                number >>= 1;
            }

            if (isNegative)
            {
                Array.Copy(buffer, 0, buffer, 1, bits);
                buffer[0] = '-';
            }

            return new string(buffer);
        }

        #endregion

        #region Decimal Conversions

        /// <summary>
        /// Converts a decimal number to hexadecimal string
        /// </summary>
        public static string DecimalToHexadecimal(int decimalNumber)
        {
            return decimalNumber.ToString("X");
        }

        /// <summary>
        /// Converts a decimal number to octal string
        /// </summary>
        public static string DecimalToOctal(int decimalNumber)
        {
            if (decimalNumber == 0) return "0";

            bool isNegative = decimalNumber < 0;
            uint number = isNegative ? (uint)(-decimalNumber) : (uint)decimalNumber;

            StringBuilder octal = new StringBuilder();

            while (number > 0)
            {
                octal.Insert(0, number % 8);
                number /= 8;
            }

            return isNegative ? "-" + octal.ToString() : octal.ToString();
        }

        #endregion

        #region Hexadecimal Conversions

        /// <summary>
        /// Converts a hexadecimal string to decimal
        /// </summary>
        public static int HexadecimalToDecimal(string hex)
        {
            ClsValidation.ValidateHexadecimal(hex);
            return Convert.ToInt32(hex, 16);
        }

        /// <summary>
        /// Converts a hexadecimal string to binary
        /// </summary>
        public static string HexadecimalToBinary(string hex)
        {
            ClsValidation.ValidateHexadecimal(hex);
            return DecimalToBinary(HexadecimalToDecimal(hex));
        }

        /// <summary>
        /// Converts a hexadecimal string to octal
        /// </summary>
        public static string HexadecimalToOctal(string hex)
        {
            ClsValidation.ValidateHexadecimal(hex);
            return DecimalToOctal(HexadecimalToDecimal(hex));
        }

        #endregion

        #region Octal Conversions

        /// <summary>
        /// Converts an octal string to decimal
        /// </summary>
        public static int OctalToDecimal(string octal)
        {
            ClsValidation.ValidateOctal(octal);
            return Convert.ToInt32(octal, 8);
        }

        /// <summary>
        /// Converts an octal string to binary
        /// </summary>
        public static string OctalToBinary(string octal)
        {
            ClsValidation.ValidateOctal(octal);
            return DecimalToBinary(OctalToDecimal(octal));
        }

        /// <summary>
        /// Converts an octal string to hexadecimal
        /// </summary>
        public static string OctalToHexadecimal(string octal)
        {
            ClsValidation.ValidateOctal(octal);
            return DecimalToHexadecimal(OctalToDecimal(octal));
        }

        #endregion
    }
}