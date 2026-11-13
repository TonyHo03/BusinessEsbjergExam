using Isopoh.Cryptography.Argon2;

namespace Service;

public class PasswordHasher
{
    private const int TimeCost = 3;
    private const int MemoryCostKiB = 65536;
    private const int Parallelism = 1;

    public string DummyHash { get; }

    public PasswordHasher() => DummyHash = Hash("dummy-password-for-timing");

    public string Hash(string password) =>
        Argon2.Hash(password, timeCost: TimeCost, memoryCost: MemoryCostKiB,
            parallelism: Parallelism, type: Argon2Type.HybridAddressing);

    public bool Verify(string hash, string password) => Argon2.Verify(hash, password);
}