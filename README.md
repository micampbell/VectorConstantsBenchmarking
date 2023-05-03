# VectorConstantsBenchmarking
This is a benchmark that corresponds to an Performance Issue post at https://github.com/dotnet/runtime

### Description
With System.Numerics, there are many useful constant Vector values. For example, Vector3.UnitX. These cannot be ```const``` because they involve a custom type so they are defined as static properties:
```
        public static Vector3 UnitX
        {
            [Intrinsic]
            get => new Vector3(1.0f, 0.0f, 0.0f);
        }
```
However, this means that every time the property is called, so is the constructor. As a result, UnitX is recreated every time. This should be avoided as it requires additional time and memory.
A simply fix is to make it a static field.
```
        public static Vector3 UnitX = new Vector3(1.0f, 0.0f, 0.0f);
```
This way, each constant will be initialized once.

### BenchmarkDotNet Data
A small repository testing this can be found here: https://github.com/micampbell/VectorConstantsBenchmarking. While this repo superficially sits outside of dotnet/runtime 
(it doesn't get under the hood), it shows that repeated calls to a similarly created property is much more time-consuming than implementing a simple static field that would be initialized once.
In the benchmarking data shown below, "UsingProperties" is like the current approach. "UsingStatics" is the recommended fix as the "UsingPrivateFields" approach is not
compatible with current programming patterns and essentially 'cheats' the benchmark by defining the constants outside of the test.
``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1555/22H2/2022Update/SunValley2)
11th Gen Intel Core i7-1185G7 3.00GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.100-preview.3.23178.7
  [Host]     : .NET 8.0.0 (8.0.23.17408), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.0 (8.0.23.17408), X64 RyuJIT AVX2


```
|             Method |      Mean |     Error |    StdDev |
|------------------- |----------:|----------:|----------:|
|    UsingProperties | 34.876 ns | 0.4941 ns | 0.4126 ns |
| UsingPrivateFields |  2.224 ns | 0.0201 ns | 0.0188 ns |
|       UsingStatics |  2.668 ns | 0.0208 ns | 0.0174 ns |
|   UsingSysNumerics |  1.013 ns | 0.0403 ns | 0.0431 ns |


### Conclusion

It is definitely interesting how much better the current System.Numerics approach is (see "UsingSysNumerics"). So, this may all be a waste of time! But I think that 
may be a result of the [Intrinsic] attribute or even something else within the System.Numerics implementation. Still, it seems clear that re-initializing these constants at every invocation is a unnecessary cost, and easily fixed.
