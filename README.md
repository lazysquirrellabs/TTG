# Terraced Terrain Generator (TTG)
Terraced Terrain Generator (TTG) is a tool that can be used in Unity projects to procedurally generate terraced terrain meshes. 

## Contents
- [Features](#features)
- [Importing](#importing)
- [Usage](#usage)
	- [Component-based](#component-based-usage)
	- [API](#api-usage)
- [Samples](#samples)
- [Compatibility and dependencies](#compatibility-and-dependencies)
- [Roadmap](#roadmap)
- [Technical aspects](#technical-aspects)
- [Contributing](#contributing)
- [Getting help](#getting-help)
- [License](#license)

## Features
- Pseudorandom procedural terrain generation.
- Deterministic procedural terrain generation with parameterized seed.
- Both synchronous and asynchronous (with `async`/`await`) support.
- Reduced GC allocations using native constructs delivered by Unity's [Collections package](https://docs.unity3d.com/Packages/com.unity.collections@1.2/manual/index.html) (e.g. `NativeArray<T>` and `NativeList<T>`).
- Customizable:
	- Basic terrain shapes from 3 to 10 sizes.
	- Number of terraces (1 to 50).
	- Terrain size (radius and height).
	- Level 
	- Feature (hills and valleys) frequency.
	- Height distribution.

## Importing
The first step to get started with TTG is to import the library into your Unity project. So far, the only easy way to import TTG into your project is via UPM, using a git URL. To do so, navigate to `Window > Package Manager` in Unity. Then click on the `+` and select "Add package from git URL":

![](https://matheusamazonas.net/assets/images/ttg/upm_adding.png)
Next, enter the following URL in the "URL" input field:
```
https://github.com/matheusamazonas/ttg.git?path=Assets/Libraries/TerracedTerrainGenerator#1.0.0
```
Finally, click on the "Add" button. The importing process should start automatically. Once it's done, TTG is ready to be used in the project. Check the [Usage](#usage) section on how to use TTG and the [Samples](#samples) section on how to import and use the package samples.

## Usage

There are two different ways to use TTG: via the `TerrainGeneratorController` component and via the API. Both methods share the same parameter list, explained below:
- Side count: number of sides of the terrain's basic shape (3 to 10).
- Radius: the greatest distance between the center of the mesh and all of its vertices (ignoring their position's Y coordinate). This number must be greater than zero.
- Terrace count: how many terraces the terrain will contain. This number must be greater than zero.
- Fragmentation depth: how many times the basic shape will be fragmented to form the terrain. The larger the value, the greater the level of detail will be (more triangles and vertices) and the longer the generation process takes.
- Deformation settings: a group of settings used to deform the terrain—the process of creating hills and valleys using Perlin noise. These settings include:
	- Maximum height: the maximum distance between the lowest and highest vertices of the terrain.
	- Feature frequency: the number of terrain features (hills and valleys) in a given area. 
	- Height distribution:  the height distribution over the terrain. In short, how low valleys and how high hills should be, and everything in between. It's represented as a curve that must start in (0,0) and end in (1,1). A linear curve is considered a canonical value.

The two usage methods will differ only on how they provide these parameters and how they use the terrain generation output.

### Component-based usage
The easier way to jump into TTG is to use its controller component. To start using it, add the `TerrainGeneratorController` component to a game object. A Mesh Renderer and a Mesh Filter will be added automatically if the game object doesn't contain them yet. The picture below displays an example of the controller on Unity's inspector:

![A view of Unity's inspector showing a component called "Terrain Generator Controller" with several fields](http://matheusamazonas.net/assets/images/ttg/controller.png)
This component contains all parameters explained in the previous section, in addition to the following fields:
- Generate on start: whether a new terrain should be generated on start. This feature is great to quickly test generation parameters.
- Renderer: the `MeshRenderer` that will be used to render the terrain.
- Mesh filter: the `MeshFilter` that will contain the terrain's mesh. 

`TerrainGeneratorController` exposes four generation methods:
```csharp
// For pseudorandom synchronous generation:
public void GenerateTerrain();
// For pseudorandom asynchronous generation:
public async Task GenerateTerrainAsync(CancellationToken token);
// For predictable, synchronous generation:
public void GenerateTerrain(int seed);
// For predictable asynchronous generation:
public async Task GenerateTerrainAsync(int seed, CancellationToken token);
```

The first two methods are meant for pseudorandom procedural generation—when the generated terrain doesn't need to be reproduced in the future. The other methods are meant for reproducible procedural generation—when we would like to generate the exact same terrain in the future. The `seed` parameter will be used to feed the randomizer and it's enough to reproduce an entire terrain. If you're aiming for reproducible terrains, use the second method. The task of generating random seed values is up to the user. The asynchronous methods support task cancellation via a cancellation token. If the token's source is cancelled, a `TaskCanceledException` might be thrown.

The controller manages the lifetime of the meshes it generates, and it destroys them once they're not being used anymore (including when the component itself is destroyed). If you would like to manage mesh lifetime yourself, use the API (described below) instead.


### API usage
The more advanced (and more flexible) way of using TTG is via its API, using the `TerrainGenerator` class. This method is better suited for editor usage and dynamic terrain generation at runtime. Its usage isn't too different from the component-based approach seen above: the generation parameters are the same and there's a 1:1 mapping between generation methods. In fact, the `TerrainGeneratorController` is just a convenient (`MonoBehaviour`) wrapper around `TerrainGenerator`. 

Generating a terraced terrain via the API requires two steps:
- Creating a `TerrainGenerator` instance.
- Calling one of its generation methods.

The generation data is stored on the `TerrainGenerator` instance and it's immutable. That means that once a generator is created, it will always create terrains using the given generation data. This class represents one instance of the terraced terrain generator (there can be multiple) and it has only one constructor:
```csharp
public TerrainGenerator(ushort sides, float radius, DeformationSettings deformationSettings, ushort depth,   
int terraces)
```
The parameters (including the deformation settings) are the same ones explained at the beginning of the [Usage](#usage) section. The deformation settings are grouped in the `DeformationSettings` struct, which exposes two constructors:
```csharp
// For pseudorandom procedural generation
public DeformationSettings(float maximumHeight, float frequency, AnimationCurve heightDistribution)
// For reproducible procedural generation
public DeformationSettings(int seed, float maximumHeight, float frequency, AnimationCurve heightDistribution)
```
Once the `TerrainGenerator` instance is created, we can actually generate terrains using its two generation methods:
```csharp
// For synchronous generation
public Mesh GenerateTerrain();
// For asynchronous generation
public async Task<Mesh> GenerateTerrainAsync(CancellationToken token);
```
The asynchronous method supports task cancellation via a cancellation token. If the token's source is cancelled, a `TaskCanceledException` might be thrown.

It's important to point out that the user is responsible for resource management (in this case, meshes). The API doesn't automatically clean terrain meshes up. 

And that's it. No other types or methods are exposed. The ones above are sufficient to use all TTG features via the API.

#### Example 
Let's say we would like to generate random octagonal terrains with 5 terraces, 20 units radius, maximum height of 9.5 units, a feature frequency of 0.075 and a fragmentation depth of 4. First, let's create the height distribution curve. For real-world usage, it would be handy to serialize this curve so it's easily editable in the inspector. Here, for the sake of simplicity, let's use a linear curve declared in code:
```csharp
var heightDistribution = AnimationCurve.Linear(0f, 0f, 1f, 1f);
```
To create the deformation settings, we first pass the maximum height (9.5), followed by the feature frequency (0.075) and finally the height distribution created on the step above:
```csharp
var deformationSettings = new DeformationSettings(9.5f, 0.075f, heightDistribution);
```
Finally, let's call the `TerrainGenerator` constructor to create the generator, passing the number of sides of the terrain's basic shape (8 for octagon), the radius (20), the previously created deformation settings, the fragmentation depth (4) and the number of terraces (5):
```csharp
var generator = new TerrainGenerator(8, 20, deformationSettings, 4, 5);
```
Once we've constructed a `TerrainGenerator`, we can simply ask it to generate a new terrain. For example, if we would like to generate the terrain synchronously:
```csharp
var mesh = generator.GenerateTerrain();
```
The call to `GenerateTerrain` returns a `Mesh` representing the generated terraced terrain. It is up to the user to manage the lifetime of this mesh. Here's an example of asynchronous terrain generation:
```csharp
var mesh = await generator.GenerateTerrainAsync(token);
```
Once we've got a reference to the terrain mesh, we can use it with a mesh filter:
```csharp
[SerializeField] private MeshFilter _meshFilter;
// ...
_meshFilter.mesh = mesh;
```
The `TerrainGeneratorController`'s  source code (described in the [previous section](#component-based-usage)) offers  great examples of API usage and can server as inspiration to API newcomers.

## Samples
The package contains two samples:
- Randomizer: this sample simply repeatedly creates completely random terraced terrains. It's a great display of the tool's capabilities and the different types of terrains it can create. 
- Parameters test: this sample repeatedly creates random terraced terrains that can be somewhat customized. It's a great tool to quickly test generation parameters. Play with the values in the `TerrainGeneratorController` component attached to the `Generator` game object in the `ParametersTest` scene to see how the parameters affect the generated terrains.

To import the samples, open the Package Manager and select TTG in the packages list. Then find the Samples section on the right panel, and click on the "Import" button right next to the sample you would like to import. Once importing is finished, navigate to the `Assets/Samples/Terraced Terrain Generator`. Finally, open and play the scene from the sample you would like to test.

## Compatibility and dependencies
TTG requires Unity 2021.3.X or above, its target API compatibility level is .NET Standard 2.1, and depends on the following packages:
- `com.unity.collections`

## Roadmap
Although the first version of TTG is out, it's still under (casual) development. The following features are planned in the next versions:
- Custom terrain heights: instead of evenly spacing the terraces between the terrain's lowest and highest points, allow custom heights to be chosen.
- Sphere as a basic shape: let's create completely terraced planets!
- Improve terrain detailing: use Perlin noise octaves to create more natural terrains.
- Real-time sculpting: instead of letting an algorithm generate the hills, let the user interactively sculpt them.
- Outer walls: “close” the generated mesh so it looks like a model carved in wood, sitting on a desk. 

You can follow TTG's development progress on its [Trello board](https://trello.com/b/cFRtgqal/terracted-terrain-generator).

## Technical aspects
Technical aspects of TTG were described in the following blogs posts:

- [Developing a Terraced Terrain Generator](https://www.matheusamazonas.net/blog/2023/04/08/ttg).
- [Terraced Terrain Generator performance improvements](https://www.matheusamazonas.net/blog/2023/04/09/ttg-performance).

## Contributing
If you would like to report e bug, please create an [issue](https://github.com/matheusamazonas/ttg/issues). If you would like to contribute with bug fixing or small improvements, please open a Pull Request. If you would like to contribute with a new feature (regardless ff it's in the roadmap or not),  [contact the developer](https://matheusamazonas.net/contact.html).  

## Getting help
Use the [issues page](https://github.com/matheusamazonas/ttg/issues) if there's a problem with your TTG setup, if something isn't working as expected, or if you would like to ask questions about the tool and its usage.

## License
Terraced Terrain Generator is distributed under the terms of the MIT license. For more information, check the [LICENSE](LICENSE) file in this repository.