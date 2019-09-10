using System;

namespace Infrastructure.Services.Cryptography
{
    public interface ICryptoService : IDisposable
    {
        string Encrypt(string str);
    }
}