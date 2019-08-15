using System;

namespace Core.DomainServices
{
    public interface ICryptoService : IDisposable
    {
        string Encrypt(string str);
    }
}