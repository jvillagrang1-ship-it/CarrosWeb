using System;
using System.Security.Cryptography;
using System.Text;

namespace CarrosWeb.Negocio
{
    public static class SeguridadHelper
    {
        public static string GenerarSalt()
        {
            byte[] bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static string CalcularHash(string contrasenaPlano, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] datos = Encoding.UTF8.GetBytes(contrasenaPlano + salt);
                return Convert.ToBase64String(sha256.ComputeHash(datos));
            }
        }

        public static bool Verificar(string contrasenaPlano, string salt, string hashGuardado)
        {
            return CalcularHash(contrasenaPlano, salt) == hashGuardado;
        }
    }
}
