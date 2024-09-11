using Microsoft.IdentityModel.Tokens;
using NLog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Arkad.Server.Helpers
{
    public class SecurityUtils
    {
        private static string TAG = typeof(SecurityUtils).FullName;
        static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Genera un token JWT firmado con los datos de reclamaciones proporcionados, y 
        /// luego encripta ese token usando AES junto con un vector de inicialización (IV) generado aleatoriamente. 
        /// Devuelve el token encriptado como una cadena Base64.
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="expire"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string TokenEncode(ClaimsIdentity claims, DateTime? expire, string secretKey)
        {
            // Validar argumentos de entrada
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException(nameof(secretKey));
            }

            // Convertir la clave secreta a bytes
            var key = Encoding.UTF8.GetBytes(secretKey);

            // Generar un vector de inicialización aleatorio
            var iv = GenerateRandomBytes(16);

            // Crear el descriptor del token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = expire,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            };

            // Crear el token
            var tokenHandler = new JwtSecurityTokenHandler();
            var createdToken = tokenHandler.CreateToken(tokenDescriptor);

            // Convertir el token a string
            var tokenString = tokenHandler.WriteToken(createdToken);

            // Encriptar el token usando AES
            var encryptedToken = EncryptToken(tokenString, key, iv);

            // Convertir el token encriptado a Base64
            return encryptedToken;
        }

        /// <summary>
        /// Descifra el token encriptado usando AES utilizando la clave y el vector de inicialización correspondientes. 
        /// Luego, decodifica el token descifrado y lo devuelve como un objeto JwtSecurityToken.
        /// </summary>
        /// <param name="tokenEncoded"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static JwtSecurityToken TokenDecode(string tokenEncoded, string secretKey)
        {
            try
            {
                // Validar si el token es nulo o vacío
                if (string.IsNullOrEmpty(tokenEncoded))
                {
                    return null;
                }

                var jwtEncodedString = (tokenEncoded.StartsWith("Bearer ")) ? tokenEncoded.Substring(7) : tokenEncoded; // trim 'Bearer ' from the start since its just a prefix for the token string

                // Decodificar el token encriptado desde Base64
                var encryptedToken = Convert.FromBase64String(jwtEncodedString);

                // Convertir la clave secreta a bytes
                var key = Encoding.UTF8.GetBytes(secretKey);

                // Extraer el vector de inicialización del token encriptado
                var iv = encryptedToken.Take(16).ToArray();

                // Extraer los datos encriptados del token
                var encryptedData = encryptedToken.Skip(16).ToArray();

                // Desencriptar el token usando AES
                var decryptedToken = DecryptToken(encryptedData, key, iv);

                // Crear un token JWT a partir del token desencriptado
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDecoded = tokenHandler.ReadJwtToken(decryptedToken);

                return tokenDecoded;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Se utiliza para generar el vector de inicialización aleatorio utilizado en la encriptación del token
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private static byte[] GenerateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        /// <summary>
        /// Crea una instancia de AES, configura la clave y el IV, y luego utiliza un encriptador AES para cifrar el token. El token cifrado se devuelve como un arreglo de bytes
        /// </summary>
        /// <param name="token"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string EncryptToken(string token, byte[] key, byte[] iv)
        {
            byte[] encryptedBytes;

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Generar una clave de 32 bytes a partir de la clave proporcionada
                byte[] derivedKey = new Rfc2898DeriveBytes(key, new byte[8], 1000, HashAlgorithmName.SHA256).GetBytes(32);

                aes.Key = derivedKey;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
                        cryptoStream.Write(tokenBytes, 0, tokenBytes.Length);
                    }

                    encryptedBytes = memoryStream.ToArray();
                }

                // Concatenar el IV y los datos encriptados
                byte[] encryptedToken = iv.Concat(encryptedBytes).ToArray();

                return Convert.ToBase64String(encryptedToken);
            }
        }

        /// <summary>
        /// Crea una instancia de AES, configura la clave y el IV, y utiliza un desencriptador AES para descifrar el token. El token descifrado se devuelve como una cadena de texto.
        /// </summary>
        /// <param name="encryptedToken"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        private static string DecryptToken(byte[] encryptedToken, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                // Derivar la clave utilizando PBKDF2 con SHA256
                byte[] derivedKey = new Rfc2898DeriveBytes(key, new byte[8], 1000, HashAlgorithmName.SHA256).GetBytes(32);

                aes.Key = derivedKey;
                aes.IV = iv;

                // Crear un desencriptador AES
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var memoryStream = new MemoryStream(encryptedToken))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (var streamReader = new StreamReader(cryptoStream))
                {
                    // Leer y devolver el token desencriptado como string
                    return streamReader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Decodifica el token utilizando la función TokenDecode y 
        /// extrae el valor correspondiente al tipo de reclamación especificado. 
        /// Devuelve el valor de la reclamación si está presente en el token, de 
        /// lo contrario devuelve una cadena vacía.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="secretKey"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetValByTypeFromToken(string token, string secretKey, string type)
        {
            string value = "";

            try
            {
                // Decodificar el token
                JwtSecurityToken jwt = TokenDecode(token, secretKey);
                List<Claim> claims = jwt.Claims.ToList();

                // Obtener el valor correspondiente al tipo especificado desde el token
                value = claims.Where(x => x.Type == type).FirstOrDefault()?.Value;
            }
            catch (Exception e)
            {
                logger.Error($"{TAG} --- {e} ---");
                throw;
            }

            return value;
        }
    }
}
