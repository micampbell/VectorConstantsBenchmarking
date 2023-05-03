using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Numerics;


public class VectorConstantTest
{
    [GlobalSetup]
    public void Setup()
    {
        // initiate fields within the class. This is for testing #2 below
        Vector3One = new Vector3(1, 1, 1);
        Vector3Zero = new Vector3(0, 0, 0);
        VectorUnitX = new Vector3(1, 0, 0);
        Vector3UnitY = new Vector3(0, 1, 0);
        Vector3UnitZ = new Vector3(0, 0, 1);
    }
    #region 1. Static properties as is similarly done in System.Numerics
    public static Vector3 Vector3ZeroAsProp => default;
    public static Vector3 Vector3OneAsProp => new Vector3(1.0f);
    public static Vector3 Vector3UnitXAsProp => new Vector3(1.0f, 0.0f, 0.0f);
    public static Vector3 Vector3UnitYAsProp => new Vector3(0.0f, 1.0f, 0.0f);
    public static Vector3 Vector3UnitZAsProp => new Vector3(0.0f, 0.0f, 1.0f);
    
    [Benchmark]
    // this is slow as the "new" is called every time the constant is referenced, which adds time and memory
    public Vector3 UsingProperties()
    {
        var v1 = Vector3UnitXAsProp + Vector3UnitYAsProp + Vector3UnitZAsProp;
        var v2 = Vector3UnitXAsProp + Vector3UnitYAsProp + Vector3UnitZAsProp + Vector3OneAsProp;
        var v3 = Vector3OneAsProp - Vector3UnitXAsProp - Vector3UnitYAsProp - Vector3UnitZAsProp + Vector3ZeroAsProp;
        var v4 = Vector3.Dot(Vector3OneAsProp, Vector3UnitXAsProp);
        return (v1 + v2 + v3) * v4;
    }
    #endregion

    #region 2. Instance fields initiated at the start
    private Vector3 Vector3One, Vector3Zero, VectorUnitX, Vector3UnitY, Vector3UnitZ;

    [Benchmark]
    // This is fast, but for a library like System.Numerics, we don't really have a facility to call a Setup function.
    // It's also not an acceptable programming pattern. The time is the same as static fields anyway
    public Vector3 UsingPrivateFields()
    {
        var v1 = VectorUnitX + Vector3UnitY + Vector3UnitZ;
        var v2 = VectorUnitX + Vector3UnitY + Vector3UnitZ + Vector3One;
        var v3 = Vector3One - VectorUnitX - Vector3UnitY - Vector3UnitZ + Vector3Zero;
        var v4 = Vector3.Dot(Vector3One, VectorUnitX);
        return (v1 + v2 + v3) * v4;
    }
    #endregion

    #region 3. Static fields initiated once (when? at the start?)
    static Vector3 Vector3OneStatic = new Vector3(1, 1, 1);
    static Vector3 Vector3ZeroStatic = new Vector3(0, 0, 0);
    static Vector3 VectorUnitXStatic = new Vector3(1, 0, 0);
    static Vector3 Vector3UnitYStatic = new Vector3(0, 1, 0);
    static Vector3 Vector3UnitZStatic = new Vector3(0, 0, 1);

    [Benchmark]
    // This is fast as the static properties are set only once.
    public Vector3 UsingStatics()
    {
        var v1 = VectorUnitXStatic + Vector3UnitYStatic + Vector3UnitZStatic;
        var v2 = VectorUnitXStatic + Vector3UnitYStatic + Vector3UnitZStatic + Vector3OneStatic;
        var v3 = Vector3OneStatic - VectorUnitXStatic - Vector3UnitYStatic - Vector3UnitZStatic + Vector3ZeroStatic;
        var v4 = Vector3.Dot(Vector3OneStatic, VectorUnitXStatic);
        return (v1 + v2 + v3) * v4;
    }
    #endregion

    #region 4. Using properties from System.Numerics. However, these are optimized with the [Intrinsic] attribute
    [Benchmark]
    // This is by far the best, but I would guess the timing is a result of the [Intrinsic] attribute,
    // which this simple method project can't access.
    public Vector3 UsingSysNumerics()
    {
        var v1 = Vector3.UnitX + Vector3.UnitY + Vector3.UnitZ;
        var v2 = Vector3.UnitX + Vector3.UnitY + Vector3.UnitZ + Vector3.One;
        var v3 = Vector3.One - Vector3.UnitX - Vector3.UnitY - Vector3.UnitZ;
        var v4 = Vector3.Dot(Vector3.One, Vector3.UnitX);
        return (v1 + v2 + v3) * v4;

    }
    #endregion
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<VectorConstantTest>();
    }
    // here are the results from my machine
    /*
    BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1555/22H2/2022Update/SunValley2)
    11th Gen Intel Core i7-1185G7 3.00GHz, 1 CPU, 8 logical and 4 physical cores
    .NET SDK= 8.0.100-preview.3.23178.7
    [Host]     : .NET 8.0.0 (8.0.23.17408), X64 RyuJIT AVX2
    DefaultJob : .NET 8.0.0 (8.0.23.17408), X64 RyuJIT AVX2


|             Method |      Mean |     Error |    StdDev |
|------------------- |----------:|----------:|----------:|
|    UsingProperties | 34.876 ns | 0.4941 ns | 0.4126 ns |
| UsingPrivateFields |  2.224 ns | 0.0201 ns | 0.0188 ns |
|       UsingStatics |  2.668 ns | 0.0208 ns | 0.0174 ns |
|   UsingSysNumerics |  1.013 ns | 0.0403 ns | 0.0431 ns |
   
     */
}